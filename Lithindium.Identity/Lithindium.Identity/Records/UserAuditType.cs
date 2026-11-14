// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// An audited entity type.
    /// </summary>
    public enum UserAuditType {

        /// <summary>
        /// User account.
        /// </summary>
        User,

        /// <summary>
        /// User email address.
        /// </summary>
        Email,

        /// <summary>
        /// User phone number.
        /// </summary>
        Phone,

        /// <summary>
        /// TOTP authenticator.
        /// </summary>
        Totp,

        /// <summary>
        /// Authentication session.
        /// </summary>
        Session,

        /// <summary>
        /// User operation challenge.
        /// </summary>
        Challenge,

        /// <summary>
        /// Recovery code.
        /// </summary>
        RecoveryCode,

        /// <summary>
        /// External identity.
        /// </summary>
        ExternalIdentity

    }

}
