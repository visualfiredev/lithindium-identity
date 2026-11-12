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
        StoreRemoveChallenges = 0x54

    }

}
