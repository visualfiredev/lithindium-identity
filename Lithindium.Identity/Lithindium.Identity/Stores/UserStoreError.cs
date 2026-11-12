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
        /// Indicates that the specified associated entity does not exist or does not belong to the specified user.
        /// </summary>
        EntityNotFound,

        /// <summary>
        /// Indicates that the requested username is already assigned to another user.
        /// </summary>
        DuplicateUsername,

        /// <summary>
        /// Indicates that the email address is already associated with a user.
        /// </summary>
        DuplicateEmail,

        /// <summary>
        /// Indicates that the combination of phone number and extension is already associated with a user.
        /// </summary>
        DuplicatePhone,

        /// <summary>
        /// Indicates that the combination of issuer and subject is already associated with a user.
        /// </summary>
        DuplicateExternalIdentity,

        /// <summary>
        /// Indicates that the supplied universally unique identifier is already in use for an entity of the same type.
        /// </summary>
        DuplicateIdentifier,

        /// <summary>
        /// Indicates that a recovery code hash duplicates an existing code outside the replaced set or another code in the replacement set.
        /// </summary>
        DuplicateRecoveryCode,

        /// <summary>
        /// Indicates that persisted state no longer matches the expected state or that a concurrent operation prevented the requested change.
        /// </summary>
        ConcurrencyConflict,

        /// <summary>
        /// Indicates that the supplied TOTP time step is less than or equal to the most recently accepted time step.
        /// </summary>
        TotpStepAlreadyUsed

    }

}
