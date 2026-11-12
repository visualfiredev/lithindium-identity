// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the outcome of a persistence operation within the identity store.
    /// </summary>
    /// <remarks>
    /// Expected persistence conflicts are returned as errors. Invalid arguments,
    /// cancellation, and unexpected infrastructure failures are reported as exceptions.
    /// </remarks>
    public enum UserStoreError {

        /// <summary>
        /// Indicates that the operation completed successfully.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the specified user does not exist.
        /// </summary>
        UserNotFound,

        /// <summary>
        /// Indicates that the specified entity does not exist
        /// or does not belong to the specified user.
        /// </summary>
        EntityNotFound,

        /// <summary>
        /// Indicates that the specified universally unique identifier
        /// is already in use by an entity of the same type.
        /// </summary>
        DuplicateIdentifier,

        /// <summary>
        /// Indicates that the specified username is already assigned to another user.
        /// </summary>
        DuplicateUsername,

        /// <summary>
        /// Indicates that the specified email address is already associated with a user.
        /// </summary>
        DuplicateEmail,

        /// <summary>
        /// Indicates that the specified combination of phone number and extension
        /// is already associated with a user.
        /// </summary>
        DuplicatePhone,

        /// <summary>
        /// Indicates that the specified combination of issuer and subject
        /// is already associated with a user.
        /// </summary>
        DuplicateExternalIdentity,

        /// <summary>
        /// Indicates that a recovery code hash duplicates an existing code outside
        /// the replaced set or another code within the replacement set.
        /// </summary>
        DuplicateRecoveryCode,

        /// <summary>
        /// Indicates that persisted state no longer matches the expected state
        /// or that a concurrent operation prevented the requested change.
        /// </summary>
        ConcurrencyConflict,

        /// <summary>
        /// Indicates that the specified TOTP time step is less than or equal
        /// to the most recently accepted time step.
        /// </summary>
        TotpStepAlreadyUsed,

        /// <summary>
        /// Indicates that the specified authentication or verification challenge has expired.
        /// </summary>
        ChallengeExpired,

        /// <summary>
        /// Indicates that the specified challenge has exhausted its permitted verification attempts.
        /// </summary>
        ChallengeAttemptsExhausted

    }

}
