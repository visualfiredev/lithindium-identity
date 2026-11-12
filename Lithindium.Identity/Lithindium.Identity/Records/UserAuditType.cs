// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the type of entity associated with an auditable event.
    /// </summary>
    public enum UserAuditType {

        /// <summary>
        /// Indicates an event associated with a user account.
        /// </summary>
        User,

        /// <summary>
        /// Indicates an event associated with a user email address.
        /// </summary>
        Email,

        /// <summary>
        /// Indicates an event associated with a user phone number.
        /// </summary>
        Phone,

        /// <summary>
        /// Indicates an event associated with a user TOTP authenticator.
        /// </summary>
        Totp,

        /// <summary>
        /// Indicates an event associated with a user authentication session.
        /// </summary>
        Session,

        /// <summary>
        /// Indicates an event associated with a user recovery code.
        /// </summary>
        RecoveryCode,

        /// <summary>
        /// Indicates an event associated with a user external identity.
        /// </summary>
        ExternalIdentity,

        /// <summary>
        /// Indicates an event associated with a user authentication or verification challenge.
        /// </summary>
        Challenge

    }

}
