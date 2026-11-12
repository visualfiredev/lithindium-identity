// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;

using OtpNet;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Issues and consumes single-use identity challenges.
    /// </summary>
    /// <remarks>
    /// Trusted backend API. Callers authorize contact and authenticator changes, deliver
    /// secrets privately, enforce distributed issuance and verification rate limits, and
    /// conceal account existence in public recovery flows. Issuing a new challenge
    /// replaces the prior challenge of the same purpose. Share the data protection key
    /// ring and application discriminator across instances. Do not invoke concurrently.
    /// </remarks>
    public class ChallengeManager {

        private readonly UserManager _users;
        private readonly IUserStore _store;
        private readonly IDataProtectionProvider _protection;
        private readonly ChallengeManagerOptions _options;

        internal UserManager Users => _users;
        internal IDataProtectionProvider Protection => _protection;

        /// <summary>
        /// Creates a manager sharing the user manager's store and clock.
        /// </summary>
        public ChallengeManager(UserManager users, IDataProtectionProvider protection, ChallengeManagerOptions? options = null) {
            ArgumentNullException.ThrowIfNull(users);
            ArgumentNullException.ThrowIfNull(protection);

            _users = users;
            _store = users.Store;
            _protection = protection;
            _options = options ?? new ChallengeManagerOptions();
            if (!ValidLifetime(_options.ContactLifetime) || !ValidLifetime(_options.PasswordResetLifetime) ||
                !ValidLifetime(_options.TotpEnrollmentLifetime) || !ValidLifetime(_options.MultiFactorLifetime) ||
                _options.MaxAttempts is < 1 or > 10)
                throw new ArgumentOutOfRangeException(nameof(options));
        }

        /// <summary>
        /// Issues proof for an email address after authorized reauthentication.
        /// </summary>
        public virtual Task<OperationResult<ChallengeTicket>> IssueEmailVerificationAsync(User authenticatedUser, UserEmail email, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeIssueEmailVerification;
            if (_store is not IUserEmailStore) return RejectedTicket(StatusCode.NotSupported, ContextCode.Email, op);
            if (!UserManager.ValidEmail(email)) return RejectedTicket(StatusCode.MalformedInput, ContextCode.Email, op);
            return IssueAsync(authenticatedUser.Uuid, authenticatedUser.Rotation, UserChallengeType.EmailVerification, ContactPayload(email), false, actor, op, null, cancellationToken);
        }

        /// <summary>
        /// Verifies and adds the bound email atomically.
        /// </summary>
        public virtual Task<OperationResult> CompleteEmailVerificationAsync(Guid userUuid, Guid challengeUuid, string proof, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeCompleteEmailVerification;
            if (_store is not IUserEmailStore emails) return Task.FromResult(Failure(StatusCode.NotSupported, ContextCode.Email, op));
            return CompleteAsync(userUuid, challengeUuid, UserChallengeType.EmailVerification, proof, op, actor, null,
                async (user, challenge, payload) => {
                    var email = new UserEmail(payload.Target!, payload.Primary);
                    OperationResult result = await emails.AddEmailAsync(user, email, cancellationToken).ConfigureAwait(false);
                    if (!MutationSucceeded(result)) return result;
                    result = await _users.RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                    if (!Succeeded(result)) return result;
                    return await _users.AuditAsync(user, actor, null, UserAuditType.Email, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
                }, cancellationToken);
        }

        /// <summary>
        /// Issues a phone verification code after authorized reauthentication.
        /// </summary>
        /// <remarks>
        /// Delivery must verify the exact number and extension, if present.
        /// </remarks>
        public virtual Task<OperationResult<ChallengeTicket>> IssuePhoneVerificationAsync(User authenticatedUser, UserPhone phone, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeIssuePhoneVerification;
            if (_store is not IUserPhoneStore) return RejectedTicket(StatusCode.NotSupported, ContextCode.Phone, op);
            if (!UserManager.ValidPhone(phone)) return RejectedTicket(StatusCode.MalformedInput, ContextCode.Phone, op);
            return IssueAsync(authenticatedUser.Uuid, authenticatedUser.Rotation, UserChallengeType.PhoneVerification, ContactPayload(phone), true, actor, op, null, cancellationToken);
        }

        /// <summary>
        /// Verifies and adds the bound phone atomically.
        /// </summary>
        public virtual Task<OperationResult> CompletePhoneVerificationAsync(Guid userUuid, Guid challengeUuid, string proof, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeCompletePhoneVerification;
            if (_store is not IUserPhoneStore phones) return Task.FromResult(Failure(StatusCode.NotSupported, ContextCode.Phone, op));
            return CompleteAsync(userUuid, challengeUuid, UserChallengeType.PhoneVerification, proof, op, actor, null,
                async (user, challenge, payload) => {
                    var phone = new UserPhone(payload.Target!, payload.Secondary, payload.Primary);
                    OperationResult result = await phones.AddPhoneAsync(user, phone, cancellationToken).ConfigureAwait(false);
                    if (!MutationSucceeded(result)) return result;
                    result = await _users.RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                    if (!Succeeded(result)) return result;
                    return await _users.AuditAsync(user, actor, null, UserAuditType.Phone, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
                }, cancellationToken);
        }

        /// <summary>
        /// Issues password recovery proof to an existing verified email.
        /// </summary>
        public virtual Task<OperationResult<ChallengeTicket>> IssuePasswordResetAsync(Guid userUuid, UserEmail email, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeIssuePasswordReset;
            if (_store is not IUserEmailStore) return RejectedTicket(StatusCode.NotSupported, ContextCode.Email, op);
            if (!UserManager.ValidEmail(email)) return RejectedTicket(StatusCode.MalformedInput, ContextCode.Email, op);
            Payload payload = ContactPayload(email);
            return IssueAsync(userUuid, null, UserChallengeType.PasswordReset, payload, false, null, op,
                user => ValidateResetDestinationAsync(user, payload, op, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Issues password recovery proof to an existing verified phone.
        /// </summary>
        public virtual Task<OperationResult<ChallengeTicket>> IssuePasswordResetAsync(Guid userUuid, UserPhone phone, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeIssuePasswordReset;
            if (_store is not IUserPhoneStore) return RejectedTicket(StatusCode.NotSupported, ContextCode.Phone, op);
            if (!UserManager.ValidPhone(phone)) return RejectedTicket(StatusCode.MalformedInput, ContextCode.Phone, op);
            Payload payload = ContactPayload(phone);
            return IssueAsync(userUuid, null, UserChallengeType.PasswordReset, payload, true, null, op,
                user => ValidateResetDestinationAsync(user, payload, op, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Resets a native password and revokes all sessions and challenges.
        /// </summary>
        /// <remarks>
        /// Does not bypass MFA or clear lockout. Only the challenge is audited.
        /// </remarks>
        public virtual Task<OperationResult> CompletePasswordResetAsync(Guid userUuid, Guid challengeUuid, string proof, string password, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeCompletePasswordReset;
            OperationResult validation = _users.ValidatePassword(password, op);
            if (validation.IsFailure) return Task.FromResult(validation);
            return CompleteAsync(userUuid, challengeUuid, UserChallengeType.PasswordReset, proof, op, null, null,
                async (user, challenge, payload) => {
                    OperationResult result = await ValidateResetDestinationAsync(user, payload, op, cancellationToken).ConfigureAwait(false);
                    if (!Succeeded(result)) return result;
                    return await _users.RotateAsync(user, user with { Password = _users.HashPassword(user, password) }, null, op, cancellationToken).ConfigureAwait(false);
                }, cancellationToken);
        }

        /// <summary>
        /// Issues an authenticator enrollment key after authorized reauthentication.
        /// </summary>
        public virtual Task<OperationResult<ChallengeTicket>> IssueTotpEnrollmentAsync(User authenticatedUser, string? name = null, UserTotpAlgorithm algorithm = UserTotpAlgorithm.Sha1, byte digits = 6, byte period = 30, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeIssueTotpEnrollment;
            if (_store is not IUserTotpStore) return RejectedTicket(StatusCode.NotSupported, ContextCode.Totp, op);
            if (!UserManager.ValidOptionalName(name, 32) || !Enum.IsDefined(algorithm) || digits is < 6 or > 8 || period == 0)
                return RejectedTicket(StatusCode.MalformedInput, ContextCode.Totp, op);

            var payload = new Payload {
                Key = RandomNumberGenerator.GetBytes(algorithm switch {
                    UserTotpAlgorithm.Sha1 => 20,
                    UserTotpAlgorithm.Sha256 => 32,
                    UserTotpAlgorithm.Sha512 => 64,
                    _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
                }),
                Target = name,
                Algorithm = algorithm,
                Digits = digits,
                Period = period
            };
            return IssueAsync(authenticatedUser.Uuid, authenticatedUser.Rotation, UserChallengeType.TotpEnrollment, payload, false, actor, op, null, cancellationToken);
        }

        /// <summary>
        /// Verifies a TOTP enrollment code and persists the protected authenticator.
        /// </summary>
        public virtual Task<OperationResult> CompleteTotpEnrollmentAsync(Guid userUuid, Guid challengeUuid, string code, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeCompleteTotpEnrollment;
            if (_store is not IUserTotpStore totps) return Task.FromResult(Failure(StatusCode.NotSupported, ContextCode.Totp, op));
            ulong step = 0;
            return CompleteAsync(userUuid, challengeUuid, UserChallengeType.TotpEnrollment, code, op, actor,
                (user, challenge, payload) => Task.FromResult(VerifyTotp(payload.Key, payload.Algorithm, payload.Digits, payload.Period, code, Now, out step)
                    ? Success(op) : Failure(StatusCode.InvalidCredential, ContextCode.Totp, op)),
                async (user, challenge, payload) => {
                    Guid uuid = Guid.CreateVersion7();
                    byte[] protectedKey = TotpProtector(user.Uuid, uuid).Protect(payload.Key);
                    if (protectedKey.Length > 255) throw new InvalidOperationException("Protected TOTP secret exceeds the store limit.");
                    var totp = new UserTotp(uuid, payload.Target, ImmutableArray.CreateRange(protectedKey), payload.Algorithm, payload.Digits, payload.Period, step);
                    OperationResult result = await totps.AddTotpAsync(user, totp, cancellationToken).ConfigureAwait(false);
                    if (!MutationSucceeded(result)) return result;
                    result = await _users.RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                    if (!Succeeded(result)) return result;
                    return await _users.AuditAsync(user, actor, uuid, UserAuditType.Totp, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
                }, cancellationToken);
        }

        /// <summary>
        /// Cancels a user's challenge.
        /// </summary>
        public virtual async Task<OperationResult> CancelAsync(Guid userUuid, Guid challengeUuid, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeCancel;
            if (_store is not IUserChallengeStore challenges) return Failure(StatusCode.NotSupported, ContextCode.Challenge, op);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return Failure(StatusCode.NotFound, ContextCode.User, op);
            UserChallenge? pending = await challenges.GetChallengeAsync(user, challengeUuid, cancellationToken).ConfigureAwait(false);
            if (pending is not { } challenge) return Failure(StatusCode.NotFound, ContextCode.Challenge, op);
            OperationResult result = await challenges.RemoveChallengeAsync(user, challenge, cancellationToken).ConfigureAwait(false);
            if (!MutationSucceeded(result)) return result;
            result = await _users.AuditAsync(user, actor, challengeUuid, UserAuditType.Challenge, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return result;
            result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Succeeded(result) ? Success(op) : result;
        }

        /// <summary>
        /// Removes expired challenges and returns the count.
        /// </summary>
        public virtual async Task<OperationResult<int>> RemoveExpiredAsync(CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.ChallengeRemoveExpired;
            if (_store is not IUserChallengeStore challenges) return new OperationResult<int>(Failure(StatusCode.NotSupported, ContextCode.Challenge, op));
            int count = await challenges.RemoveExpiredChallengesAsync(Now, cancellationToken).ConfigureAwait(false);
            return new OperationResult<int>(Success(op), count);
        }

        // Sign-in must call this inside its first-factor transaction, using freshly verified state.
        // Never expose the returned ticket before committing that transaction.
        internal async Task<OperationResult<ChallengeTicket>> IssueMultiFactorAsync(User user, string firstFactor, CancellationToken cancellationToken) {
            const OperationCode op = OperationCode.ChallengeIssueMultiFactor;
            if (_store is not IUserChallengeStore challenges) return RejectTicket(StatusCode.NotSupported, ContextCode.Challenge, op);
            if (_store is not IUserTotpStore totps) return RejectTicket(StatusCode.NotSupported, ContextCode.Totp, op);
            if (user.Status != UserStatus.Active) return RejectTicket(StatusCode.AccessDenied, ContextCode.User, op);
            bool enrolled = false;
            await foreach (UserTotp _ in totps.GetTotpsAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                enrolled = true;
                break;
            }
            if (!enrolled) return RejectTicket(StatusCode.InvalidState, ContextCode.Totp, op);

            if (firstFactor is not ("pwd" or "federated")) throw new ArgumentOutOfRangeException(nameof(firstFactor));
            var payload = new Payload { Key = RandomNumberGenerator.GetBytes(32), Target = firstFactor };
            try {
                return await IssueCoreAsync(challenges, user, UserChallengeType.MultiFactorAuthentication, payload, false, null, op, cancellationToken).ConfigureAwait(false);
            } finally {
                CryptographicOperations.ZeroMemory(payload.Key);
            }
        }

        // Only SignInManager may supply the completion callback. It must issue the session
        // in this transaction without opening another one or publishing credentials.
        internal Task<OperationResult> CompleteMultiFactorAsync(Guid userUuid, Guid challengeUuid, string continuationProof, Guid? totpUuid, string code,
            Func<User, UserChallenge, string, CancellationToken, Task<OperationResult>> complete, CancellationToken cancellationToken,
            Func<User, CancellationToken, Task<OperationResult>>? rejectedFactor = null) {
            const OperationCode op = OperationCode.ChallengeCompleteMultiFactor;
            if (_store is not IUserTotpStore totps) return Task.FromResult(Failure(StatusCode.NotSupported, ContextCode.Totp, op));
            return CompleteAsync(userUuid, challengeUuid, UserChallengeType.MultiFactorAuthentication, continuationProof, op, null,
                async (user, challenge, payload) => {
                    if (payload.Target is not ("pwd" or "federated")) throw new InvalidOperationException("Missing first-factor method.");
                    if (_store is IUserLockoutStore lockouts) {
                        UserLockout? state = await lockouts.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
                        if (state is { Enabled: true, ReleaseAt: { } release } && release > Now)
                            return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.LockedOut);
                    }
                    if (totpUuid is { } uuid) {
                        UserTotp? found = await totps.GetTotpAsync(user, uuid, cancellationToken).ConfigureAwait(false);
                        if (found is not { } totp) return Failure(StatusCode.InvalidCredential, ContextCode.Totp, op);
                        byte[] key = TotpProtector(user.Uuid, uuid).Unprotect(totp.Secret.AsSpan().ToArray());
                        try {
                            if (!VerifyTotp(key, totp.Algorithm, totp.Digits, totp.Period, code, Now, out ulong step))
                                return Failure(StatusCode.InvalidCredential, ContextCode.Totp, op);
                            if (totp.LastStep is { } previous && step <= previous)
                                return Failure(StatusCode.AlreadyUsed, ContextCode.Totp, op);
                            OperationResult advanced = await totps.TryAdvanceTotpStepAsync(user, totp, step, cancellationToken).ConfigureAwait(false);
                            MutationSucceeded(advanced);
                            return advanced;
                        } finally {
                            CryptographicOperations.ZeroMemory(key);
                        }
                    }

                    if (_store is not IUserRecoveryCodeStore recovery) return Failure(StatusCode.NotSupported, ContextCode.RecoveryCode, op);
                    if (!ValidRecoveryCode(code)) return Failure(StatusCode.InvalidCredential, ContextCode.RecoveryCode, op);
                    bool enrolled = false;
                    await foreach (UserTotp _ in totps.GetTotpsAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                        enrolled = true;
                        break;
                    }
                    if (!enrolled) return Failure(StatusCode.InvalidState, ContextCode.Totp, op);
                    OperationResult result = await recovery.TryConsumeRecoveryCodeAsync(user, UserManager.HashRecoveryCode(code.ToUpperInvariant()), cancellationToken).ConfigureAwait(false);
                    if (result.Status == StatusCode.NotFound && result.Context == ContextCode.Entity)
                        return Failure(StatusCode.InvalidCredential, ContextCode.RecoveryCode, op);
                    if (!MutationSucceeded(result)) return result;
                    return await _users.AuditAsync(user, null, null, UserAuditType.RecoveryCode, UserAuditAction.Consumed, cancellationToken).ConfigureAwait(false);
                }, (user, challenge, payload) => complete(user, challenge, payload.Target!, cancellationToken), cancellationToken, rejectedFactor);
        }

        private async Task<OperationResult<ChallengeTicket>> IssueAsync(Guid uuid, ushort? expectedRotation, UserChallengeType type, Payload payload, bool numeric, Guid? actor,
            OperationCode op, Func<User, Task<OperationResult>>? validate, CancellationToken cancellationToken) {
            try {
                if (_store is not IUserChallengeStore challenges) return RejectTicket(StatusCode.NotSupported, ContextCode.Challenge, op);
                await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                User? found = await _store.FindByUuidAsync(uuid, cancellationToken).ConfigureAwait(false);
                if (found is not { } user) return RejectTicket(StatusCode.NotFound, ContextCode.User, op);
                if (user.Status != UserStatus.Active) return RejectTicket(StatusCode.AccessDenied, ContextCode.User, op);
                if (expectedRotation is { } rotation && rotation != user.Rotation) return RejectTicket(StatusCode.Conflict, ContextCode.User, op);
                if (validate is not null) {
                    OperationResult validation = await validate(user).ConfigureAwait(false);
                    if (!Succeeded(validation)) return new OperationResult<ChallengeTicket>(validation);
                }
                OperationResult<ChallengeTicket> issued = await IssueCoreAsync(challenges, user, type, payload, numeric, actor, op, cancellationToken).ConfigureAwait(false);
                if (!issued.HasValue) return issued;
                OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return Succeeded(commit) ? issued : new OperationResult<ChallengeTicket>(commit);
            } finally {
                CryptographicOperations.ZeroMemory(payload.Key);
            }
        }

        private async Task<OperationResult<ChallengeTicket>> IssueCoreAsync(IUserChallengeStore challenges, User user, UserChallengeType type, Payload payload, bool numeric, Guid? actor, OperationCode op, CancellationToken cancellationToken) {
            Guid uuid = Guid.NewGuid();
            DateTime now = Now;
            DateTime expires = now + Lifetime(type);
            bool enrollment = type == UserChallengeType.TotpEnrollment;
            string secret = enrollment ? Base32Encoding.ToString(payload.Key) : numeric
                ? RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture)
                : Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            ImmutableArray<byte>? hash = enrollment ? null : ImmutableArray.CreateRange(ComputeVerifier(payload.Key, secret));
            ImmutableArray<byte> protectedPayload = ProtectPayload(user.Uuid, uuid, type, payload);
            var challenge = new UserChallenge(uuid, type, hash, protectedPayload, 0, _options.MaxAttempts, now, expires);

            // Keep one pending challenge per user and purpose.
            OperationResult result = await challenges.RemoveChallengesAsync(user, type, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return new OperationResult<ChallengeTicket>(result);
            result = await challenges.AddChallengeAsync(user, challenge, cancellationToken).ConfigureAwait(false);
            if (!MutationSucceeded(result)) return new OperationResult<ChallengeTicket>(result);
            result = await _users.AuditAsync(user, actor, uuid, UserAuditType.Challenge, UserAuditAction.Created, cancellationToken).ConfigureAwait(false);
            if (!Succeeded(result)) return new OperationResult<ChallengeTicket>(result);

            return new OperationResult<ChallengeTicket>(Success(op), new ChallengeTicket(uuid, type, secret, expires));
        }

        private async Task<OperationResult> CompleteAsync(Guid userUuid, Guid challengeUuid, UserChallengeType type, string proof, OperationCode op, Guid? actor,
            Func<User, UserChallenge, Payload, Task<OperationResult>>? verify,
            Func<User, UserChallenge, Payload, Task<OperationResult>> apply, CancellationToken cancellationToken,
            Func<User, CancellationToken, Task<OperationResult>>? rejectedFactor = null) {
            if (_store is not IUserChallengeStore challenges) return Failure(StatusCode.NotSupported, ContextCode.Challenge, op);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(userUuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return Failure(StatusCode.NotFound, ContextCode.User, op);
            if (user.Status != UserStatus.Active) return Failure(StatusCode.AccessDenied, ContextCode.User, op);
            UserChallenge? pending = await challenges.GetChallengeAsync(user, challengeUuid, cancellationToken).ConfigureAwait(false);
            if (pending is not { } challenge || challenge.Type != type) return Failure(StatusCode.NotFound, ContextCode.Challenge, op);

            OperationResult result = await challenges.TryReserveChallengeAttemptAsync(user, challenge, Now, cancellationToken).ConfigureAwait(false);
            if (!MutationSucceeded(result)) return result;
            Payload payload = UnprotectPayload(user.Uuid, challenge);
            try {
                bool validContinuation = type == UserChallengeType.TotpEnrollment || VerifyProof(challenge, payload.Key, proof);
                if (!validContinuation) {
                    result = Failure(StatusCode.InvalidCredential, ContextCode.Challenge, op);
                } else if (verify is not null) {
                    result = await verify(user, challenge, payload).ConfigureAwait(false);
                } else {
                    result = Success(op);
                }

                if (!Succeeded(result)) {
                    // Only rejected proof/replay commits an attempt without the identity mutation.
                    if (result.Status is not (StatusCode.InvalidCredential or StatusCode.AlreadyUsed)) return result;
                    // Only a valid continuation can increment the account-wide MFA failure count.
                    if (validContinuation && rejectedFactor is not null) {
                        OperationResult recorded = await rejectedFactor(user, cancellationToken).ConfigureAwait(false);
                        if (!Succeeded(recorded)) return recorded;
                    }
                    OperationResult failureCommit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return Succeeded(failureCommit) ? result : failureCommit;
                }

                result = await challenges.TryConsumeChallengeAsync(user, challenge, Now, cancellationToken).ConfigureAwait(false);
                if (!MutationSucceeded(result)) return result;
                result = await apply(user, challenge, payload).ConfigureAwait(false);
                if (!Succeeded(result)) return result;
                result = await _users.AuditAsync(user, actor, challengeUuid, UserAuditType.Challenge, UserAuditAction.Consumed, cancellationToken).ConfigureAwait(false);
                if (!Succeeded(result)) return result;
                result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return Succeeded(result) ? Success(op) : result;
            } finally {
                CryptographicOperations.ZeroMemory(payload.Key);
            }
        }

        private async Task<OperationResult> ValidateResetDestinationAsync(User user, Payload payload, OperationCode op, CancellationToken cancellationToken) {
            if (user.Password is null) return Failure(StatusCode.InvalidState, ContextCode.Password, op);
            if (payload.IsPhone) {
                if (_store is not IUserPhoneStore phones) return Failure(StatusCode.NotSupported, ContextCode.Phone, op);
                await foreach (UserPhone phone in phones.GetPhonesAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                    if (phone.Number == payload.Target && phone.Extension == payload.Secondary) return Success(op);
                }
                return Failure(StatusCode.NotFound, ContextCode.Phone, op);
            }
            if (_store is not IUserEmailStore emails) return Failure(StatusCode.NotSupported, ContextCode.Email, op);
            await foreach (UserEmail email in emails.GetEmailsAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                if (email.Address == payload.Target) return Success(op);
            }
            return Failure(StatusCode.NotFound, ContextCode.Email, op);
        }

        private static Payload ContactPayload(UserEmail email) => new() {
            Key = RandomNumberGenerator.GetBytes(32), Target = email.Address, Primary = email.Primary
        };

        private static Payload ContactPayload(UserPhone phone) => new() {
            Key = RandomNumberGenerator.GetBytes(32), Target = phone.Number, Secondary = phone.Extension, Primary = phone.Primary, IsPhone = true
        };

        private IDataProtector PayloadProtector(Guid user, Guid challenge, UserChallengeType type) =>
            _protection.CreateProtector("Lithindium.Identity.Challenge.v1").CreateProtector(user.ToString("N")).CreateProtector(challenge.ToString("N")).CreateProtector(type.ToString());

        private IDataProtector TotpProtector(Guid user, Guid totp) =>
            _protection.CreateProtector("Lithindium.Identity.Totp.v1").CreateProtector(user.ToString("N")).CreateProtector(totp.ToString("N"));

        private ImmutableArray<byte> ProtectPayload(Guid user, Guid challenge, UserChallengeType type, Payload payload) {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(payload.Key.Length);
            writer.Write(payload.Key);
            WriteNullable(writer, payload.Target);
            WriteNullable(writer, payload.Secondary);
            writer.Write(payload.Primary);
            writer.Write(payload.IsPhone);
            writer.Write((byte)payload.Algorithm);
            writer.Write(payload.Digits);
            writer.Write(payload.Period);
            writer.Flush();
            byte[] clear = stream.ToArray();
            try {
                return ImmutableArray.CreateRange(PayloadProtector(user, challenge, type).Protect(clear));
            } finally {
                CryptographicOperations.ZeroMemory(clear);
                if (stream.TryGetBuffer(out ArraySegment<byte> buffer)) CryptographicOperations.ZeroMemory(buffer.AsSpan());
            }
        }

        private Payload UnprotectPayload(Guid user, UserChallenge challenge) {
            if (challenge.Payload is not { } protectedPayload || protectedPayload.IsDefaultOrEmpty)
                throw new InvalidOperationException("Missing challenge payload.");
            byte[] clear = PayloadProtector(user, challenge.Uuid, challenge.Type).Unprotect(protectedPayload.AsSpan().ToArray());
            byte[]? key = null;
            try {
                using var stream = new MemoryStream(clear, writable: false);
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                int length = reader.ReadInt32();
                if (length is not (20 or 32 or 64)) throw new InvalidOperationException("Invalid challenge key length.");
                key = reader.ReadBytes(length);
                if (key.Length != length) throw new InvalidOperationException("Truncated challenge key.");
                var payload = new Payload {
                    Key = key,
                    Target = ReadNullable(reader),
                    Secondary = ReadNullable(reader),
                    Primary = reader.ReadBoolean(),
                    IsPhone = reader.ReadBoolean(),
                    Algorithm = (UserTotpAlgorithm)reader.ReadByte(),
                    Digits = reader.ReadByte(),
                    Period = reader.ReadByte()
                };
                if (stream.Position != stream.Length) throw new InvalidOperationException("Unexpected challenge payload data.");
                key = null;
                return payload;
            } finally {
                CryptographicOperations.ZeroMemory(clear);
                if (key is not null) CryptographicOperations.ZeroMemory(key);
            }
        }

        private static void WriteNullable(BinaryWriter writer, string? value) {
            writer.Write(value is not null);
            if (value is not null) writer.Write(value);
        }

        private static string? ReadNullable(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadString() : null;

        private static byte[] ComputeVerifier(byte[] key, string proof) {
            byte[] bytes = Encoding.UTF8.GetBytes(proof);
            try {
                return HMACSHA256.HashData(key, bytes);
            } finally {
                CryptographicOperations.ZeroMemory(bytes);
            }
        }

        private static bool VerifyProof(UserChallenge challenge, byte[] key, string proof) {
            if (string.IsNullOrEmpty(proof) || proof.Length > 64 || challenge.SecretHash is not { } hash || hash.IsDefaultOrEmpty) return false;
            byte[] computed = ComputeVerifier(key, proof);
            try {
                return CryptographicOperations.FixedTimeEquals(computed, hash.AsSpan());
            } finally {
                CryptographicOperations.ZeroMemory(computed);
            }
        }

        private static bool VerifyTotp(byte[] key, UserTotpAlgorithm algorithm, byte digits, byte period, string code, DateTime now, out ulong step) {
            step = 0;
            if (string.IsNullOrEmpty(code) || code.Length != digits) return false;
            foreach (char c in code) if (c is < '0' or > '9') return false;
            var totp = new Totp(key, step: period, mode: algorithm switch {
                UserTotpAlgorithm.Sha1 => OtpHashMode.Sha1,
                UserTotpAlgorithm.Sha256 => OtpHashMode.Sha256,
                UserTotpAlgorithm.Sha512 => OtpHashMode.Sha512,
                _ => throw new InvalidOperationException("Unsupported TOTP algorithm.")
            }, totpSize: digits);
            if (!totp.VerifyTotp(now, code, out long matched, new VerificationWindow(previous: 1, future: 1)) || matched < 0) return false;
            step = (ulong)matched;
            return true;
        }

        private static bool ValidRecoveryCode(string code) {
            if (code is not { Length: 32 }) return false;
            foreach (char c in code) if (!Uri.IsHexDigit(c)) return false;
            return true;
        }

        private DateTime Now => _users.Clock.GetUtcNow().UtcDateTime;

        private TimeSpan Lifetime(UserChallengeType type) => type switch {
            UserChallengeType.EmailVerification or UserChallengeType.PhoneVerification => _options.ContactLifetime,
            UserChallengeType.PasswordReset => _options.PasswordResetLifetime,
            UserChallengeType.TotpEnrollment => _options.TotpEnrollmentLifetime,
            UserChallengeType.MultiFactorAuthentication => _options.MultiFactorLifetime,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        private static bool ValidLifetime(TimeSpan duration) => duration > TimeSpan.Zero && duration <= TimeSpan.FromHours(1);

        private static bool Succeeded(OperationResult result) => UserManager.StoreSucceeded(result);

        private static bool MutationSucceeded(OperationResult result) {
            if (result.Status == StatusCode.NoOp) throw new InvalidOperationException("Required challenge mutation was not applied.");
            return Succeeded(result);
        }

        private static OperationResult Success(OperationCode op) => new(StatusCode.Success, operation: op);

        private static OperationResult Failure(StatusCode status, ContextCode context, OperationCode op, DetailCode detail = DetailCode.None) => new(status, context, op, detail);

        private static OperationResult<ChallengeTicket> RejectTicket(StatusCode status, ContextCode context, OperationCode op) => new(Failure(status, context, op));

        private static Task<OperationResult<ChallengeTicket>> RejectedTicket(StatusCode status, ContextCode context, OperationCode op) => Task.FromResult(RejectTicket(status, context, op));

        private sealed class Payload {

            public required byte[] Key { get; init; }
            public string? Target { get; init; }
            public string? Secondary { get; init; }
            public bool Primary { get; init; }
            public bool IsPhone { get; init; }
            public UserTotpAlgorithm Algorithm { get; init; }
            public byte Digits { get; init; }
            public byte Period { get; init; }

        }

    }

}
