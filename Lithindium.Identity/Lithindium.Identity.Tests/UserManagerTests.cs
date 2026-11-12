// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using Xunit;

namespace Lithindium.Identity.Tests {

    public sealed class UserManagerTests {

        private const string Password = "a sufficiently long password";
        private const string Replacement = "a different strong password";

        [Fact]
        public async Task CreateHashesPasswordAndCommitsAudit() {
            var store = new TestStore();
            var manager = new UserManager(store);

            OperationResult<User> result = await manager.CreateAsync("alice", Password);

            Assert.True(result.HasValue);
            Assert.Equal(OperationCode.UserCreateUser, result.Result.Operation);
            Assert.NotEqual(Password, result.Value.Password);
            Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<object>().VerifyHashedPassword(result.Value, result.Value.Password!, Password));
            Assert.Equal(result.Value, store.Users[result.Value.Uuid]);
            Assert.Single(store.Audits);
            Assert.Equal(UserAuditAction.Created, store.Audits[0].Action);
        }

        [Fact]
        public async Task ValidationDoesNotWriteAndDuplicateIsReported() {
            var store = new TestStore();
            var manager = new UserManager(store);

            var invalid = await manager.CreateAsync("alice", "short");
            Assert.Equal(StatusCode.PolicyViolation, invalid.Result.Status);
            Assert.Equal((byte)DetailCode.TooShort, invalid.Result.Detail);
            Assert.Empty(store.Users);

            Assert.True((await manager.CreateAsync("alice", Password)).HasValue);
            var duplicate = await manager.CreateAsync("ALICE", Password);
            Assert.Equal(StatusCode.AlreadyExists, duplicate.Result.Status);
            Assert.Equal(ContextCode.Username, duplicate.Result.Context);
            Assert.Single(store.Users);
            Assert.Single(store.Audits);
        }

        [Fact]
        public async Task PasswordChangePreservesOnlyAuthorizedSession() {
            var store = new TestStore();
            var manager = new UserManager(store);
            User user = (await manager.CreateAsync("alice", Password)).Value;
            UserSession current = NewSession();
            UserSession other = NewSession();
            store.Sessions.Add(current.Uuid, current);
            store.Sessions.Add(other.Uuid, other);
            UserChallenge challenge = NewChallenge();
            store.Challenges.Add(challenge.Uuid, challenge);

            OperationResult result = await manager.ChangePasswordAsync(user.Uuid, Password, Replacement, current.Uuid);

            Assert.True(result.IsSuccess);
            Assert.Equal((ushort)1, store.Users[user.Uuid].Rotation);
            Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<object>().VerifyHashedPassword(user, store.Users[user.Uuid].Password!, Replacement));
            Assert.Single(store.Sessions);
            Assert.True(store.Sessions.ContainsKey(current.Uuid));
            Assert.Empty(store.Challenges);
            Assert.Single(store.Audits); // Password changes are not audited.
        }

        [Fact]
        public async Task FailedRevocationRollsBackPasswordAndChallenges() {
            var store = new TestStore();
            var manager = new UserManager(store);
            User user = (await manager.CreateAsync("alice", Password)).Value;
            UserSession session = NewSession();
            store.Sessions.Add(session.Uuid, session);
            UserChallenge challenge = NewChallenge();
            store.Challenges.Add(challenge.Uuid, challenge);
            store.FailSessionRemoval = true;

            OperationResult result = await manager.ChangePasswordAsync(user.Uuid, Password, Replacement);

            Assert.Equal(StatusCode.Conflict, result.Status);
            Assert.Equal(user, store.Users[user.Uuid]);
            Assert.Single(store.Sessions);
            Assert.Single(store.Challenges);
        }

        [Fact]
        public async Task ExternalAccountCannotLoseItsLastCredential() {
            var store = new TestStore();
            var manager = new UserManager(store);
            var external = new UserExternalIdentity(Guid.NewGuid(), null, "https://issuer.example", "subject", null);
            User user = (await manager.CreateExternalAsync("alice", external)).Value;
            Assert.Null(user.Password);

            var denied = await manager.RemoveExternalIdentityAsync(user.Uuid, external.Uuid);
            Assert.Equal(StatusCode.PolicyViolation, denied.Status);
            Assert.Equal((byte)DetailCode.LastCredential, denied.Detail);
            Assert.Single(store.Identities);

            Assert.True((await manager.SetPasswordAsync(user.Uuid, Password)).IsSuccess);
            Assert.True((await manager.RemoveExternalIdentityAsync(user.Uuid, external.Uuid)).IsSuccess);
            Assert.Empty(store.Identities);
            denied = await manager.RemovePasswordAsync(user.Uuid, Password);
            Assert.Equal((byte)DetailCode.LastCredential, denied.Detail);
            Assert.NotNull(store.Users[user.Uuid].Password);
        }

        [Fact]
        public async Task AuditFailureRollsBackCreation() {
            var store = new TestStore { FailAudit = true };
            var manager = new UserManager(store);
            var result = await manager.CreateAsync("alice", Password);

            Assert.False(result.HasValue);
            Assert.Empty(store.Users);
            Assert.Empty(store.Audits);
        }

        [Fact]
        public async Task ExhaustedRotationDoesNotWrapAndSuspensionStillWorks() {
            var store = new TestStore();
            var manager = new UserManager(store);
            User user = (await manager.CreateAsync("alice", Password)).Value;
            store.Users[user.Uuid] = user with { Rotation = ushort.MaxValue };

            var result = await manager.ChangePasswordAsync(user.Uuid, Password, Replacement);
            Assert.Equal((byte)DetailCode.RotationExhausted, result.Detail);
            Assert.Equal(ushort.MaxValue, store.Users[user.Uuid].Rotation);

            Assert.True((await manager.SetStatusAsync(user.Uuid, UserStatus.Suspended)).IsSuccess);
            Assert.Equal(UserStatus.Suspended, store.Users[user.Uuid].Status);
            Assert.Equal(ushort.MaxValue, store.Users[user.Uuid].Rotation);
            Assert.True((await manager.SetStatusAsync(user.Uuid, UserStatus.Active)).IsFailure);
        }

        [Fact]
        public async Task ProfileNoOpDoesNotAudit() {
            var store = new TestStore();
            var manager = new UserManager(store);
            User user = (await manager.CreateAsync("alice", Password)).Value;

            Assert.Equal(StatusCode.NoOp, (await manager.UpdateProfileAsync(user.Uuid, user.Username, user.DisplayName)).Status);
            Assert.Single(store.Audits);
        }

        private static UserSession NewSession() => new(Guid.NewGuid(), null, null, null, ImmutableArray.Create("pwd"), null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        private static UserChallenge NewChallenge() => new(Guid.NewGuid(), UserChallengeType.PasswordReset, null, null, 0, 5, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5));

        // Single-user transactional fake; not a concurrency or provider conformance test.
        internal sealed class TestStore : IUserStore, IUserSessionStore, IUserChallengeStore, IUserAuditStore, IUserExternalIdentityStore, IUserEmailStore, IUserPhoneStore, IUserTotpStore, IUserRecoveryCodeStore {

            public Dictionary<Guid, User> Users { get; private set; } = [];
            public Dictionary<Guid, UserSession> Sessions { get; private set; } = [];
            public Dictionary<Guid, UserChallenge> Challenges { get; private set; } = [];
            public Dictionary<Guid, UserExternalIdentity> Identities { get; private set; } = [];
            public List<UserEmail> Emails { get; private set; } = [];
            public List<UserPhone> Phones { get; private set; } = [];
            public Dictionary<Guid, UserTotp> Totps { get; private set; } = [];
            public List<UserRecoveryCode> RecoveryCodes { get; private set; } = [];
            public bool FailEmailAddition { get; set; }
            private HashSet<Guid> Reserved { get; } = [];

            public List<UserAudit> Audits { get; private set; } = [];
            public bool FailSessionRemoval { get; set; }
            public bool FailAudit { get; init; }

            private static Task<OperationResult> Ok(OperationCode operation) => Task.FromResult(new OperationResult(StatusCode.Success, operation: operation));
            private static Task<OperationResult> Fail(StatusCode status, ContextCode context, OperationCode operation) => Task.FromResult(new OperationResult(status, context, operation));

            public Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) {
                Reserved.Clear();
                return Task.FromResult<IUserStoreTransaction>(new Transaction(this));
            }

            public Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default) =>
                Task.FromResult(Users.TryGetValue(uuid, out User user) ? (User?)user : null);

            public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
                Task.FromResult(Users.Values.Where(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)).Select(u => (User?)u).FirstOrDefault());

            public Task<OperationResult> CreateAsync(User user, CancellationToken cancellationToken = default) {
                if (Users.Values.Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase)))
                    return Fail(StatusCode.AlreadyExists, ContextCode.Username, OperationCode.StoreCreateUser);
                Users.Add(user.Uuid, user);
                return Ok(OperationCode.StoreCreateUser);
            }

            public Task<OperationResult> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default) {
                if (!Users.TryGetValue(user.Uuid, out User current) || current != expected)
                    return Fail(StatusCode.Conflict, ContextCode.None, OperationCode.StoreUpdateUser);
                Users[user.Uuid] = user;
                return Ok(OperationCode.StoreUpdateUser);
            }

            public Task<OperationResult> DeleteAsync(Guid uuid, CancellationToken cancellationToken = default) {
                Users.Remove(uuid);
                Sessions.Clear();
                Challenges.Clear();
                Identities.Clear();
                Emails.Clear();
                Phones.Clear();
                Totps.Clear();
                RecoveryCodes.Clear();
                return Ok(OperationCode.StoreDeleteUser);
            }

            public IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default) => Stream(Sessions.Values);

            public Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default) =>
                Task.FromResult(Sessions.TryGetValue(uuid, out UserSession session) ? (UserSession?)session : null);

            public Task<OperationResult> AddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default) {
                Sessions.Add(session.Uuid, session);
                return Ok(OperationCode.StoreAddSession);
            }

            public Task<OperationResult> RemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default) {
                Sessions.Remove(session.Uuid);
                return Ok(OperationCode.StoreRemoveSession);
            }

            public Task<OperationResult> RemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default) {
                if (FailSessionRemoval) return Fail(StatusCode.Conflict, ContextCode.None, OperationCode.StoreRemoveSessions);
                foreach (Guid uuid in Sessions.Keys.Where(uuid => uuid != exceptUuid).ToArray()) Sessions.Remove(uuid);
                return Ok(OperationCode.StoreRemoveSessions);
            }

            public Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default) =>
                Task.FromResult(Challenges.TryGetValue(uuid, out UserChallenge challenge) ? (UserChallenge?)challenge : null);

            public Task<OperationResult> AddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default) {
                Challenges.Add(challenge.Uuid, challenge);
                return Ok(OperationCode.StoreAddChallenge);
            }

            public Task<OperationResult> TryReserveChallengeAttemptAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default) {
                if (!Challenges.TryGetValue(challenge.Uuid, out UserChallenge current))
                    return Fail(StatusCode.NotFound, ContextCode.Entity, OperationCode.StoreReserveChallengeAttempt);
                if (current.ExpiresAt <= now) return Fail(StatusCode.Expired, ContextCode.Challenge, OperationCode.StoreReserveChallengeAttempt);
                if (current.Attempts >= current.MaxAttempts) return Fail(StatusCode.AttemptsExhausted, ContextCode.Challenge, OperationCode.StoreReserveChallengeAttempt);
                Challenges[current.Uuid] = current with { Attempts = current.Attempts + 1 };
                Reserved.Add(current.Uuid);
                return Ok(OperationCode.StoreReserveChallengeAttempt);
            }

            public Task<OperationResult> TryConsumeChallengeAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default) {
                if (!Challenges.TryGetValue(challenge.Uuid, out UserChallenge current))
                    return Fail(StatusCode.NotFound, ContextCode.Entity, OperationCode.StoreConsumeChallenge);
                if (current.ExpiresAt <= now) return Fail(StatusCode.Expired, ContextCode.Challenge, OperationCode.StoreConsumeChallenge);
                if (!Reserved.Remove(challenge.Uuid)) throw new InvalidOperationException("No reserved attempt.");
                Challenges.Remove(challenge.Uuid);
                return Ok(OperationCode.StoreConsumeChallenge);
            }

            public Task<OperationResult> RemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default) {
                if (!Challenges.Remove(challenge.Uuid)) return Fail(StatusCode.NotFound, ContextCode.Entity, OperationCode.StoreRemoveChallenge);
                return Ok(OperationCode.StoreRemoveChallenge);
            }

            public Task<OperationResult> RemoveChallengesAsync(User user, UserChallengeType? type = null, CancellationToken cancellationToken = default) {
                foreach (Guid uuid in Challenges.Values.Where(c => type is null || c.Type == type).Select(c => c.Uuid).ToArray()) Challenges.Remove(uuid);
                return Ok(OperationCode.StoreRemoveChallenges);
            }

            public Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default) {
                Guid[] expired = Challenges.Values.Where(c => c.ExpiresAt <= expiresAt).Select(c => c.Uuid).ToArray();
                foreach (Guid uuid in expired) Challenges.Remove(uuid);
                return Task.FromResult(expired.Length);
            }

            public IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default) => Stream(Audits.Where(a => a.User == user));
            public IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default) => Stream(Audits.Where(a => a.Actor == actor));
            public IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default) => Stream(Audits.Where(a => a.Entity == entity));

            public Task<OperationResult> AddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default) {
                if (FailAudit) return Fail(StatusCode.Conflict, ContextCode.None, OperationCode.StoreAddAudit);
                Audits.Add(audit);
                return Ok(OperationCode.StoreAddAudit);
            }

            public IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default) => Stream(Identities.Values);

            public Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default) =>
                Task.FromResult(Identities.TryGetValue(uuid, out UserExternalIdentity identity) ? (UserExternalIdentity?)identity : null);

            public Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<OperationResult> AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default) {
                Identities.Add(identity.Uuid, identity);
                return Ok(OperationCode.StoreAddExternalIdentity);
            }

            public Task<OperationResult> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default) {
                Identities[identity.Uuid] = identity;
                return Ok(OperationCode.StoreUpdateExternalIdentity);
            }

            public Task<OperationResult> RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default) {
                Identities.Remove(identity.Uuid);
                return Ok(OperationCode.StoreRemoveExternalIdentity);
            }

            public IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default) => Stream(Emails);
            public Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<OperationResult> AddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default) {
                if (FailEmailAddition) return Fail(StatusCode.Conflict, ContextCode.None, OperationCode.StoreAddEmail);
                if (Emails.Any(e => string.Equals(e.Address, email.Address, StringComparison.OrdinalIgnoreCase)))
                    return Fail(StatusCode.AlreadyExists, ContextCode.Email, OperationCode.StoreAddEmail);
                if (email.Primary) for (int i = 0; i < Emails.Count; i++) Emails[i] = Emails[i] with { Primary = false };
                Emails.Add(email);
                return Ok(OperationCode.StoreAddEmail);
            }

            public Task<OperationResult> RemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default) {
                Emails.RemoveAll(e => e.Address == email.Address);
                return Ok(OperationCode.StoreRemoveEmail);
            }

            public Task<OperationResult> SetPrimaryEmailAsync(User user, UserEmail? email, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default) => Stream(Phones);
            public Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<OperationResult> AddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default) {
                if (phone.Primary) for (int i = 0; i < Phones.Count; i++) Phones[i] = Phones[i] with { Primary = false };
                Phones.Add(phone);
                return Ok(OperationCode.StoreAddPhone);
            }

            public Task<OperationResult> RemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<OperationResult> SetPrimaryPhoneAsync(User user, UserPhone? phone, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default) => Stream(Totps.Values);

            public Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default) =>
                Task.FromResult(Totps.TryGetValue(uuid, out UserTotp totp) ? (UserTotp?)totp : null);

            public Task<OperationResult> AddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default) {
                Totps.Add(totp.Uuid, totp);
                return Ok(OperationCode.StoreAddTotp);
            }

            public Task<OperationResult> RemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<OperationResult> TryRenameTotpAsync(User user, UserTotp totp, string? name, CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<OperationResult> TryAdvanceTotpStepAsync(User user, UserTotp totp, ulong step, CancellationToken cancellationToken = default) {
                if (!Totps.TryGetValue(totp.Uuid, out UserTotp current)) return Fail(StatusCode.NotFound, ContextCode.Entity, OperationCode.StoreAdvanceTotpStep);
                if (current.LastStep is { } previous && step <= previous) return Fail(StatusCode.AlreadyUsed, ContextCode.Totp, OperationCode.StoreAdvanceTotpStep);
                Totps[totp.Uuid] = current with { LastStep = step };
                return Ok(OperationCode.StoreAdvanceTotpStep);
            }

            public IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default) => Stream(RecoveryCodes);

            public Task<OperationResult> ReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default) {
                RecoveryCodes = codes.ToList();
                return Ok(OperationCode.StoreReplaceRecoveryCodes);
            }

            public Task<OperationResult> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default) {
                int removed = RecoveryCodes.RemoveAll(code => code.CodeHash.AsSpan().SequenceEqual(codeHash.AsSpan()));
                return removed == 0 ? Fail(StatusCode.NotFound, ContextCode.Entity, OperationCode.StoreConsumeRecoveryCode) : Ok(OperationCode.StoreConsumeRecoveryCode);
            }

            public Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default) => Task.FromResult(RecoveryCodes.Count);

            private static async IAsyncEnumerable<T> Stream<T>(IEnumerable<T> items) {
                await Task.CompletedTask;
                foreach (T item in items) yield return item;
            }

            private sealed class Transaction(TestStore store) : IUserStoreTransaction {

                private readonly Dictionary<Guid, User> _users = new(store.Users);
                private readonly Dictionary<Guid, UserSession> _sessions = new(store.Sessions);
                private readonly Dictionary<Guid, UserChallenge> _challenges = new(store.Challenges);
                private readonly Dictionary<Guid, UserExternalIdentity> _identities = new(store.Identities);
                private readonly List<UserEmail> _emails = new(store.Emails);
                private readonly List<UserPhone> _phones = new(store.Phones);
                private readonly Dictionary<Guid, UserTotp> _totps = new(store.Totps);
                private readonly List<UserRecoveryCode> _recoveryCodes = new(store.RecoveryCodes);
                private readonly List<UserAudit> _audits = new(store.Audits);
                private bool _completed;

                public Task<OperationResult> CommitAsync(CancellationToken cancellationToken = default) {
                    _completed = true;
                    return Ok(OperationCode.StoreCommit);
                }

                public Task RollbackAsync(CancellationToken cancellationToken = default) {
                    if (!_completed) {
                        store.Users = _users;
                        store.Sessions = _sessions;
                        store.Challenges = _challenges;
                        store.Identities = _identities;
                        store.Audits = _audits;
                        store.Emails = _emails;
                        store.Phones = _phones;
                        store.Totps = _totps;
                        store.RecoveryCodes = _recoveryCodes;
                        _completed = true;
                    }
                    return Task.CompletedTask;
                }

                public async ValueTask DisposeAsync() => await RollbackAsync();

            }

        }

    }

}
