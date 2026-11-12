// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Lithindium operation codes.
    /// </summary>
    /// <remarks>
    /// Unassigned values are unknown. Existing assignments must remain stable.
    /// </remarks>
    public enum OperationCode : byte {

        /// <summary>
        /// Unknown or unspecified operation.
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// Store operation: CreateAsync.
        /// </summary>
        StoreCreateUser = 0x01,

        /// <summary>
        /// Store operation: TryUpdateAsync.
        /// </summary>
        StoreUpdateUser = 0x02,

        /// <summary>
        /// Store operation: DeleteAsync.
        /// </summary>
        StoreDeleteUser = 0x03,

        /// <summary>
        /// Store operation: CommitAsync.
        /// </summary>
        StoreCommit = 0x04,

        /// <summary>
        /// Store operation: AddEmailAsync.
        /// </summary>
        StoreAddEmail = 0x10,

        /// <summary>
        /// Store operation: RemoveEmailAsync.
        /// </summary>
        StoreRemoveEmail = 0x11,

        /// <summary>
        /// Store operation: SetPrimaryEmailAsync.
        /// </summary>
        StoreSetPrimaryEmail = 0x12,

        /// <summary>
        /// Store operation: AddPhoneAsync.
        /// </summary>
        StoreAddPhone = 0x18,

        /// <summary>
        /// Store operation: RemovePhoneAsync.
        /// </summary>
        StoreRemovePhone = 0x19,

        /// <summary>
        /// Store operation: SetPrimaryPhoneAsync.
        /// </summary>
        StoreSetPrimaryPhone = 0x1A,

        /// <summary>
        /// Store operation: TryUpdateLockoutAsync.
        /// </summary>
        StoreUpdateLockout = 0x20,

        /// <summary>
        /// Store operation: AddTotpAsync.
        /// </summary>
        StoreAddTotp = 0x28,

        /// <summary>
        /// Store operation: RemoveTotpAsync.
        /// </summary>
        StoreRemoveTotp = 0x29,

        /// <summary>
        /// Store operation: TryRenameTotpAsync.
        /// </summary>
        StoreRenameTotp = 0x2A,

        /// <summary>
        /// Store operation: TryAdvanceTotpStepAsync.
        /// </summary>
        StoreAdvanceTotpStep = 0x2B,

        /// <summary>
        /// Store operation: ReplaceRecoveryCodesAsync.
        /// </summary>
        StoreReplaceRecoveryCodes = 0x30,

        /// <summary>
        /// Store operation: TryConsumeRecoveryCodeAsync.
        /// </summary>
        StoreConsumeRecoveryCode = 0x31,

        /// <summary>
        /// Store operation: AddExternalIdentityAsync.
        /// </summary>
        StoreAddExternalIdentity = 0x38,

        /// <summary>
        /// Store operation: TryUpdateExternalIdentityAsync.
        /// </summary>
        StoreUpdateExternalIdentity = 0x39,

        /// <summary>
        /// Store operation: RemoveExternalIdentityAsync.
        /// </summary>
        StoreRemoveExternalIdentity = 0x3A,

        /// <summary>
        /// Store operation: AddSessionAsync.
        /// </summary>
        StoreAddSession = 0x40,

        /// <summary>
        /// Store operation: RemoveSessionAsync.
        /// </summary>
        StoreRemoveSession = 0x41,

        /// <summary>
        /// Store operation: RemoveSessionsAsync.
        /// </summary>
        StoreRemoveSessions = 0x42,

        /// <summary>
        /// Store operation: AddAuditAsync.
        /// </summary>
        StoreAddAudit = 0x48,

        /// <summary>
        /// Store operation: AddChallengeAsync.
        /// </summary>
        StoreAddChallenge = 0x50,

        /// <summary>
        /// Store operation: TryReserveChallengeAttemptAsync.
        /// </summary>
        StoreReserveChallengeAttempt = 0x51,

        /// <summary>
        /// Store operation: TryConsumeChallengeAsync.
        /// </summary>
        StoreConsumeChallenge = 0x52,

        /// <summary>
        /// Store operation: RemoveChallengeAsync.
        /// </summary>
        StoreRemoveChallenge = 0x53,

        /// <summary>
        /// Store operation: RemoveChallengesAsync.
        /// </summary>
        StoreRemoveChallenges = 0x54,

        /// <summary>
        /// User manager: CreateUser.
        /// </summary>
        UserCreateUser = 0x60,

        /// <summary>
        /// User manager: CreateExternalUser.
        /// </summary>
        UserCreateExternalUser = 0x61,

        /// <summary>
        /// User manager: UpdateProfile.
        /// </summary>
        UserUpdateProfile = 0x62,

        /// <summary>
        /// User manager: DeleteUser.
        /// </summary>
        UserDeleteUser = 0x63,

        /// <summary>
        /// User manager: ChangePassword.
        /// </summary>
        UserChangePassword = 0x64,

        /// <summary>
        /// User manager: SetStatus.
        /// </summary>
        UserSetStatus = 0x65,

        /// <summary>
        /// User manager: InvalidateAuthentication.
        /// </summary>
        UserInvalidateAuthentication = 0x66,

        /// <summary>
        /// User manager: GetEmails.
        /// </summary>
        UserGetEmails = 0x67,

        /// <summary>
        /// User manager: GetPhones.
        /// </summary>
        UserGetPhones = 0x68,

        /// <summary>
        /// User manager: GetTotps.
        /// </summary>
        UserGetTotps = 0x69,

        /// <summary>
        /// User manager: GetExternalIdentities.
        /// </summary>
        UserGetExternalIdentities = 0x6A,

        /// <summary>
        /// User manager: CountRecoveryCodes.
        /// </summary>
        UserCountRecoveryCodes = 0x6B,

        /// <summary>
        /// User manager: AddVerifiedEmail.
        /// </summary>
        UserAddVerifiedEmail = 0x6C,

        /// <summary>
        /// User manager: RemoveEmail.
        /// </summary>
        UserRemoveEmail = 0x6D,

        /// <summary>
        /// User manager: SetPrimaryEmail.
        /// </summary>
        UserSetPrimaryEmail = 0x6E,

        /// <summary>
        /// User manager: AddVerifiedPhone.
        /// </summary>
        UserAddVerifiedPhone = 0x6F,

        /// <summary>
        /// User manager: RemovePhone.
        /// </summary>
        UserRemovePhone = 0x70,

        /// <summary>
        /// User manager: SetPrimaryPhone.
        /// </summary>
        UserSetPrimaryPhone = 0x71,

        /// <summary>
        /// User manager: AddVerifiedTotp.
        /// </summary>
        UserAddVerifiedTotp = 0x72,

        /// <summary>
        /// User manager: RenameTotp.
        /// </summary>
        UserRenameTotp = 0x73,

        /// <summary>
        /// User manager: RemoveTotp.
        /// </summary>
        UserRemoveTotp = 0x74,

        /// <summary>
        /// User manager: LinkExternalIdentity.
        /// </summary>
        UserLinkExternalIdentity = 0x75,

        /// <summary>
        /// User manager: UpdateExternalIdentity.
        /// </summary>
        UserUpdateExternalIdentity = 0x76,

        /// <summary>
        /// User manager: RemoveExternalIdentity.
        /// </summary>
        UserRemoveExternalIdentity = 0x77,

        /// <summary>
        /// User manager: GenerateRecoveryCodes.
        /// </summary>
        UserGenerateRecoveryCodes = 0x78,

        /// <summary>
        /// User manager: ClearRecoveryCodes.
        /// </summary>
        UserClearRecoveryCodes = 0x79,

        /// <summary>
        /// User manager: SetLockoutEnabled.
        /// </summary>
        UserSetLockoutEnabled = 0x7A,

        /// <summary>
        /// User manager: ResetLockout.
        /// </summary>
        UserResetLockout = 0x7B,

        /// <summary>
        /// Sets the first password.
        /// </summary>
        UserSetPassword = 0x7C,

        /// <summary>
        /// Removes a password.
        /// </summary>
        UserRemovePassword = 0x7D,

        /// <summary>
        /// Gets lockout state.
        /// </summary>
        UserGetLockout = 0x7E,

        /// <summary>
        /// Challenge manager: IssueEmailVerification.
        /// </summary>
        ChallengeIssueEmailVerification = 0x80,

        /// <summary>
        /// Challenge manager: CompleteEmailVerification.
        /// </summary>
        ChallengeCompleteEmailVerification = 0x81,

        /// <summary>
        /// Challenge manager: IssuePhoneVerification.
        /// </summary>
        ChallengeIssuePhoneVerification = 0x82,

        /// <summary>
        /// Challenge manager: CompletePhoneVerification.
        /// </summary>
        ChallengeCompletePhoneVerification = 0x83,

        /// <summary>
        /// Challenge manager: IssuePasswordReset.
        /// </summary>
        ChallengeIssuePasswordReset = 0x84,

        /// <summary>
        /// Challenge manager: CompletePasswordReset.
        /// </summary>
        ChallengeCompletePasswordReset = 0x85,

        /// <summary>
        /// Challenge manager: IssueTotpEnrollment.
        /// </summary>
        ChallengeIssueTotpEnrollment = 0x86,

        /// <summary>
        /// Challenge manager: CompleteTotpEnrollment.
        /// </summary>
        ChallengeCompleteTotpEnrollment = 0x87,

        /// <summary>
        /// Challenge manager: IssueMultiFactor.
        /// </summary>
        ChallengeIssueMultiFactor = 0x88,

        /// <summary>
        /// Challenge manager: CompleteMultiFactor.
        /// </summary>
        ChallengeCompleteMultiFactor = 0x89,

        /// <summary>
        /// Challenge manager: Cancel.
        /// </summary>
        ChallengeCancel = 0x8A,

        /// <summary>
        /// Challenge manager: RemoveExpired.
        /// </summary>
        ChallengeRemoveExpired = 0x8B,

        /// <summary>
        /// Sign-in manager: Password.
        /// </summary>
        SignInPassword = 0x90,

        /// <summary>
        /// Sign-in manager: External.
        /// </summary>
        SignInExternal = 0x91,

        /// <summary>
        /// Sign-in manager: CompleteMultiFactor.
        /// </summary>
        SignInCompleteMultiFactor = 0x92,

        /// <summary>
        /// Sign-in manager: ValidateSession.
        /// </summary>
        SignInValidateSession = 0x93,

        /// <summary>
        /// Sign-in manager: SignOut.
        /// </summary>
        SignInSignOut = 0x94,

        /// <summary>
        /// Sign-in manager: GetSessions.
        /// </summary>
        SignInGetSessions = 0x95,

        /// <summary>
        /// Sign-in manager: RevokeSession.
        /// </summary>
        SignInRevokeSession = 0x96,

        /// <summary>
        /// Sign-in manager: RevokeSessions.
        /// </summary>
        SignInRevokeSessions = 0x97,

        /// <summary>
        /// Sign-in manager: RemoveExpiredSessions.
        /// </summary>
        SignInRemoveExpiredSessions = 0x98

    }

}
