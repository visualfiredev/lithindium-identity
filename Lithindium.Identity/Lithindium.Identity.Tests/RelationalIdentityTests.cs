// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Lithindium.Identity.EntityFrameworkCore;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OtpNet;

using Xunit;

namespace Lithindium.Identity.Tests {

    public sealed class RelationalIdentityTests {

        private const string Password = "a sufficiently long password";
        private const string Replacement = "a different strong password";

        [Theory]
        [InlineData("sqlite")]
        [InlineData("postgres")]
        public async Task FullCredentialLifecycleAcrossScopes(string provider) {
            await using Fixture? fixture = await Fixture.CreateAsync(provider);
            if (fixture is null) return; // PostgreSQL is exercised when configured, including CI.
            Guid uuid;
            string currentToken;
            string otherToken;
            await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope()) {
                var users = scope.ServiceProvider.GetRequiredService<UserManager>();
                var challenges = scope.ServiceProvider.GetRequiredService<ChallengeManager>();
                User user = (await users.CreateAsync("Alice", Password)).Value;
                uuid = user.Uuid;
                var email = new UserEmail("alice@example.com", true);
                var issued = (await challenges.IssueEmailVerificationAsync(user, email)).Value;
                Assert.Equal(StatusCode.InvalidCredential, (await challenges.CompleteEmailVerificationAsync(uuid, issued.Uuid, "wrong")).Status);
                Assert.True((await challenges.CompleteEmailVerificationAsync(uuid, issued.Uuid, issued.Secret)).IsSuccess);
                user = (await users.FindByUuidAsync(uuid))!.Value;
                var phone = new UserPhone("+18015550123", "12", true);
                issued = (await challenges.IssuePhoneVerificationAsync(user, phone)).Value;
                Assert.True((await challenges.CompletePhoneVerificationAsync(uuid, issued.Uuid, issued.Secret)).IsSuccess);
            }
            await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope()) {
                var signIn = scope.ServiceProvider.GetRequiredService<SignInManager>();
                currentToken = (await signIn.EmailSignInAsync("ALICE@example.com", Password)).Value.SessionToken!;
                otherToken = (await signIn.PhoneSignInAsync("+18015550123", "12", Password)).Value.SessionToken!;
                Assert.True((await signIn.ValidateSessionAsync(currentToken)).HasValue);
            }
            await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope()) {
                var users = scope.ServiceProvider.GetRequiredService<UserManager>();
                var signIn = scope.ServiceProvider.GetRequiredService<SignInManager>();
                var current = (await signIn.ValidateSessionAsync(currentToken)).Value;
                Assert.True((await users.ChangePasswordAsync(uuid, Password, Replacement, current.Session.Uuid)).IsSuccess);
                Assert.True((await signIn.ValidateSessionAsync(currentToken)).HasValue);
                Assert.False((await signIn.ValidateSessionAsync(otherToken)).HasValue);
                Assert.False((await signIn.PasswordSignInAsync("alice", Password)).HasValue);

                var challenges = scope.ServiceProvider.GetRequiredService<ChallengeManager>();
                User user = (await users.FindByUuidAsync(uuid))!.Value;
                var enrollment = (await challenges.IssueTotpEnrollmentAsync(user)).Value;
                var totp = new Totp(Base32Encoding.ToBytes(enrollment.Secret));
                Assert.True((await challenges.CompleteTotpEnrollmentAsync(uuid, enrollment.Uuid, totp.ComputeTotp(fixture.Clock.GetUtcNow().UtcDateTime))).IsSuccess);
                string recovery = (await users.GenerateRecoveryCodesAsync(uuid)).Value[0];
                var pending = (await signIn.PasswordSignInAsync("alice", Replacement)).Value;
                Assert.False(pending.IsAuthenticated);
                var challenge = pending.Challenge!;
                var completed = (await signIn.CompleteRecoveryCodeAsync(uuid, challenge.Uuid, challenge.Secret, recovery)).Value;
                Assert.True(completed.IsAuthenticated);
                Assert.Equal(9, (await users.CountRecoveryCodesAsync(uuid)).Value);
                Assert.False((await signIn.CompleteRecoveryCodeAsync(uuid, challenge.Uuid, challenge.Secret, recovery)).HasValue);

                var reset = (await challenges.IssuePasswordResetAsync(uuid, new UserEmail("alice@example.com", true))).Value;
                Assert.True((await challenges.CompletePasswordResetAsync(uuid, reset.Uuid, reset.Secret, Password)).IsSuccess);
                Assert.False((await signIn.ValidateSessionAsync(completed.SessionToken!)).HasValue);
                Assert.False((await signIn.PasswordSignInAsync("alice", Password)).Value.IsAuthenticated);
            }
        }

        [Theory]
        [InlineData("sqlite")]
        [InlineData("postgres")]
        public async Task OwnershipUniquenessPrimaryAndRollback(string provider) {
            await using Fixture? fixture = await Fixture.CreateAsync(provider);
            if (fixture is null) return;
            await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager>();
            var store = scope.ServiceProvider.GetRequiredService<EntityFrameworkUserStore>();
            User alice = (await users.CreateAsync("Alice", Password)).Value;
            User bob = (await users.CreateAsync("Bob", Password)).Value;
            Assert.Equal(StatusCode.AlreadyExists, (await users.CreateAsync("ALICE", Password)).Result.Status);
            var first = new UserEmail("first@example.com", true);
            var second = new UserEmail("second@example.com", true);
            Assert.True((await store.AddEmailAsync(alice, first)).IsSuccess);
            Assert.True((await store.AddEmailAsync(alice, second)).IsSuccess);
            var contacts = (await users.GetEmailsAsync(alice.Uuid)).Value;
            Assert.Equal(second.Address, Assert.Single(contacts.Where(e => e.Primary)).Address);
            Assert.Equal(StatusCode.AlreadyExists, (await store.AddEmailAsync(bob, new UserEmail("FIRST@example.com", false))).Status);
            Assert.Equal(StatusCode.NotFound, (await store.RemoveEmailAsync(bob, first)).Status);
            Assert.Equal(StatusCode.NotFound, (await store.SetPrimaryEmailAsync(bob, first)).Status);

            await using (IUserStoreTransaction transaction = await store.BeginTransactionAsync()) {
                Assert.True((await store.SetPrimaryEmailAsync(alice, first)).IsSuccess);
                Assert.True((await store.TryUpdateAsync(alice, alice with { DisplayName = "Uncommitted" })).IsSuccess);
                // Roll back both changes on disposal.
            }
            Assert.Equal(second.Address, Assert.Single((await users.GetEmailsAsync(alice.Uuid)).Value.Where(e => e.Primary)).Address);
            Assert.Equal(alice, (await store.FindByUuidAsync(alice.Uuid))!.Value);

            Assert.True((await store.TryUpdateAsync(alice, alice with { DisplayName = "New" })).IsSuccess);
            Assert.Equal(StatusCode.Conflict, (await store.TryUpdateAsync(alice, alice with { DisplayName = "Stale" })).Status);

            var external = new UserExternalIdentity(Guid.NewGuid(), null, "https://issuer.example", "CaseSensitive", null);
            Assert.True((await store.AddExternalIdentityAsync(alice, external)).IsSuccess);
            Assert.Equal(alice.Uuid, (await store.FindByExternalIdentityAsync(external.Issuer, external.Subject))!.Value.Uuid);
            Assert.Null(await store.FindByExternalIdentityAsync(external.Issuer, "casesensitive"));
            Assert.Equal(StatusCode.AlreadyExists, (await store.AddExternalIdentityAsync(bob, external with { Uuid = Guid.NewGuid() })).Status);
        }

        [Theory]
        [InlineData("sqlite")]
        [InlineData("postgres")]
        public async Task IdentifiersCannotBeReusedAndAuditsSurviveDeletion(string provider) {
            await using Fixture? fixture = await Fixture.CreateAsync(provider);
            if (fixture is null) return;
            await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager>();
            var signIn = scope.ServiceProvider.GetRequiredService<SignInManager>();
            var challenges = scope.ServiceProvider.GetRequiredService<ChallengeManager>();
            var store = scope.ServiceProvider.GetRequiredService<EntityFrameworkUserStore>();
            User user = (await users.CreateAsync("Alice", Password)).Value;
            var session = (await signIn.PasswordSignInAsync("Alice", Password)).Value;
            Assert.True((await signIn.SignOutAsync(session.SessionToken!)).IsSuccess);
            await using (IUserStoreTransaction transaction = await store.BeginTransactionAsync()) {
                Assert.Equal(StatusCode.AlreadyExists, (await store.AddSessionAsync(user, session.Session!.Value.Session)).Status);
            }
            var ticket = (await challenges.IssueEmailVerificationAsync(user, new UserEmail("alice@example.com", true))).Value;
            UserChallenge pending = (await store.GetChallengeAsync(user, ticket.Uuid))!.Value;
            Assert.True((await challenges.CancelAsync(user.Uuid, ticket.Uuid)).IsSuccess);
            await using (IUserStoreTransaction transaction = await store.BeginTransactionAsync()) {
                Assert.Equal(StatusCode.AlreadyExists, (await store.AddChallengeAsync(user, pending)).Status);
            }

            Assert.True((await users.DeleteAsync(user.Uuid)).IsSuccess);
            Assert.Null(await store.FindByUuidAsync(user.Uuid));
            Assert.Equal(StatusCode.AlreadyExists, (await store.CreateAsync(user)).Status);
            int audits = 0;
            await foreach (UserAudit audit in store.GetAuditsAsync(user.Uuid)) audits++;
            Assert.True(audits >= 5);
        }

        [Theory]
        [InlineData("sqlite")]
        [InlineData("postgres")]
        public async Task ConcurrentRecoveryCodeConsumptionHasOneWinner(string provider) {
            await using Fixture? fixture = await Fixture.CreateAsync(provider);
            if (fixture is null) return;
            User user;
            var hash = ImmutableArray.CreateRange(new byte[32]);
            await using (AsyncServiceScope scope = fixture.Services.CreateAsyncScope()) {
                var store = scope.ServiceProvider.GetRequiredService<EntityFrameworkUserStore>();
                user = (await scope.ServiceProvider.GetRequiredService<UserManager>().CreateAsync("Alice", Password)).Value;
                Assert.True((await store.ReplaceRecoveryCodesAsync(user, new[] { new UserRecoveryCode(Guid.NewGuid(), hash) })).IsSuccess);
            }

            async Task<OperationResult> ConsumeAsync() {
                await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
                var store = scope.ServiceProvider.GetRequiredService<EntityFrameworkUserStore>();
                return await store.TryConsumeRecoveryCodeAsync(user, hash);
            }
            OperationResult[] results = await Task.WhenAll(Task.Run(ConsumeAsync), Task.Run(ConsumeAsync));
            Assert.Single(results.Where(r => r.IsSuccess));
            Assert.All(results.Where(r => r.IsFailure), r => Assert.True(r.Status is StatusCode.NotFound or StatusCode.Conflict));
            await using AsyncServiceScope verifyScope = fixture.Services.CreateAsyncScope();
            Assert.Equal(0, await verifyScope.ServiceProvider.GetRequiredService<EntityFrameworkUserStore>().CountRecoveryCodesAsync(user));
        }

        [Fact]
        public async Task ScopedRegistrationSharesStoreAndPreservesConfiguredOptions() {
            await using Fixture? fixture = await Fixture.CreateAsync("sqlite");
            await using AsyncServiceScope scope = fixture!.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;
            Assert.Same(services.GetRequiredService<IUserStore>(), services.GetRequiredService<EntityFrameworkUserStore>());
            Assert.Same(services.GetRequiredService<UserManager>(), services.GetRequiredService<ChallengeManager>().Users);
            Assert.Same(services.GetRequiredService<UserManager>(), services.GetRequiredService<UserManager>());
            Assert.Equal(10, services.GetRequiredService<UserManagerOptions>().RecoveryCodeCount);
            await using AsyncServiceScope second = fixture.Services.CreateAsyncScope();
            Assert.NotSame(services.GetRequiredService<IUserStore>(), second.ServiceProvider.GetRequiredService<IUserStore>());
        }

        private sealed class FixedClock : TimeProvider {
            public override DateTimeOffset GetUtcNow() => new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        }

        private sealed class Fixture : IAsyncDisposable {

            public ServiceProvider Services { get; }
            public TimeProvider Clock { get; }

            private Fixture(ServiceProvider services, TimeProvider clock) {
                Services = services;
                Clock = clock;
            }

            public static async Task<Fixture?> CreateAsync(string provider) {
                string connection;
                if (provider == "postgres") {
                    string? configured = Environment.GetEnvironmentVariable("LITHINDIUM_TEST_POSTGRES");
                    if (string.IsNullOrEmpty(configured)) return null;
                    connection = new NpgsqlConnectionStringBuilder(configured) {
                        Database = "lithindium_test_" + Guid.NewGuid().ToString("N"), Pooling = false
                    }.ConnectionString;
                } else {
                    connection = "Data Source=" + Path.Combine(Path.GetTempPath(), "lithindium-" + Guid.NewGuid().ToString("N") + ".db") + ";Foreign Keys=True;Pooling=False";
                }
                var services = new ServiceCollection();
                var clock = new FixedClock();
                services.AddSingleton<TimeProvider>(clock);
                services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
                services.AddLogging();
                services.AddLithindiumEntityFrameworkCore(options => {
                    if (provider == "postgres") options.UseNpgsql(connection);
                    else options.UseSqlite(connection);
                });
                services.AddLithindiumIdentity(users: new UserManagerOptions { RecoveryCodeCount = 10 });
                var built = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
                await using (AsyncServiceScope scope = built.CreateAsyncScope()) {
                    await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();
                }
                return new Fixture(built, clock);
            }

            public async ValueTask DisposeAsync() {
                await using (AsyncServiceScope scope = Services.CreateAsyncScope()) {
                    await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureDeletedAsync();
                }
                await Services.DisposeAsync();
            }

        }

    }

}
