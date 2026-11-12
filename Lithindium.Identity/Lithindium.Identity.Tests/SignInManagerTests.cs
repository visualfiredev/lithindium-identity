// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;

using OtpNet;

using Xunit;

namespace Lithindium.Identity.Tests {

    public sealed class SignInManagerTests {

        private const string Password = "a sufficiently long password";
        private const string Replacement = "a different strong password";

        [Fact]
        public async Task PasswordSignInIssuesProtectedSessionAndSignOutRevokesIt() {
            var f = await Fixture.CreateAsync();
            var result = await f.SignIn.PasswordSignInAsync("alice", Password, new SessionMetadata { DeviceName = "Desktop", IpAddress = "127.0.0.1" });
            Assert.True(result.HasValue);
            Assert.True(result.Value.IsAuthenticated);
            Assert.Null(result.Value.Challenge);
            var session = result.Value.Session!.Value.Session;
            Assert.Equal("pwd", Assert.Single(session.Amr));
            Assert.Equal("Desktop", session.DeviceName);
            Assert.Equal("127.0.0.1", session.IpAddress);
            Assert.Single(f.Store.Sessions);
            Assert.Contains(f.Store.Audits, a => a.Type == UserAuditType.Session && a.Action == UserAuditAction.Created);
            string token = result.Value.SessionToken!;
            Assert.True((await f.SignIn.ValidateSessionAsync(token)).HasValue);
            Assert.False((await f.SignIn.ValidateSessionAsync(session.Uuid.ToString())).HasValue);
            Assert.False((await f.SignIn.ValidateSessionAsync("not a token")).HasValue);
            Assert.True((await f.SignIn.SignOutAsync(token)).IsSuccess);
            Assert.Empty(f.Store.Sessions);
            Assert.False((await f.SignIn.ValidateSessionAsync(token)).HasValue);
            Assert.Equal(StatusCode.NoOp, (await f.SignIn.SignOutAsync(token)).Status);
        }

        [Fact]
        public async Task ExpirationIsInclusiveAndCleanupRemovesExpiredSessions() {
            var f = await Fixture.CreateAsync();
            string token = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value.SessionToken!;
            f.Clock.Advance(TimeSpan.FromDays(14));
            Assert.Equal(StatusCode.Expired, (await f.SignIn.ValidateSessionAsync(token)).Result.Status);
            Assert.Equal(1, (await f.SignIn.RemoveExpiredSessionsAsync()).Value);
            Assert.Empty(f.Store.Sessions);
        }

        [Fact]
        public async Task PasswordFailuresCommitLockoutAndSuccessfulSignInResetsIt() {
            var f = await Fixture.CreateAsync(maxFailures: 2);
            Assert.Equal(StatusCode.InvalidCredential, (await f.SignIn.PasswordSignInAsync("alice", "wrong")).Result.Status);
            Assert.Equal(1u, f.Store.Lockouts[f.User.Uuid].Failures);
            Assert.Equal(StatusCode.InvalidCredential, (await f.SignIn.PasswordSignInAsync("alice", "wrong")).Result.Status);
            Assert.Equal(2u, f.Store.Lockouts[f.User.Uuid].Failures);
            Assert.Equal(f.Clock.GetUtcNow().UtcDateTime.AddMinutes(15), f.Store.Lockouts[f.User.Uuid].ReleaseAt);
            Assert.Equal((byte)DetailCode.LockedOut, (await f.SignIn.PasswordSignInAsync("alice", Password)).Result.Detail);
            Assert.Empty(f.Store.Sessions);
            f.Clock.Advance(TimeSpan.FromMinutes(15));
            Assert.True((await f.SignIn.PasswordSignInAsync("alice", Password)).Value.IsAuthenticated);
            Assert.Equal(0u, f.Store.Lockouts[f.User.Uuid].Failures);
            Assert.Null(f.Store.Lockouts[f.User.Uuid].ReleaseAt);
        }

        [Fact]
        public async Task UnknownAndIncorrectCredentialsReturnSameResult() {
            var f = await Fixture.CreateAsync();
            var missing = await f.SignIn.PasswordSignInAsync("missing", Password);
            var invalid = await f.SignIn.PasswordSignInAsync("alice", "wrong");
            Assert.Equal(missing.Result, invalid.Result);
            Assert.Empty(f.Store.Sessions);
        }

        [Fact]
        public async Task PasswordChangePreservesAuthorizedTokenWithoutRotationBinding() {
            var f = await Fixture.CreateAsync();
            var current = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value;
            var other = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value;
            Assert.True((await f.Users.ChangePasswordAsync(f.User.Uuid, Password, Replacement, current.Session!.Value.Session.Uuid)).IsSuccess);
            Assert.True((await f.SignIn.ValidateSessionAsync(current.SessionToken!)).HasValue);
            Assert.False((await f.SignIn.ValidateSessionAsync(other.SessionToken!)).HasValue);
            Assert.True((await f.Users.SetStatusAsync(f.User.Uuid, UserStatus.Suspended)).IsSuccess);
            Assert.False((await f.SignIn.ValidateSessionAsync(current.SessionToken!)).HasValue);
            Assert.Equal((byte)DetailCode.Suspended, (await f.SignIn.PasswordSignInAsync("alice", Replacement)).Result.Detail);
        }

        [Fact]
        public async Task VerifiedContactAndExternalSignInUseLinkedIdentity() {
            var f = await Fixture.CreateAsync();
            f.Store.Emails.Add(new UserEmail("alice@example.com", true));
            f.Store.Phones.Add(new UserPhone("+18015550123", null, true));
            var identity = new UserExternalIdentity(Guid.NewGuid(), null, "https://issuer.example", "Subject", null);
            Assert.True((await f.Users.LinkExternalIdentityAsync(f.User.Uuid, identity)).IsSuccess);
            Assert.True((await f.SignIn.EmailSignInAsync("alice@example.com", Password)).Value.IsAuthenticated);
            Assert.True((await f.SignIn.PhoneSignInAsync("+18015550123", null, Password)).Value.IsAuthenticated);
            Assert.False((await f.SignIn.ExternalSignInAsync(identity.Issuer, "subject")).HasValue);
            var external = (await f.SignIn.ExternalSignInAsync(identity.Issuer, identity.Subject)).Value;
            Assert.Equal("federated", Assert.Single(external.Session!.Value.Session.Amr));
        }

        [Fact]
        public async Task EnrolledMfaRequiresSecondFactorAndBindsFirstFactor() {
            var f = await Fixture.CreateAsync();
            Totp totp = await f.EnrollAsync();
            var identity = new UserExternalIdentity(Guid.NewGuid(), null, "https://issuer.example", "Subject", null);
            Assert.True((await f.Users.LinkExternalIdentityAsync(f.User.Uuid, identity)).IsSuccess);
            var first = await f.SignIn.ExternalSignInAsync(identity.Issuer, identity.Subject);
            Assert.Equal((byte)DetailCode.MultiFactorRequired, first.Result.Detail);
            Assert.False(first.Value.IsAuthenticated);
            Assert.Null(first.Value.SessionToken);
            Assert.Empty(f.Store.Sessions);
            var ticket = first.Value.Challenge!;
            var authenticator = Assert.Single(f.Store.Totps.Values);
            f.Clock.Advance(TimeSpan.FromSeconds(60));
            string code = totp.ComputeTotp(f.Clock.GetUtcNow().UtcDateTime);
            var completed = await f.SignIn.CompleteTotpAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, authenticator.Uuid, code);
            Assert.True(completed.Value.IsAuthenticated);
            Assert.Equal(new[] { "federated", "otp", "mfa" }, completed.Value.Session!.Value.Session.Amr.ToArray());
            Assert.Empty(f.Store.Challenges);
            Assert.Single(f.Store.Sessions);
            Assert.False((await f.SignIn.CompleteTotpAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, authenticator.Uuid, code)).HasValue);
        }

        [Fact]
        public async Task InvalidContinuationDoesNotCountAgainstAccountButBadFactorDoes() {
            var f = await Fixture.CreateAsync(maxFailures: 1);
            await f.EnrollAsync();
            var ticket = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value.Challenge!;
            var authenticator = Assert.Single(f.Store.Totps.Values);
            var invalid = await f.SignIn.CompleteTotpAsync(f.User.Uuid, ticket.Uuid, "wrong", authenticator.Uuid, "invalid");
            Assert.Equal(StatusCode.InvalidCredential, invalid.Result.Status);
            Assert.Equal(0u, f.Store.Lockouts[f.User.Uuid].Failures);
            Assert.Equal(1u, f.Store.Challenges[ticket.Uuid].Attempts);
            invalid = await f.SignIn.CompleteTotpAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, authenticator.Uuid, "invalid");
            Assert.Equal(StatusCode.InvalidCredential, invalid.Result.Status);
            Assert.Equal(1u, f.Store.Lockouts[f.User.Uuid].Failures);
            Assert.Equal(2u, f.Store.Challenges[ticket.Uuid].Attempts);
            Assert.Equal((byte)DetailCode.LockedOut, (await f.SignIn.PasswordSignInAsync("alice", Password)).Result.Detail);
            Assert.Empty(f.Store.Sessions);
        }

        [Fact]
        public async Task RecoveryCodeAndSessionCommitTogether() {
            var f = await Fixture.CreateAsync();
            await f.EnrollAsync();
            string code = (await f.Users.GenerateRecoveryCodesAsync(f.User.Uuid)).Value[0];
            int count = f.Store.RecoveryCodes.Count;
            var ticket = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value.Challenge!;
            f.Store.FailSessionAddition = true;
            var failed = await f.SignIn.CompleteRecoveryCodeAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, code);
            Assert.Equal(StatusCode.Conflict, failed.Result.Status);
            Assert.Empty(f.Store.Sessions);
            Assert.Equal(count, f.Store.RecoveryCodes.Count);
            Assert.Equal(0u, f.Store.Challenges[ticket.Uuid].Attempts);
            f.Store.FailSessionAddition = false;
            var completed = await f.SignIn.CompleteRecoveryCodeAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, code);
            Assert.True(completed.Value.IsAuthenticated);
            Assert.Equal(new[] { "pwd", "recovery", "mfa" }, completed.Value.Session!.Value.Session.Amr.ToArray());
            Assert.Equal(count - 1, f.Store.RecoveryCodes.Count);
            Assert.Empty(f.Store.Challenges);
        }

        [Fact]
        public async Task BulkRevocationCancelsPendingMfa() {
            var f = await Fixture.CreateAsync();
            await f.EnrollAsync();
            var ticket = (await f.SignIn.PasswordSignInAsync("alice", Password)).Value.Challenge!;
            Assert.True((await f.SignIn.RevokeSessionsAsync(f.User.Uuid)).IsSuccess);
            Assert.Empty(f.Store.Challenges);
            Assert.False((await f.SignIn.CompleteRecoveryCodeAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, "invalid")).HasValue);
        }

        [Fact]
        public async Task RequiredMfaCannotBeBypassedByAnUnenrolledAccount() {
            var f = await Fixture.CreateAsync(requireMfa: true);
            var result = await f.SignIn.PasswordSignInAsync("alice", Password);
            Assert.Equal(StatusCode.PolicyViolation, result.Result.Status);
            Assert.Empty(f.Store.Sessions);
            Assert.Empty(f.Store.Challenges);
        }

        private sealed class TestClock : TimeProvider {

            private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            public override DateTimeOffset GetUtcNow() => _now;
            public void Advance(TimeSpan duration) => _now += duration;

        }

        private sealed class Fixture {

            public UserManagerTests.TestStore Store { get; } = new();
            public TestClock Clock { get; } = new();
            public UserManager Users { get; }
            public ChallengeManager Challenges { get; }
            public SignInManager SignIn { get; }
            public User User { get; private set; }

            private Fixture(uint maxFailures, bool requireMfa) {
                Users = new UserManager(Store, timeProvider: Clock);
                Challenges = new ChallengeManager(Users, new EphemeralDataProtectionProvider());
                SignIn = new SignInManager(Users, Challenges, new SignInManagerOptions { MaxFailures = maxFailures, RequireMultiFactor = requireMfa });
            }

            public static async Task<Fixture> CreateAsync(uint maxFailures = 5, bool requireMfa = false) {
                var fixture = new Fixture(maxFailures, requireMfa);
                fixture.User = (await fixture.Users.CreateAsync("alice", Password)).Value;
                return fixture;
            }

            public async Task<Totp> EnrollAsync() {
                var ticket = (await Challenges.IssueTotpEnrollmentAsync(Store.Users[User.Uuid])).Value;
                var totp = new Totp(Base32Encoding.ToBytes(ticket.Secret));
                Assert.True((await Challenges.CompleteTotpEnrollmentAsync(User.Uuid, ticket.Uuid, totp.ComputeTotp(Clock.GetUtcNow().UtcDateTime))).IsSuccess);
                return totp;
            }

        }

    }

}
