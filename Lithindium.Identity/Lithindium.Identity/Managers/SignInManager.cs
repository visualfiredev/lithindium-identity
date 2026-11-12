// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Authenticates users and manages persisted sessions.
    /// </summary>
    /// <remarks>
    /// Trusted backend API; authorize administrative calls and protect bearer tokens.
    /// Public endpoints must conceal account existence and enforce distributed rate limits.
    /// Share the data protection key ring across instances. Bearer tokens are not OIDC tokens.
    /// Use one scoped manager/store instance per operation; do not invoke concurrently.
    /// </remarks>
    public class SignInManager {

        private readonly UserManager _users;
        private readonly ChallengeManager _challenges;
        private readonly IUserStore _store;
        private readonly IDataProtector _protector;
        private readonly SignInManagerOptions _options;

        // Match the default password hasher's work for unknown or passwordless accounts.
        private static readonly Lazy<string> DummyHash = new(() => new PasswordHasher<object>().HashPassword(new object(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32))));

        /// <summary>
        /// Creates a sign-in manager sharing the user and challenge managers' store.
        /// </summary>
        public SignInManager(UserManager users, ChallengeManager challenges, SignInManagerOptions? options = null) {
            ArgumentNullException.ThrowIfNull(users);
            ArgumentNullException.ThrowIfNull(challenges);
            if (!ReferenceEquals(users, challenges.Users)) throw new ArgumentException("Managers must share the same UserManager.", nameof(challenges));

            _users = users;
            _challenges = challenges;
            _store = users.Store;
            _protector = challenges.Protection.CreateProtector("Lithindium.Identity.Session.v1");
            _options = options ?? new SignInManagerOptions();
            if (_options.SessionLifetime <= TimeSpan.Zero || _options.SessionLifetime > TimeSpan.FromDays(365) || _options.LockoutDuration <= TimeSpan.Zero || _options.LockoutDuration > TimeSpan.FromDays(1) || _options.MaxFailures is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(options));
        }

        /// <summary>
        /// Authenticates a username and password.
        /// </summary>
        /// <remarks>
        /// Success may require MFA; check the returned ticket's authentication state.
        /// </remarks>
        public virtual Task<OperationResult<SignInTicket>> PasswordSignInAsync(string username, string password, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) => PasswordCoreAsync(() => string.IsNullOrEmpty(username) || username.Length > 21 ? Task.FromResult<User?>(null) : _store.FindByUsernameAsync(username, cancellationToken), password, metadata, cancellationToken);

        /// <summary>
        /// Authenticates a verified email and password.
        /// </summary>
        public virtual Task<OperationResult<SignInTicket>> EmailSignInAsync(string address, string password, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) => _store is IUserEmailStore emails ? PasswordCoreAsync(() => UserManager.ValidEmail(new UserEmail(address, false)) ? emails.FindByEmailAsync(address, cancellationToken) : Task.FromResult<User?>(null), password, metadata, cancellationToken) : Task.FromResult(Rejected(StatusCode.NotSupported, ContextCode.Email, OperationCode.SignInPassword));

        /// <summary>
        /// Authenticates a verified phone and password.
        /// </summary>
        public virtual Task<OperationResult<SignInTicket>> PhoneSignInAsync(string number, string? extension, string password, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) => _store is IUserPhoneStore phones ? PasswordCoreAsync(() => UserManager.ValidPhone(new UserPhone(number, extension, false)) ? phones.FindByPhoneAsync(number, extension, cancellationToken) : Task.FromResult<User?>(null), password, metadata, cancellationToken) : Task.FromResult(Rejected(StatusCode.NotSupported, ContextCode.Phone, OperationCode.SignInPassword));

        /// <summary>
        /// Authenticates an already validated external provider assertion.
        /// </summary>
        /// <remarks>
        /// Caller validates signature, issuer, audience, expiration, nonce and replay protection.
        /// Never accept issuer or subject directly from an untrusted client.
        /// </remarks>
        public virtual async Task<OperationResult<SignInTicket>> ExternalSignInAsync(string issuer, string subject, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.SignInExternal;
            if (_store is not IUserExternalIdentityStore external) return Rejected(StatusCode.NotSupported, ContextCode.ExternalIdentity, op);
            if (string.IsNullOrWhiteSpace(issuer) || issuer.Length > 512 || string.IsNullOrWhiteSpace(subject) || subject.Length > 255) return Rejected(StatusCode.InvalidCredential, ContextCode.ExternalIdentity, op);
            OperationResult validation = ValidateConfiguration(metadata, op);
            if (validation.IsFailure) return new OperationResult<SignInTicket>(validation);

            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await external.FindByExternalIdentityAsync(issuer, subject, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return Rejected(StatusCode.InvalidCredential, ContextCode.ExternalIdentity, op);
            validation = await CheckAccountAsync(user, op, cancellationToken).ConfigureAwait(false);
            if (validation.IsFailure) return new OperationResult<SignInTicket>(validation);

            return await FinishFirstFactorAsync(transaction, user, "federated", metadata, op, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Completes MFA using a TOTP authenticator.
        /// </summary>
        public virtual Task<OperationResult<SignInTicket>> CompleteTotpAsync(Guid userUuid, Guid challengeUuid, string continuationProof, Guid totpUuid, string code, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) => CompleteMultiFactorCoreAsync(userUuid, challengeUuid, continuationProof, totpUuid, code, metadata, cancellationToken);

        /// <summary>
        /// Completes MFA using a single-use recovery code.
        /// </summary>
        public virtual Task<OperationResult<SignInTicket>> CompleteRecoveryCodeAsync(Guid userUuid, Guid challengeUuid, string continuationProof, string code, SessionMetadata? metadata = null, CancellationToken cancellationToken = default) => CompleteMultiFactorCoreAsync(userUuid, challengeUuid, continuationProof, null, code, metadata, cancellationToken);

        /// <summary>
        /// Validates a protected session token against current persisted state.
        /// </summary>
        /// <remarks>
        /// UUIDs alone are not credentials. Validation requires a fresh, active account
        /// and an existing unexpired session. Lockout does not revoke existing sessions.
        /// </remarks>
        public virtual async Task<OperationResult<AuthenticatedSession>> ValidateSessionAsync(string token, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.SignInValidateSession;
            if (_store is not IUserSessionStore sessions) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.NotSupported, ContextCode.Session, op));
            if (!TryReadToken(token, out Guid userUuid, out Guid sessionUuid)) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.InvalidCredential, ContextCode.Session, op));

            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user || user.Status != UserStatus.Active) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.InvalidCredential, ContextCode.Session, op));
            UserSession? persisted = await sessions.GetSessionAsync(user, sessionUuid, cancellationToken).ConfigureAwait(false);
            if (persisted is not { } session) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.InvalidCredential, ContextCode.Session, op));
            DateTime now = Now;
            if (session.ExpiresAt <= now) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.Expired, ContextCode.Session, op));
            if (session.AuthenticatedAt > now || session.ExpiresAt <= session.AuthenticatedAt) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.InvalidState, ContextCode.Session, op));
            OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!Succeeded(commit)) return new OperationResult<AuthenticatedSession>(commit);
            if (session.ExpiresAt <= Now) return new OperationResult<AuthenticatedSession>(Failure(StatusCode.Expired, ContextCode.Session, op));
            return new OperationResult<AuthenticatedSession>(Success(op), new AuthenticatedSession(userUuid, session));
        }

        /// <summary>
        /// Revokes the session identified by a protected token.
        /// </summary>
        public virtual Task<OperationResult> SignOutAsync(string token, CancellationToken cancellationToken = default) => TryReadToken(token, out Guid userUuid, out Guid sessionUuid) ? RevokeCoreAsync(userUuid, sessionUuid, null, OperationCode.SignInSignOut, true, cancellationToken) : Task.FromResult(Failure(StatusCode.InvalidCredential, ContextCode.Session, OperationCode.SignInSignOut));

        /// <summary>
        /// Gets a user's persisted sessions, including expired sessions.
        /// </summary>
        public virtual async Task<OperationResult<ImmutableArray<UserSession>>> GetSessionsAsync(Guid userUuid, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.SignInGetSessions;
            if (_store is not IUserSessionStore sessions) return new OperationResult<ImmutableArray<UserSession>>(Failure(StatusCode.NotSupported, ContextCode.Session, op));
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return new OperationResult<ImmutableArray<UserSession>>(Failure(StatusCode.NotFound, ContextCode.User, op));
            ImmutableArray<UserSession>.Builder result = ImmutableArray.CreateBuilder<UserSession>();
            await foreach (UserSession session in sessions.GetSessionsAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) result.Add(session);
            OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Succeeded(commit) ? new OperationResult<ImmutableArray<UserSession>>(Success(op), result.ToImmutable()) : new OperationResult<ImmutableArray<UserSession>>(commit);
        }

        /// <summary>
        /// Revokes a specific session after caller authorization.
        /// </summary>
        public virtual Task<OperationResult> RevokeSessionAsync(Guid userUuid, Guid sessionUuid, Guid? actor = null, CancellationToken cancellationToken = default) => RevokeCoreAsync(userUuid, sessionUuid, actor, OperationCode.SignInRevokeSession, false, cancellationToken);

        /// <summary>
        /// Revokes sessions and pending MFA; optionally preserves one authorized session.
        /// </summary>
        public virtual async Task<OperationResult> RevokeSessionsAsync(Guid userUuid, Guid? exceptSessionUuid = null, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.SignInRevokeSessions;
            if (_store is not IUserSessionStore sessions) return Failure(StatusCode.NotSupported, ContextCode.Session, op);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return Failure(StatusCode.NotFound, ContextCode.User, op);
            if (exceptSessionUuid is { } preserved) {
                UserSession? session = await sessions.GetSessionAsync(user, preserved, cancellationToken).ConfigureAwait(false);
                if (session is not { } current) return Failure(StatusCode.NotFound, ContextCode.Session, op);
                if (current.ExpiresAt <= Now) return Failure(StatusCode.Expired, ContextCode.Session, op);
            }
            OperationResult result = await sessions.RemoveSessionsAsync(user, exceptSessionUuid, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return result;
            if (_store is IUserChallengeStore challenges) {
                result = await challenges.RemoveChallengesAsync(user, UserChallengeType.MultiFactorAuthentication, cancellationToken).ConfigureAwait(false);
                if (!Succeeded(result)) return result;
            }
            result = await _users.AuditAsync(user, actor, null, UserAuditType.Session, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return result;
            result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Succeeded(result) ? Success(op) : result;
        }

        /// <summary>
        /// Removes expired sessions and returns the count.
        /// </summary>
        public virtual async Task<OperationResult<int>> RemoveExpiredSessionsAsync(CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.SignInRemoveExpiredSessions;
            if (_store is not IUserSessionStore sessions) return new OperationResult<int>(Failure(StatusCode.NotSupported, ContextCode.Session, op));
            int count = await sessions.RemoveExpiredSessionsAsync(Now, cancellationToken).ConfigureAwait(false);
            return new OperationResult<int>(Success(op), count);
        }

        private async Task<OperationResult<SignInTicket>> PasswordCoreAsync(Func<Task<User?>> find, string password, SessionMetadata? metadata, CancellationToken cancellationToken) {
            const OperationCode op = OperationCode.SignInPassword;
            OperationResult validation = ValidateConfiguration(metadata, op);
            if (validation.IsFailure) return new OperationResult<SignInTicket>(validation);
            if (string.IsNullOrEmpty(password) || password.Length > 1024) return Rejected(StatusCode.InvalidCredential, ContextCode.Password, op);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await find().ConfigureAwait(false);
            if (found is not { } user) {
                VerifyDummy(password);
                return Rejected(StatusCode.InvalidCredential, ContextCode.Password, op);
            }
            validation = await CheckAccountAsync(user, op, cancellationToken).ConfigureAwait(false);
            if (validation.IsFailure) {
                VerifyDummy(password);
                return new OperationResult<SignInTicket>(validation);
            }

            PasswordVerificationResult verification = _users.VerifyPassword(user, password);
            if (user.Password is null) VerifyDummy(password);
            if (verification is not (PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)) {
                OperationResult recorded = await RecordFailureAsync(user, cancellationToken).ConfigureAwait(false);
                if (!Succeeded(recorded)) return new OperationResult<SignInTicket>(recorded);
                OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return Succeeded(commit) ? Rejected(StatusCode.InvalidCredential, ContextCode.Password, op) : new OperationResult<SignInTicket>(commit);
            }
            if (verification == PasswordVerificationResult.SuccessRehashNeeded) {
                User replacement = user with { Password = _users.HashPassword(user, password) };
                OperationResult updated = await _store.TryUpdateAsync(user, replacement, cancellationToken).ConfigureAwait(false);
                if (!MutationSucceeded(updated)) return new OperationResult<SignInTicket>(updated);
                user = replacement;
            }
            return await FinishFirstFactorAsync(transaction, user, "pwd", metadata, op, cancellationToken).ConfigureAwait(false);
        }

        private async Task<OperationResult<SignInTicket>> FinishFirstFactorAsync(IUserStoreTransaction transaction, User user, string firstFactor, SessionMetadata? metadata, OperationCode op, CancellationToken cancellationToken) {
            bool enrolled = false;
            if (_store is IUserTotpStore totps) {
                await foreach (UserTotp _ in totps.GetTotpsAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                    enrolled = true;
                    break;
                }
            }
            if (enrolled) {
                OperationResult<ChallengeTicket> pending = await _challenges.IssueMultiFactorAsync(user, firstFactor, cancellationToken).ConfigureAwait(false);
                if (!pending.HasValue) return new OperationResult<SignInTicket>(pending.Result);
                OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                if (!Succeeded(commit)) return new OperationResult<SignInTicket>(commit);
                return new OperationResult<SignInTicket>(new OperationResult(StatusCode.Success, ContextCode.None, op, DetailCode.MultiFactorRequired), new SignInTicket(user.Uuid, pending.Value));
            }
            if (_options.RequireMultiFactor) return Rejected(StatusCode.PolicyViolation, ContextCode.Totp, op, DetailCode.MultiFactorRequired);

            OperationResult<SignInTicket> issued = await IssueSessionAsync(user, ImmutableArray.Create(firstFactor), metadata, op, cancellationToken).ConfigureAwait(false);
            if (!issued.HasValue) return issued;
            OperationResult result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Succeeded(result) ? issued : new OperationResult<SignInTicket>(result);
        }

        private async Task<OperationResult<SignInTicket>> CompleteMultiFactorCoreAsync(Guid userUuid, Guid challengeUuid, string continuationProof, Guid? totpUuid, string code, SessionMetadata? metadata, CancellationToken cancellationToken) {
            const OperationCode op = OperationCode.SignInCompleteMultiFactor;
            OperationResult validation = ValidateConfiguration(metadata, op);
            if (validation.IsFailure) return new OperationResult<SignInTicket>(validation);

            SignInTicket? ticket = null;
            OperationResult result = await _challenges.CompleteMultiFactorAsync(userUuid, challengeUuid, continuationProof, totpUuid, code, async (user, challenge, firstFactor, ct) => {
                OperationResult<SignInTicket> issued = await IssueSessionAsync(user, ImmutableArray.Create(firstFactor, totpUuid.HasValue ? "otp" : "recovery", "mfa"), metadata, op, ct).ConfigureAwait(false);
                if (issued.HasValue) ticket = issued.Value;
                return issued.Result;
            }, cancellationToken, RecordFailureAsync).ConfigureAwait(false);
            if (!Succeeded(result)) return new OperationResult<SignInTicket>(result);
            return new OperationResult<SignInTicket>(Success(op), ticket ?? throw new InvalidOperationException("MFA completed without a session."));
        }

        private async Task<OperationResult<SignInTicket>> IssueSessionAsync(User user, ImmutableArray<string> amr, SessionMetadata? metadata, OperationCode op, CancellationToken cancellationToken) {
            if (_store is not IUserSessionStore sessions) return Rejected(StatusCode.NotSupported, ContextCode.Session, op);
            OperationResult result = await ResetFailuresAsync(user, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return new OperationResult<SignInTicket>(result);
            DateTime now = Now;
            string? ip = metadata?.IpAddress is { } address ? IPAddress.Parse(address).ToString() : null;
            UserSession session = new UserSession(Guid.NewGuid(), metadata?.DeviceName, ip, metadata?.UserAgent, amr, null, now, now + _options.SessionLifetime);
            string token = CreateToken(user.Uuid, session.Uuid);
            result = await sessions.AddSessionAsync(user, session, cancellationToken).ConfigureAwait(false);
            if (!MutationSucceeded(result)) return new OperationResult<SignInTicket>(result);
            result = await _users.AuditAsync(user, user.Uuid, session.Uuid, UserAuditType.Session, UserAuditAction.Created, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return new OperationResult<SignInTicket>(result);
            return new OperationResult<SignInTicket>(Success(op), new SignInTicket(new AuthenticatedSession(user.Uuid, session), token));
        }

        private async Task<OperationResult> CheckAccountAsync(User user, OperationCode op, CancellationToken cancellationToken) {
            if (user.Status != UserStatus.Active) return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.Suspended);
            if (_store is IUserLockoutStore lockouts) {
                UserLockout? state = await lockouts.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
                if (state is { Enabled: true, ReleaseAt: { } release } && release > Now) return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.LockedOut);
            }
            return Success(op);
        }

        private async Task<OperationResult> RecordFailureAsync(User user, CancellationToken cancellationToken) {
            if (_store is not IUserLockoutStore lockouts) return Success(OperationCode.SignInPassword);
            UserLockout? current = await lockouts.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
            if (current is { Enabled: false }) return Success(OperationCode.SignInPassword);
            DateTime now = Now;
            if (current is { ReleaseAt: { } release } && release > now) return Success(OperationCode.SignInPassword);
            uint failures = current is { ReleaseAt: null } state ? state.Failures : 0;
            if (failures < uint.MaxValue) failures++;
            DateTime? until = failures >= _options.MaxFailures ? now + _options.LockoutDuration : null;
            OperationResult result = await lockouts.TryUpdateLockoutAsync(user, current, new UserLockout(true, failures, until), cancellationToken).ConfigureAwait(false);
            MutationSucceeded(result);
            return result;
        }

        private async Task<OperationResult> ResetFailuresAsync(User user, CancellationToken cancellationToken) {
            if (_store is not IUserLockoutStore lockouts) return Success(OperationCode.SignInPassword);
            UserLockout? current = await lockouts.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
            if (current is not { } state || (state.Failures == 0 && state.ReleaseAt is null)) return Success(OperationCode.SignInPassword);
            OperationResult result = await lockouts.TryUpdateLockoutAsync(user, current, state with { Failures = 0, ReleaseAt = null }, cancellationToken).ConfigureAwait(false);
            MutationSucceeded(result);
            return result;
        }

        private async Task<OperationResult> RevokeCoreAsync(Guid userUuid, Guid sessionUuid, Guid? actor, OperationCode op, bool idempotent, CancellationToken cancellationToken) {
            if (_store is not IUserSessionStore sessions) return Failure(StatusCode.NotSupported, ContextCode.Session, op);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return idempotent ? new OperationResult(StatusCode.NoOp, operation: op) : Failure(StatusCode.NotFound, ContextCode.User, op);
            UserSession? persisted = await sessions.GetSessionAsync(user, sessionUuid, cancellationToken).ConfigureAwait(false);
            if (persisted is not { } session) return idempotent ? new OperationResult(StatusCode.NoOp, operation: op) : Failure(StatusCode.NotFound, ContextCode.Session, op);
            OperationResult result = await sessions.RemoveSessionAsync(user, session, cancellationToken).ConfigureAwait(false);
            if (!MutationSucceeded(result)) return result;
            result = await _users.AuditAsync(user, actor ?? (idempotent ? (Guid?)userUuid : null), sessionUuid, UserAuditType.Session, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return result;
            result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Succeeded(result) ? Success(op) : result;
        }

        private OperationResult ValidateConfiguration(SessionMetadata? metadata, OperationCode op) {
            if (_store is not IUserSessionStore) return Failure(StatusCode.NotSupported, ContextCode.Session, op);
            if (_options.RequireLockoutStore && _store is not IUserLockoutStore) return Failure(StatusCode.NotSupported, ContextCode.Lockout, op);
            if (_options.RequireMultiFactor && (_store is not IUserTotpStore || _store is not IUserChallengeStore)) return Failure(StatusCode.NotSupported, ContextCode.Totp, op);
            if (metadata is not null && (!UserManager.ValidOptionalName(metadata.DeviceName, 64) || metadata.UserAgent is { Length: > 2048 } || (metadata.IpAddress is { } ip && (ip.Length > 64 || !IPAddress.TryParse(ip, out IPAddress? parsed) || (parsed.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && parsed.ScopeId != 0))))) return Failure(StatusCode.MalformedInput, ContextCode.Session, op);
            return Success(op);
        }

        private string CreateToken(Guid userUuid, Guid sessionUuid) {
            byte[] data = new byte[32];
            userUuid.TryWriteBytes(data.AsSpan(0, 16));
            sessionUuid.TryWriteBytes(data.AsSpan(16, 16));
            return Convert.ToBase64String(_protector.Protect(data));
        }

        private bool TryReadToken(string token, out Guid userUuid, out Guid sessionUuid) {
            userUuid = sessionUuid = Guid.Empty;
            if (string.IsNullOrEmpty(token) || token.Length > 1024) return false;
            try {
                byte[] data = _protector.Unprotect(Convert.FromBase64String(token));
                if (data.Length != 32) return false;
                userUuid = new Guid(data.AsSpan(0, 16));
                sessionUuid = new Guid(data.AsSpan(16, 16));
                return userUuid != Guid.Empty && sessionUuid != Guid.Empty;
            } catch (FormatException) {
                return false;
            } catch (CryptographicException) {
                return false;
            }
        }

        private static void VerifyDummy(string password) => _ = new PasswordHasher<object>().VerifyHashedPassword(new object(), DummyHash.Value, password);

        private DateTime Now => _users.Clock.GetUtcNow().UtcDateTime;

        private static bool Succeeded(OperationResult result) => UserManager.StoreSucceeded(result);

        private static bool MutationSucceeded(OperationResult result) {
            if (result.Status == StatusCode.NoOp) throw new InvalidOperationException("Required sign-in mutation was not applied.");
            return Succeeded(result);
        }

        private static OperationResult Success(OperationCode op) => new(StatusCode.Success, operation: op);

        private static OperationResult Failure(StatusCode status, ContextCode context, OperationCode op, DetailCode detail = DetailCode.None) => new(status, context, op, detail);

        private static OperationResult<SignInTicket> Rejected(StatusCode status, ContextCode context, OperationCode op, DetailCode detail = DetailCode.None) => new(Failure(status, context, op, detail));

    }

}
