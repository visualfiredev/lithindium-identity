// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Manages users and their credentials.
    /// </summary>
    /// <remarks>
    /// Trusted backend API; callers enforce authorization and reauthentication.
    /// Use a scoped store and do not invoke concurrent operations on this instance.
    /// Store failures retain their original operation code. Manager successes use manager codes.
    /// Returned records may contain sensitive material. Never expose them directly to clients.
    /// </remarks>
    public class UserManager {

        private readonly IUserStore _store;
        private readonly IPasswordHasher<object> _passwordHasher;
        private readonly UserManagerOptions _options;
        private readonly TimeProvider _timeProvider;

        internal IUserStore Store => _store;
        internal TimeProvider Clock => _timeProvider;

        /// <summary>
        /// Creates a user manager.
        /// </summary>
        /// <remarks>
        /// Custom hashers must depend only on stable user identity, not mutable snapshot fields.
        /// </remarks>
        public UserManager(IUserStore store, IPasswordHasher<object>? passwordHasher = null, UserManagerOptions? options = null, TimeProvider? timeProvider = null) {
            ArgumentNullException.ThrowIfNull(store);

            _store = store;
            _passwordHasher = passwordHasher ?? new PasswordHasher<object>();
            _options = options ?? new UserManagerOptions();
            _timeProvider = timeProvider ?? TimeProvider.System;

            if (_options.MinimumPasswordLength < 1 || _options.MaximumPasswordLength < _options.MinimumPasswordLength || _options.MaximumPasswordLength > 1024 ||
                _options.RecoveryCodeCount is < 1 or > 100) {
                throw new ArgumentOutOfRangeException(nameof(options));
            }
        }

        /// <summary>
        /// Finds a user by UUID.
        /// </summary>
        public virtual Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            _store.FindByUuidAsync(uuid, cancellationToken);

        /// <summary>
        /// Finds a user by username.
        /// </summary>
        public virtual Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            _store.FindByUsernameAsync(username, cancellationToken);

        /// <summary>
        /// Creates a native account.
        /// </summary>
        public virtual Task<OperationResult<User>> CreateAsync(string username, string password, string? displayName = null, Guid? actor = null, CancellationToken cancellationToken = default) =>
            CreateCoreAsync(username, password, displayName, null, actor, OperationCode.UserCreateUser, cancellationToken);

        /// <summary>
        /// Creates a passwordless account with a validated external identity.
        /// </summary>
        /// <remarks>
        /// Caller must validate the external provider assertion and authorize registration.
        /// Email claims do not establish verified local email ownership.
        /// </remarks>
        public virtual Task<OperationResult<User>> CreateExternalAsync(string username, UserExternalIdentity identity, string? displayName = null, Guid? actor = null, CancellationToken cancellationToken = default) =>
            CreateCoreAsync(username, null, displayName, identity, actor, OperationCode.UserCreateExternalUser, cancellationToken);

        /// <summary>
        /// Updates a user's profile.
        /// </summary>
        public virtual Task<OperationResult> UpdateProfileAsync(Guid uuid, string username, string? displayName, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserUpdateProfile;
            OperationResult validation = ValidateProfile(username, displayName, op);
            if (validation.IsFailure) return Task.FromResult(validation);

            return MutateAsync(uuid, op, async user => {
                if (user.Username == username && user.DisplayName == displayName) return Success(op, true);
                OperationResult result = await _store.TryUpdateAsync(user, user with { Username = username, DisplayName = displayName }, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, user.Uuid, UserAuditType.User, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Deletes a user and dependent identity state.
        /// </summary>
        /// <remarks>
        /// Application-specific ownership cleanup remains the caller's responsibility.
        /// </remarks>
        public virtual Task<OperationResult> DeleteAsync(Guid uuid, Guid? actor = null, CancellationToken cancellationToken = default) =>
            MutateAsync(uuid, OperationCode.UserDeleteUser, async user => {
                OperationResult result = await _store.DeleteAsync(user.Uuid, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, user.Uuid, UserAuditType.User, UserAuditAction.Deleted, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Changes a native password after verifying the current password.
        /// </summary>
        /// <remarks>
        /// Caller may preserve only its authorized current session. This is not sign-in.
        /// </remarks>
        public virtual Task<OperationResult> ChangePasswordAsync(Guid uuid, string currentPassword, string newPassword, Guid? exceptSessionUuid = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserChangePassword;
            OperationResult validation = ValidatePassword(newPassword, op);
            if (validation.IsFailure) return Task.FromResult(validation);
            if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length > 1024) return Task.FromResult(Failure(StatusCode.InvalidCredential, ContextCode.Password, op));

            return MutateAsync(uuid, op, async user => {
                if (user.Status != UserStatus.Active) return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.Suspended);
                if (user.Password is null || VerifyPassword(user, currentPassword) is not (PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded))
                    return Failure(StatusCode.InvalidCredential, ContextCode.Password, op);

                return await RotateAsync(user, user with { Password = HashPassword(user, newPassword) }, exceptSessionUuid, op, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Sets account status and invalidates authentication on change.
        /// </summary>
        /// <remarks>
        /// Suspension remains possible at maximum rotation; reactivation requires a new generation.
        /// </remarks>
        public virtual Task<OperationResult> SetStatusAsync(Guid uuid, UserStatus status, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserSetStatus;
            if (status is not (UserStatus.Active or UserStatus.Suspended)) return Task.FromResult(new OperationResult(StatusCode.InvalidArgument, ContextCode.User, op, (byte)1));

            return MutateAsync(uuid, op, async user => {
                if (user.Status == status) return Success(op, true);
                OperationResult result = await RotateAsync(user, user with { Status = status }, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, user.Uuid, UserAuditType.User, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Invalidates outstanding authentication and verification state.
        /// </summary>
        public virtual Task<OperationResult> InvalidateAuthenticationAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            MutateAsync(uuid, OperationCode.UserInvalidateAuthentication, user => RotateAsync(user, user, null, OperationCode.UserInvalidateAuthentication, cancellationToken), cancellationToken);

        /// <summary>
        /// Gets a user's verified emails.
        /// </summary>
        public virtual Task<OperationResult<ImmutableArray<UserEmail>>> GetEmailsAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserEmailStore, ImmutableArray<UserEmail>>(uuid, OperationCode.UserGetEmails, ContextCode.Email,
                (store, user) => CollectAsync(store.GetEmailsAsync(user, cancellationToken), cancellationToken), cancellationToken);

        /// <summary>
        /// Gets a user's verified phones.
        /// </summary>
        public virtual Task<OperationResult<ImmutableArray<UserPhone>>> GetPhonesAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserPhoneStore, ImmutableArray<UserPhone>>(uuid, OperationCode.UserGetPhones, ContextCode.Phone,
                (store, user) => CollectAsync(store.GetPhonesAsync(user, cancellationToken), cancellationToken), cancellationToken);

        /// <summary>
        /// Gets a user's TOTP authenticators.
        /// </summary>
        public virtual Task<OperationResult<ImmutableArray<UserTotp>>> GetTotpsAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserTotpStore, ImmutableArray<UserTotp>>(uuid, OperationCode.UserGetTotps, ContextCode.Totp,
                (store, user) => CollectAsync(store.GetTotpsAsync(user, cancellationToken), cancellationToken), cancellationToken);

        /// <summary>
        /// Gets a user's external identities.
        /// </summary>
        public virtual Task<OperationResult<ImmutableArray<UserExternalIdentity>>> GetExternalIdentitiesAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserExternalIdentityStore, ImmutableArray<UserExternalIdentity>>(uuid, OperationCode.UserGetExternalIdentities, ContextCode.ExternalIdentity,
                (store, user) => CollectAsync(store.GetExternalIdentitiesAsync(user, cancellationToken), cancellationToken), cancellationToken);

        /// <summary>
        /// Counts unused recovery codes.
        /// </summary>
        public virtual Task<OperationResult<int>> CountRecoveryCodesAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserRecoveryCodeStore, int>(uuid, OperationCode.UserCountRecoveryCodes, ContextCode.RecoveryCode,
                (store, user) => store.CountRecoveryCodesAsync(user, cancellationToken), cancellationToken);

        /// <summary>
        /// Adds an independently verified email.
        /// </summary>
        /// <remarks>
        /// Trusted callers must establish ownership before invoking this method.
        /// Do not use this method to complete a persisted challenge; consumption and
        /// credential addition must share the challenge transaction.
        /// </remarks>
        public virtual Task<OperationResult> AddVerifiedEmailAsync(Guid uuid, UserEmail email, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserAddVerifiedEmail;
            if (!ValidEmail(email)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Email, op));

            return MutateCapabilityAsync<IUserEmailStore>(uuid, op, ContextCode.Email, async (store, user) => {
                OperationResult result = await store.AddEmailAsync(user, email, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Email, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Removes a verified email.
        /// </summary>
        public virtual Task<OperationResult> RemoveEmailAsync(Guid uuid, UserEmail email, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserRemoveEmail;
            if (!ValidEmail(email)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Email, op));

            return MutateCapabilityAsync<IUserEmailStore>(uuid, op, ContextCode.Email, async (store, user) => {
                OperationResult result = await store.RemoveEmailAsync(user, email, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Email, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Sets or clears the primary email.
        /// </summary>
        public virtual Task<OperationResult> SetPrimaryEmailAsync(Guid uuid, UserEmail? email, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserSetPrimaryEmail;
            if (email is { } value && !ValidEmail(value)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Email, op));

            return MutateCapabilityAsync<IUserEmailStore>(uuid, op, ContextCode.Email, async (store, user) => {
                OperationResult result = await store.SetPrimaryEmailAsync(user, email, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result) || result.Status == StatusCode.NoOp) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Email, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Adds an independently verified phone.
        /// </summary>
        /// <remarks>
        /// Trusted callers must establish ownership before invoking this method.
        /// Do not use this method to complete a persisted challenge; consumption and
        /// credential addition must share the challenge transaction.
        /// </remarks>
        public virtual Task<OperationResult> AddVerifiedPhoneAsync(Guid uuid, UserPhone phone, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserAddVerifiedPhone;
            if (!ValidPhone(phone)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Phone, op));

            return MutateCapabilityAsync<IUserPhoneStore>(uuid, op, ContextCode.Phone, async (store, user) => {
                OperationResult result = await store.AddPhoneAsync(user, phone, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Phone, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Removes a verified phone.
        /// </summary>
        public virtual Task<OperationResult> RemovePhoneAsync(Guid uuid, UserPhone phone, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserRemovePhone;
            if (!ValidPhone(phone)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Phone, op));

            return MutateCapabilityAsync<IUserPhoneStore>(uuid, op, ContextCode.Phone, async (store, user) => {
                OperationResult result = await store.RemovePhoneAsync(user, phone, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Phone, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Sets or clears the primary phone.
        /// </summary>
        public virtual Task<OperationResult> SetPrimaryPhoneAsync(Guid uuid, UserPhone? phone, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserSetPrimaryPhone;
            if (phone is { } value && !ValidPhone(value)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Phone, op));

            return MutateCapabilityAsync<IUserPhoneStore>(uuid, op, ContextCode.Phone, async (store, user) => {
                OperationResult result = await store.SetPrimaryPhoneAsync(user, phone, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result) || result.Status == StatusCode.NoOp) return result;
                return await AuditAsync(user, actor, null, UserAuditType.Phone, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Adds an independently verified, protected TOTP authenticator.
        /// </summary>
        /// <remarks>
        /// Trusted callers must verify enrollment and protect the secret before calling.
        /// Do not use this method to complete a persisted challenge. Imported secrets must
        /// use the same data protection provider and purposes as ChallengeManager:
        /// Lithindium.Identity.Totp.v1, user UUID (N), authenticator UUID (N).
        /// </remarks>
        public virtual Task<OperationResult> AddVerifiedTotpAsync(Guid uuid, UserTotp totp, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserAddVerifiedTotp;
            if (totp.Uuid == Guid.Empty || totp.Secret.IsDefaultOrEmpty || totp.Secret.Length > 255 ||
                !Enum.IsDefined(totp.Algorithm) || totp.Digits is < 6 or > 8 || totp.Period == 0 || totp.LastStep is null ||
                !ValidOptionalName(totp.Name, 32))
                return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Totp, op));

            return MutateCapabilityAsync<IUserTotpStore>(uuid, op, ContextCode.Totp, async (store, user) => {
                OperationResult result = await store.AddTotpAsync(user, totp, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, totp.Uuid, UserAuditType.Totp, UserAuditAction.Verified, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Renames a TOTP authenticator.
        /// </summary>
        public virtual Task<OperationResult> RenameTotpAsync(Guid uuid, Guid totpUuid, string? name, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserRenameTotp;
            if (!ValidOptionalName(name, 32)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.Totp, op));

            return MutateCapabilityAsync<IUserTotpStore>(uuid, op, ContextCode.Totp, async (store, user) => {
                UserTotp? totp = await store.GetTotpAsync(user, totpUuid, cancellationToken).ConfigureAwait(false);
                if (totp is not { } current) return Failure(StatusCode.NotFound, ContextCode.Totp, op);
                if (current.Name == name) return Success(op, true);
                OperationResult result = await store.TryRenameTotpAsync(user, current, name, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, totpUuid, UserAuditType.Totp, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Removes a TOTP authenticator and revokes authentication.
        /// </summary>
        public virtual Task<OperationResult> RemoveTotpAsync(Guid uuid, Guid totpUuid, Guid? actor = null, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserTotpStore>(uuid, OperationCode.UserRemoveTotp, ContextCode.Totp, async (store, user) => {
                const OperationCode op = OperationCode.UserRemoveTotp;
                UserTotp? totp = await store.GetTotpAsync(user, totpUuid, cancellationToken).ConfigureAwait(false);
                if (totp is not { } current) return Failure(StatusCode.NotFound, ContextCode.Totp, op);
                OperationResult result = await store.RemoveTotpAsync(user, current, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                if (_store is IUserRecoveryCodeStore recovery && !await AnyAsync(store.GetTotpsAsync(user, cancellationToken), cancellationToken).ConfigureAwait(false)) {
                    result = await recovery.ReplaceRecoveryCodesAsync(user, Array.Empty<UserRecoveryCode>(), cancellationToken).ConfigureAwait(false);
                    if (!StoreSucceeded(result)) return result;
                }
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, totpUuid, UserAuditType.Totp, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Links a validated external provider identity.
        /// </summary>
        /// <remarks>
        /// Caller must validate the provider assertion; never link based on an email claim.
        /// </remarks>
        public virtual Task<OperationResult> LinkExternalIdentityAsync(Guid uuid, UserExternalIdentity identity, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserLinkExternalIdentity;
            if (!ValidExternalIdentity(identity)) return Task.FromResult(Failure(StatusCode.MalformedInput, ContextCode.ExternalIdentity, op));

            return MutateCapabilityAsync<IUserExternalIdentityStore>(uuid, op, ContextCode.ExternalIdentity, async (store, user) => {
                OperationResult result = await store.AddExternalIdentityAsync(user, identity, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, identity.Uuid, UserAuditType.ExternalIdentity, UserAuditAction.Created, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Updates external identity metadata without changing issuer or subject.
        /// </summary>
        public virtual Task<OperationResult> UpdateExternalIdentityAsync(Guid uuid, Guid identityUuid, string? name, string? email, Guid? actor = null, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserExternalIdentityStore>(uuid, OperationCode.UserUpdateExternalIdentity, ContextCode.ExternalIdentity, async (store, user) => {
                const OperationCode op = OperationCode.UserUpdateExternalIdentity;
                UserExternalIdentity? identity = await store.GetExternalIdentityAsync(user, identityUuid, cancellationToken).ConfigureAwait(false);
                if (identity is not { } current) return Failure(StatusCode.NotFound, ContextCode.ExternalIdentity, op);
                UserExternalIdentity replacement = current with { Name = name, Email = email };
                if (!ValidExternalIdentity(replacement)) return Failure(StatusCode.MalformedInput, ContextCode.ExternalIdentity, op);
                if (replacement == current) return Success(op, true);
                OperationResult result = await store.TryUpdateExternalIdentityAsync(user, current, replacement, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, identityUuid, UserAuditType.ExternalIdentity, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Removes an external identity without removing the last sign-in credential.
        /// </summary>
        public virtual Task<OperationResult> RemoveExternalIdentityAsync(Guid uuid, Guid identityUuid, Guid? actor = null, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserExternalIdentityStore>(uuid, OperationCode.UserRemoveExternalIdentity, ContextCode.ExternalIdentity, async (store, user) => {
                const OperationCode op = OperationCode.UserRemoveExternalIdentity;
                UserExternalIdentity? identity = await store.GetExternalIdentityAsync(user, identityUuid, cancellationToken).ConfigureAwait(false);
                if (identity is not { } current) return Failure(StatusCode.NotFound, ContextCode.ExternalIdentity, op);
                if (user.Password is null) {
                    bool other = false;
                    await foreach (UserExternalIdentity item in store.GetExternalIdentitiesAsync(user, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false)) {
                        if (item.Uuid != identityUuid) { other = true; break; }
                    }
                    if (!other) return Failure(StatusCode.PolicyViolation, ContextCode.ExternalIdentity, op, DetailCode.LastCredential);
                }
                OperationResult result = await store.RemoveExternalIdentityAsync(user, current, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, identityUuid, UserAuditType.ExternalIdentity, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Replaces recovery codes and returns plaintext once, after commit.
        /// </summary>
        public virtual async Task<OperationResult<ImmutableArray<string>>> GenerateRecoveryCodesAsync(Guid uuid, Guid? actor = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserGenerateRecoveryCodes;
            ImmutableArray<string> plaintext = ImmutableArray<string>.Empty;
            OperationResult result = await MutateCapabilityAsync<IUserRecoveryCodeStore>(uuid, op, ContextCode.RecoveryCode, async (store, user) => {
                if (_store is not IUserTotpStore totps) return Failure(StatusCode.NotSupported, ContextCode.Totp, op);
                if (!await AnyAsync(totps.GetTotpsAsync(user, cancellationToken), cancellationToken).ConfigureAwait(false))
                    return Failure(StatusCode.InvalidState, ContextCode.Totp, op);
                var codes = ImmutableArray.CreateBuilder<UserRecoveryCode>(_options.RecoveryCodeCount);
                var secrets = ImmutableArray.CreateBuilder<string>(_options.RecoveryCodeCount);
                for (int i = 0; i < _options.RecoveryCodeCount; i++) {
                    string code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                    secrets.Add(code);
                    codes.Add(new UserRecoveryCode(Guid.CreateVersion7(), HashRecoveryCode(code)));
                }
                OperationResult write = await store.ReplaceRecoveryCodesAsync(user, codes.MoveToImmutable(), cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(write)) return write;
                write = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(write)) return write;
                write = await AuditAsync(user, actor, null, UserAuditType.RecoveryCode, UserAuditAction.Updated, cancellationToken).ConfigureAwait(false);
                if (StoreSucceeded(write)) plaintext = secrets.MoveToImmutable();
                return write;
            }, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess ? new OperationResult<ImmutableArray<string>>(result, plaintext) : new OperationResult<ImmutableArray<string>>(result);
        }

        /// <summary>
        /// Removes all recovery codes.
        /// </summary>
        public virtual Task<OperationResult> ClearRecoveryCodesAsync(Guid uuid, Guid? actor = null, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserRecoveryCodeStore>(uuid, OperationCode.UserClearRecoveryCodes, ContextCode.RecoveryCode, async (store, user) => {
                const OperationCode op = OperationCode.UserClearRecoveryCodes;
                if (await store.CountRecoveryCodesAsync(user, cancellationToken).ConfigureAwait(false) == 0) return Success(op, true);
                OperationResult result = await store.ReplaceRecoveryCodesAsync(user, Array.Empty<UserRecoveryCode>(), cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                result = await RotateAsync(user, user, null, op, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
                return await AuditAsync(user, actor, null, UserAuditType.RecoveryCode, UserAuditAction.Removed, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Enables or disables authentication lockout.
        /// </summary>
        public virtual Task<OperationResult> SetLockoutEnabledAsync(Guid uuid, bool enabled, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserLockoutStore>(uuid, OperationCode.UserSetLockoutEnabled, ContextCode.Lockout, async (store, user) => {
                UserLockout? current = await store.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
                if (current is { } state && state.Enabled == enabled) return Success(OperationCode.UserSetLockoutEnabled, true);
                return await store.TryUpdateLockoutAsync(user, current, new UserLockout(enabled, 0, null), cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Resets failed authentication attempts and any active lockout.
        /// </summary>
        public virtual Task<OperationResult> ResetLockoutAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            MutateCapabilityAsync<IUserLockoutStore>(uuid, OperationCode.UserResetLockout, ContextCode.Lockout, async (store, user) => {
                UserLockout? current = await store.GetLockoutAsync(user, cancellationToken).ConfigureAwait(false);
                if (current is not { } state || (state.Failures == 0 && state.ReleaseAt is null)) return Success(OperationCode.UserResetLockout, true);
                return await store.TryUpdateLockoutAsync(user, current, state with { Failures = 0, ReleaseAt = null }, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);

        /// <summary>
        /// Sets the first native password after independent reauthentication.
        /// </summary>
        /// <remarks>
        /// Caller must verify an existing credential; cannot replace an existing password.
        /// </remarks>
        public virtual Task<OperationResult> SetPasswordAsync(Guid uuid, string password, Guid? exceptSessionUuid = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserSetPassword;
            OperationResult validation = ValidatePassword(password, op);
            if (validation.IsFailure) return Task.FromResult(validation);

            return MutateAsync(uuid, op, async user => {
                if (user.Status != UserStatus.Active) return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.Suspended);
                if (user.Password is not null) return Failure(StatusCode.AlreadyExists, ContextCode.Password, op);
                if (_store is not IUserExternalIdentityStore external) return Failure(StatusCode.NotSupported, ContextCode.ExternalIdentity, op);
                if (!await AnyAsync(external.GetExternalIdentitiesAsync(user, cancellationToken), cancellationToken).ConfigureAwait(false))
                    return Failure(StatusCode.InvalidState, ContextCode.ExternalIdentity, op);
                return await RotateAsync(user, user with { Password = HashPassword(user, password) }, exceptSessionUuid, op, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Removes a password when another sign-in credential exists.
        /// </summary>
        public virtual Task<OperationResult> RemovePasswordAsync(Guid uuid, string currentPassword, Guid? exceptSessionUuid = null, CancellationToken cancellationToken = default) {
            const OperationCode op = OperationCode.UserRemovePassword;
            if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length > 1024)
                return Task.FromResult(Failure(StatusCode.InvalidCredential, ContextCode.Password, op));

            return MutateAsync(uuid, op, async user => {
                if (user.Status != UserStatus.Active) return Failure(StatusCode.AccessDenied, ContextCode.User, op, DetailCode.Suspended);
                if (user.Password is null || VerifyPassword(user, currentPassword) is not (PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded))
                    return Failure(StatusCode.InvalidCredential, ContextCode.Password, op);
                if (_store is not IUserExternalIdentityStore external ||
                    !await AnyAsync(external.GetExternalIdentitiesAsync(user, cancellationToken), cancellationToken).ConfigureAwait(false))
                    return Failure(StatusCode.PolicyViolation, ContextCode.Password, op, DetailCode.LastCredential);
                return await RotateAsync(user, user with { Password = null }, exceptSessionUuid, op, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// Gets the persisted lockout state.
        /// </summary>
        public virtual Task<OperationResult<UserLockout?>> GetLockoutAsync(Guid uuid, CancellationToken cancellationToken = default) =>
            ReadAsync<IUserLockoutStore, UserLockout?>(uuid, OperationCode.UserGetLockout, ContextCode.Lockout,
                (store, user) => store.GetLockoutAsync(user, cancellationToken), cancellationToken);

        private async Task<OperationResult<User>> CreateCoreAsync(string username, string? password, string? displayName, UserExternalIdentity? identity, Guid? actor, OperationCode op, CancellationToken cancellationToken) {
            OperationResult result = ValidateProfile(username, displayName, op);
            if (result.IsFailure) return new OperationResult<User>(result);

            if (identity is { } external) {
                if (_store is not IUserExternalIdentityStore) return new OperationResult<User>(Failure(StatusCode.NotSupported, ContextCode.ExternalIdentity, op));
                if (!ValidExternalIdentity(external)) return new OperationResult<User>(Failure(StatusCode.MalformedInput, ContextCode.ExternalIdentity, op));
            } else {
                result = ValidatePassword(password, op);
                if (result.IsFailure) return new OperationResult<User>(result);
            }

            User user = new(Guid.CreateVersion7(), username, null, displayName, 0, UserStatus.Active);
            if (password is not null) user = user with { Password = HashPassword(user, password) };

            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            result = await _store.CreateAsync(user, cancellationToken).ConfigureAwait(false);
            if (!StoreSucceeded(result)) return new OperationResult<User>(result);
            if (identity is { } linked) {
                result = await ((IUserExternalIdentityStore)_store).AddExternalIdentityAsync(user, linked, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return new OperationResult<User>(result);
                result = await AuditAsync(user, actor, linked.Uuid, UserAuditType.ExternalIdentity, UserAuditAction.Created, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return new OperationResult<User>(result);
            }
            if (_store is IUserLockoutStore lockouts) {
                result = await lockouts.TryUpdateLockoutAsync(user, null, new UserLockout(true, 0, null), cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return new OperationResult<User>(result);
            }
            result = await AuditAsync(user, actor, user.Uuid, UserAuditType.User, UserAuditAction.Created, cancellationToken).ConfigureAwait(false);
            if (!StoreSucceeded(result)) return new OperationResult<User>(result);
            result = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return StoreSucceeded(result) ? new OperationResult<User>(Success(op), user) : new OperationResult<User>(result);
        }

        private async Task<OperationResult> MutateAsync(Guid uuid, OperationCode op, Func<User, Task<OperationResult>> action, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (uuid == Guid.Empty) return new OperationResult(StatusCode.InvalidArgument, ContextCode.User, op, (byte)0);
            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(uuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return Failure(StatusCode.NotFound, ContextCode.User, op);
            OperationResult result = await action(user).ConfigureAwait(false);
            if (!StoreSucceeded(result)) return result;
            OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return StoreSucceeded(commit) ? Success(op, result.Status == StatusCode.NoOp) : commit;
        }

        private Task<OperationResult> MutateCapabilityAsync<TStore>(Guid uuid, OperationCode op, ContextCode context, Func<TStore, User, Task<OperationResult>> action, CancellationToken cancellationToken) where TStore : class, IUserStore =>
            _store is TStore store
                ? MutateAsync(uuid, op, user => action(store, user), cancellationToken)
                : Task.FromResult(Failure(StatusCode.NotSupported, context, op));

        private async Task<OperationResult<T>> ReadAsync<TStore, T>(Guid uuid, OperationCode op, ContextCode context, Func<TStore, User, Task<T>> read, CancellationToken cancellationToken) where TStore : class, IUserStore {
            cancellationToken.ThrowIfCancellationRequested();
            if (_store is not TStore store) return new OperationResult<T>(Failure(StatusCode.NotSupported, context, op));
            if (uuid == Guid.Empty) return new OperationResult<T>(new OperationResult(StatusCode.InvalidArgument, ContextCode.User, op, (byte)0));

            await using IUserStoreTransaction transaction = await _store.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            User? found = await _store.FindByUuidAsync(uuid, cancellationToken).ConfigureAwait(false);
            if (found is not { } user) return new OperationResult<T>(Failure(StatusCode.NotFound, ContextCode.User, op));
            T value = await read(store, user).ConfigureAwait(false);
            OperationResult commit = await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return StoreSucceeded(commit) ? new OperationResult<T>(Success(op), value) : new OperationResult<T>(commit);
        }

        internal async Task<OperationResult> RotateAsync(User current, User replacement, Guid? exceptSessionUuid, OperationCode op, CancellationToken cancellationToken) {
            bool exhausted = current.Rotation == ushort.MaxValue;
            // Suspension must remain possible even when the generation is exhausted.
            if (exhausted && !(current.Status == UserStatus.Active && replacement.Status == UserStatus.Suspended))
                return Failure(StatusCode.InvalidState, ContextCode.User, op, DetailCode.RotationExhausted);
            if (exceptSessionUuid is { } sessionUuid) {
                if (_store is not IUserSessionStore sessions) return Failure(StatusCode.NotSupported, ContextCode.Session, op);
                UserSession? session = await sessions.GetSessionAsync(current, sessionUuid, cancellationToken).ConfigureAwait(false);
                if (session is not { } existing) return Failure(StatusCode.NotFound, ContextCode.Session, op);
                if (existing.ExpiresAt <= _timeProvider.GetUtcNow().UtcDateTime) return Failure(StatusCode.Expired, ContextCode.Session, op);
            }

            OperationResult result = await _store.TryUpdateAsync(current, replacement with { Rotation = exhausted ? current.Rotation : (ushort)(current.Rotation + 1) }, cancellationToken).ConfigureAwait(false);
            if (!StoreSucceeded(result)) return result;
            if (result.Status == StatusCode.NoOp) throw new InvalidOperationException("Authentication state update was not applied.");
            if (_store is IUserChallengeStore challenges) {
                result = await challenges.RemoveChallengesAsync(current, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
            }
            if (_store is IUserSessionStore sessionStore) {
                result = await sessionStore.RemoveSessionsAsync(current, exceptSessionUuid, cancellationToken).ConfigureAwait(false);
                if (!StoreSucceeded(result)) return result;
            }
            return Success(op);
        }

        internal Task<OperationResult> AuditAsync(User user, Guid? actor, Guid? entity, UserAuditType type, UserAuditAction action, CancellationToken cancellationToken) =>
            _store is IUserAuditStore audits
                ? audits.AddAuditAsync(new UserAudit(Guid.CreateVersion7(), user.Uuid, actor, entity, type, action, "{}", _timeProvider.GetUtcNow().UtcDateTime), cancellationToken)
                : Task.FromResult(new OperationResult(StatusCode.Success));

        internal PasswordVerificationResult VerifyPassword(User user, string password) =>
            user.Password is null ? PasswordVerificationResult.Failed : _passwordHasher.VerifyHashedPassword(user, user.Password, password);

        internal string HashPassword(User user, string password) {
            string hash = _passwordHasher.HashPassword(user, password);
            if (string.IsNullOrEmpty(hash) || hash.Length > 255) throw new InvalidOperationException("Password hasher returned an invalid hash.");
            return hash;
        }

        internal static ImmutableArray<byte> HashRecoveryCode(string code) =>
            ImmutableArray.CreateRange(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        internal OperationResult ValidatePassword(string? password, OperationCode op) {
            if (string.IsNullOrEmpty(password) || password.Length < _options.MinimumPasswordLength)
                return Failure(StatusCode.PolicyViolation, ContextCode.Password, op, DetailCode.TooShort);
            if (password.Length > _options.MaximumPasswordLength)
                return Failure(StatusCode.PolicyViolation, ContextCode.Password, op, DetailCode.TooLong);
            return Success(op);
        }

        private static OperationResult ValidateProfile(string username, string? displayName, OperationCode op) {
            if (string.IsNullOrEmpty(username)) return Failure(StatusCode.MalformedInput, ContextCode.Username, op);
            if (username.Length > 21) return Failure(StatusCode.PolicyViolation, ContextCode.Username, op, DetailCode.TooLong);
            foreach (char c in username) if (char.IsWhiteSpace(c) || char.IsControl(c)) return Failure(StatusCode.MalformedInput, ContextCode.Username, op);
            if (displayName is { Length: > 32 }) return Failure(StatusCode.PolicyViolation, ContextCode.DisplayName, op, DetailCode.TooLong);
            if (!ValidOptionalName(displayName, 32)) return Failure(StatusCode.MalformedInput, ContextCode.DisplayName, op);
            return Success(op);
        }

        internal static bool ValidOptionalName(string? value, int maxLength) {
            if (value is null) return true;
            if (value.Length > maxLength || string.IsNullOrWhiteSpace(value)) return false;
            foreach (char c in value) if (char.IsControl(c)) return false;
            return true;
        }

        internal static bool ValidEmail(UserEmail email) =>
            !string.IsNullOrWhiteSpace(email.Address) && email.Address.Length <= 254 &&
            MailAddress.TryCreate(email.Address, out MailAddress? parsed) &&
            string.Equals(parsed.Address, email.Address, StringComparison.Ordinal);

        internal static bool ValidPhone(UserPhone phone) {
            if (phone.Number is not { Length: >= 2 and <= 16 } number || number[0] != '+' || number[1] is < '1' or > '9') return false;
            for (int i = 1; i < number.Length; i++) if (number[i] is < '0' or > '9') return false;
            if (phone.Extension is { } extension) {
                if (extension.Length is < 1 or > 10) return false;
                foreach (char c in extension) if (c is < '0' or > '9') return false;
            }
            return true;
        }

        private static bool ValidExternalIdentity(UserExternalIdentity identity) =>
            identity.Uuid != Guid.Empty && ValidExternalPart(identity.Issuer, 512) && ValidExternalPart(identity.Subject, 255) &&
            ValidOptionalName(identity.Name, 255) && (identity.Email is null || ValidEmail(new UserEmail(identity.Email, false)));

        private static bool ValidExternalPart(string value, int maxLength) {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength) return false;
            foreach (char c in value) if (c < 0x20 || c > 0x7E) return false;
            return true;
        }

        private static async Task<ImmutableArray<T>> CollectAsync<T>(IAsyncEnumerable<T> items, CancellationToken cancellationToken) {
            var builder = ImmutableArray.CreateBuilder<T>();
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(false)) builder.Add(item);
            return builder.ToImmutable();
        }

        private static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> items, CancellationToken cancellationToken) {
            await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(false)) return true;
            return false;
        }

        internal static bool StoreSucceeded(OperationResult result) {
            if (result.Status is StatusCode.Success or StatusCode.NoOp) return true;
            if (result.IsFailure) return false;
            throw new InvalidOperationException("Unexpected store result: " + result);
        }

        private static OperationResult Success(OperationCode op, bool noOp = false) =>
            new(noOp ? StatusCode.NoOp : StatusCode.Success, operation: op);

        private static OperationResult Failure(StatusCode status, ContextCode context, OperationCode op, DetailCode detail = DetailCode.None) =>
            new(status, context, op, detail);

    }

}
