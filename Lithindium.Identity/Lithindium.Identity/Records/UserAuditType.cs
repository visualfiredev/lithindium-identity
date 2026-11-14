// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Specifies the type of entity affected by an audited action.
    /// </summary>
    public enum UserAuditType {

        /// <summary>
        /// The affected entity is a user account.
        /// </summary>
        User,

        /// <summary>
        /// The affected entity is a user email address.
        /// </summary>
        Email,

        /// <summary>
        /// The affected entity is a user phone number.
        /// </summary>
        Phone,

        /// <summary>
        /// The affected entity is a TOTP authenticator.
        /// </summary>
        Totp,

        /// <summary>
        /// The affected entity is an authentication session.
        /// </summary>
        Session,

        /// <summary>
        /// The affected entity is a user operation challenge.
        /// </summary>
        Challenge,

        /// <summary>
        /// The affected entity is a recovery code.
        /// </summary>
        RecoveryCode,

        /// <summary>
        /// The affected entity is an external identity.
        /// </summary>
        ExternalIdentity

    }

}
