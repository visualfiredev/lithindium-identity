// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Identity persistence outcome.
    /// </summary>
    /// <remarks>
    /// Expected conflicts return errors; invalid arguments, cancellation and unexpected
    /// infrastructure failures throw.
    /// </remarks>
    public enum UserStoreError {

        /// <summary>
        /// Operation succeeded.
        /// </summary>
        None = 0,

        /// <summary>
        /// User does not exist.
        /// </summary>
        UserNotFound,

        /// <summary>
        /// Entity does not exist or belongs to another user.
        /// </summary>
        EntityNotFound,

        /// <summary>
        /// UUID already used by the same entity type.
        /// </summary>
        DuplicateIdentifier,

        /// <summary>
        /// Username belongs to another user.
        /// </summary>
        DuplicateUsername,

        /// <summary>
        /// Email already associated with a user.
        /// </summary>
        DuplicateEmail,

        /// <summary>
        /// Phone number and extension already associated with a user.
        /// </summary>
        DuplicatePhone,

        /// <summary>
        /// Issuer and subject already associated with a user.
        /// </summary>
        DuplicateExternalIdentity,

        /// <summary>
        /// Recovery code hash duplicates an existing code outside the replaced set or another replacement code.
        /// </summary>
        DuplicateRecoveryCode,

        /// <summary>
        /// Persisted state changed or a concurrent operation prevented the change.
        /// </summary>
        ConcurrencyConflict,

        /// <summary>
        /// TOTP step is at or below the last accepted step.
        /// </summary>
        TotpStepAlreadyUsed,

        /// <summary>
        /// Challenge expired.
        /// </summary>
        ChallengeExpired,

        /// <summary>
        /// Challenge attempt limit exhausted.
        /// </summary>
        ChallengeAttemptsExhausted

    }

}
