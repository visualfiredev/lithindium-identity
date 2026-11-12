// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

using OtpNet;

using Xunit;

namespace Lithindium.Identity.Tests {

    public sealed class ChallengeManagerTests {

        private const string Password = "a sufficiently long password";
        private const string Replacement = "a different strong password";

        [Fact]
        public async Task InvalidProofCountsAndFinalAttemptCanSucceed() {
            var f = await Fixture.CreateAsync(maxAttempts: 2);
            var email = new UserEmail("alice@example.com", true);
            ChallengeTicket ticket = (await f.Challenges.IssueEmailVerificationAsync(f.User, email)).Value;
            Assert.Empty(f.Store.Emails);
            Assert.Equal(32, f.Store.Challenges[ticket.Uuid].SecretHash!.Value.Length);
            Assert.NotNull(f.Store.Challenges[ticket.Uuid].Payload);

            var failed = await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, ticket.Uuid, "wrong");
            Assert.Equal(StatusCode.InvalidCredential, failed.Status);
            Assert.Equal(1u, f.Store.Challenges[ticket.Uuid].Attempts);
            Assert.Empty(f.Store.Emails);
            Assert.True((await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, ticket.Uuid, ticket.Secret)).IsSuccess);
            Assert.Equal(email, Assert.Single(f.Store.Emails));
            Assert.Empty(f.Store.Challenges);
            Assert.Equal(StatusCode.NotFound, (await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, ticket.Uuid, ticket.Secret)).Status);
        }

        [Fact]
        public async Task ExhaustedChallengeRejectsEvenCorrectProof() {
            var f = await Fixture.CreateAsync(maxAttempts: 1);
            var ticket = (await f.Challenges.IssuePhoneVerificationAsync(f.User, new UserPhone("+18015550123", null, true))).Value;
            Assert.Matches("^[0-9]{6}$", ticket.Secret);
            string wrong = ticket.Secret == "000000" ? "000001" : "000000";
            Assert.Equal(StatusCode.InvalidCredential, (await f.Challenges.CompletePhoneVerificationAsync(f.User.Uuid, ticket.Uuid, wrong)).Status);
            Assert.Equal(StatusCode.AttemptsExhausted, (await f.Challenges.CompletePhoneVerificationAsync(f.User.Uuid, ticket.Uuid, ticket.Secret)).Status);
            Assert.Empty(f.Store.Phones);
        }

        [Fact]
        public async Task IssuanceSupersedesAndExpirationIsInclusive() {
            var f = await Fixture.CreateAsync();
            var email = new UserEmail("alice@example.com", false);
            var first = (await f.Challenges.IssueEmailVerificationAsync(f.User, email)).Value;
            var second = (await f.Challenges.IssueEmailVerificationAsync(f.User, email)).Value;
            Assert.NotEqual(first.Uuid, second.Uuid);
            Assert.Equal(StatusCode.NotFound, (await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, first.Uuid, first.Secret)).Status);
            f.Clock.Advance(TimeSpan.FromMinutes(10));
            Assert.Equal(StatusCode.Expired, (await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, second.Uuid, second.Secret)).Status);
            Assert.Equal(1, (await f.Challenges.RemoveExpiredAsync()).Value);
        }

        [Fact]
        public async Task PurposeAndAuthenticationGenerationAreEnforced() {
            var f = await Fixture.CreateAsync();
            var ticket = (await f.Challenges.IssueEmailVerificationAsync(f.User, new UserEmail("alice@example.com", false))).Value;
            var wrong = await f.Challenges.CompletePhoneVerificationAsync(f.User.Uuid, ticket.Uuid, ticket.Secret);
            Assert.Equal(StatusCode.NotFound, wrong.Status);
            Assert.Equal(0u, f.Store.Challenges[ticket.Uuid].Attempts);

            Assert.True((await f.Users.InvalidateAuthenticationAsync(f.User.Uuid)).IsSuccess);
            Assert.Empty(f.Store.Challenges);
            var stale = await f.Challenges.IssueEmailVerificationAsync(f.User, new UserEmail("alice@example.com", false));
            Assert.Equal(StatusCode.Conflict, stale.Result.Status);
        }

        [Fact]
        public async Task CredentialFailureRollsBackConsumptionAndAttempt() {
            var f = await Fixture.CreateAsync();
            var ticket = (await f.Challenges.IssueEmailVerificationAsync(f.User, new UserEmail("alice@example.com", true))).Value;
            f.Store.FailEmailAddition = true;
            var result = await f.Challenges.CompleteEmailVerificationAsync(f.User.Uuid, ticket.Uuid, ticket.Secret);
            Assert.Equal(StatusCode.Conflict, result.Status);
            Assert.Equal(0u, f.Store.Challenges[ticket.Uuid].Attempts);
            Assert.Empty(f.Store.Emails);
            Assert.DoesNotContain(f.Store.Audits, audit => audit.Action == UserAuditAction.Consumed);
        }

        [Fact]
        public async Task ResetRequiresVerifiedDestinationAndRevokesSessions() {
            var f = await Fixture.CreateAsync();
            var email = new UserEmail("alice@example.com", true);
            Assert.Equal(StatusCode.NotFound, (await f.Challenges.IssuePasswordResetAsync(f.User.Uuid, email)).Result.Status);
            f.Store.Emails.Add(email);
            var ticket = (await f.Challenges.IssuePasswordResetAsync(f.User.Uuid, email)).Value;
            var session = new UserSession(Guid.NewGuid(), null, null, null, ImmutableArray.Create("pwd"), null, f.Clock.GetUtcNow().UtcDateTime, f.Clock.GetUtcNow().UtcDateTime.AddHours(1));
            f.Store.Sessions.Add(session.Uuid, session);

            Assert.True((await f.Challenges.CompletePasswordResetAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, Replacement)).IsSuccess);
            User current = f.Store.Users[f.User.Uuid];
            Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<object>().VerifyHashedPassword(current, current.Password!, Replacement));
            Assert.Equal((ushort)1, current.Rotation);
            Assert.Empty(f.Store.Challenges);
            Assert.Empty(f.Store.Sessions);
        }

        [Fact]
        public async Task EnrollmentProtectsKeyAndPreventsImmediateReplay() {
            var f = await Fixture.CreateAsync();
            var ticket = (await f.Challenges.IssueTotpEnrollmentAsync(f.User, "Phone")).Value;
            byte[] key = Base32Encoding.ToBytes(ticket.Secret);
            var totp = new Totp(key);
            string code = totp.ComputeTotp(f.Clock.GetUtcNow().UtcDateTime);
            Assert.True((await f.Challenges.CompleteTotpEnrollmentAsync(f.User.Uuid, ticket.Uuid, code)).IsSuccess);
            UserTotp enrolled = Assert.Single(f.Store.Totps.Values);
            Assert.NotNull(enrolled.LastStep);
            Assert.False(enrolled.Secret.AsSpan().SequenceEqual(key));
            Assert.Empty(f.Store.Challenges);

            var mfa = await f.IssueMfaAsync();
            bool completed = false;
            Task<OperationResult> Complete(User user, UserChallenge challenge, CancellationToken ct) {
                completed = true;
                return Task.FromResult(new OperationResult(StatusCode.Success));
            }
            var replay = await f.Challenges.CompleteMultiFactorAsync(f.User.Uuid, mfa.Uuid, mfa.Secret, enrolled.Uuid, code, Complete, default);
            Assert.Equal(StatusCode.AlreadyUsed, replay.Status);
            Assert.False(completed);
            Assert.Equal(1u, f.Store.Challenges[mfa.Uuid].Attempts);

            // Move beyond both neighboring windows to avoid a collision with the enrollment code.
            f.Clock.Advance(TimeSpan.FromSeconds(60));
            string next = totp.ComputeTotp(f.Clock.GetUtcNow().UtcDateTime);
            Assert.True((await f.Challenges.CompleteMultiFactorAsync(f.User.Uuid, mfa.Uuid, mfa.Secret, enrolled.Uuid, next, Complete, default)).IsSuccess);
            Assert.True(completed);
            Assert.Empty(f.Store.Challenges);
        }

        [Fact]
        public async Task RecoveryCodeConsumptionRollsBackWhenSessionCompletionFails() {
            var f = await Fixture.CreateAsync();
            var enrollment = (await f.Challenges.IssueTotpEnrollmentAsync(f.User)).Value;
            string code = new Totp(Base32Encoding.ToBytes(enrollment.Secret)).ComputeTotp(f.Clock.GetUtcNow().UtcDateTime);
            Assert.True((await f.Challenges.CompleteTotpEnrollmentAsync(f.User.Uuid, enrollment.Uuid, code)).IsSuccess);
            string recovery = (await f.Users.GenerateRecoveryCodesAsync(f.User.Uuid)).Value[0];
            int count = f.Store.RecoveryCodes.Count;
            var ticket = await f.IssueMfaAsync();
            Task<OperationResult> Fail(User user, UserChallenge challenge, CancellationToken ct) =>
                Task.FromResult(new OperationResult(StatusCode.Conflict, operation: OperationCode.StoreAddSession));
            var result = await f.Challenges.CompleteMultiFactorAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, null, recovery, Fail, default);
            Assert.Equal(StatusCode.Conflict, result.Status);
            Assert.Equal(count, f.Store.RecoveryCodes.Count);
            Assert.Equal(0u, f.Store.Challenges[ticket.Uuid].Attempts);

            Task<OperationResult> Success(User user, UserChallenge challenge, CancellationToken ct) =>
                Task.FromResult(new OperationResult(StatusCode.Success));
            Assert.True((await f.Challenges.CompleteMultiFactorAsync(f.User.Uuid, ticket.Uuid, ticket.Secret, null, recovery, Success, default)).IsSuccess);
            Assert.Equal(count - 1, f.Store.RecoveryCodes.Count);
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
            public User User { get; private set; }

            private Fixture(uint maxAttempts) {
                Users = new UserManager(Store, timeProvider: Clock);
                Challenges = new ChallengeManager(Users, new EphemeralDataProtectionProvider(), new ChallengeManagerOptions { MaxAttempts = maxAttempts });
            }

            public static async Task<Fixture> CreateAsync(uint maxAttempts = 5) {
                var fixture = new Fixture(maxAttempts);
                fixture.User = (await fixture.Users.CreateAsync("alice", Password)).Value;
                return fixture;
            }

            public async Task<ChallengeTicket> IssueMfaAsync() {
                await using IUserStoreTransaction transaction = await Store.BeginTransactionAsync();
                User current = Store.Users[User.Uuid];
                ChallengeTicket ticket = (await Challenges.IssueMultiFactorAsync(current, default)).Value;
                Assert.True((await transaction.CommitAsync()).IsSuccess);
                return ticket;
            }

        }

    }

}
