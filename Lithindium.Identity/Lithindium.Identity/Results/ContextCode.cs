// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Lithindium result context codes.
    /// </summary>
    /// <remarks>
    /// Success-level results require zero context. Other meanings are operation-defined.
    /// </remarks>
    public enum ContextCode : byte {

        /// <summary>
        /// No additional context.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// User account.
        /// </summary>
        User = 0x01,

        /// <summary>
        /// Associated entity.
        /// </summary>
        Entity = 0x02,

        /// <summary>
        /// Entity UUID.
        /// </summary>
        Identifier = 0x03,

        /// <summary>
        /// Username.
        /// </summary>
        Username = 0x04,

        /// <summary>
        /// Email address.
        /// </summary>
        Email = 0x05,

        /// <summary>
        /// Phone number and extension.
        /// </summary>
        Phone = 0x06,

        /// <summary>
        /// External issuer and subject.
        /// </summary>
        ExternalIdentity = 0x07,

        /// <summary>
        /// Recovery code.
        /// </summary>
        RecoveryCode = 0x08,

        /// <summary>
        /// TOTP authenticator.
        /// </summary>
        Totp = 0x0A,

        /// <summary>
        /// Authentication or verification challenge.
        /// </summary>
        Challenge = 0x0B,

        /// <summary>
        /// Password credential.
        /// </summary>
        Password = 0x0C,

        /// <summary>
        /// Authentication session.
        /// </summary>
        Session = 0x0D,

        /// <summary>
        /// Authentication lockout.
        /// </summary>
        Lockout = 0x0E,

        /// <summary>
        /// Audit record.
        /// </summary>
        Audit = 0x0F,

        /// <summary>
        /// User display name.
        /// </summary>
        DisplayName = 0x10

    }

}
