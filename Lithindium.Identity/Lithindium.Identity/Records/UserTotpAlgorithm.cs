// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the hashing algorithm used by a TOTP authenticator.
    /// </summary>
    public enum UserTotpAlgorithm {

        /// <summary>
        /// Indicates that the authenticator uses HMAC-SHA1.
        /// </summary>
        Sha1,

        /// <summary>
        /// Indicates that the authenticator uses HMAC-SHA256.
        /// </summary>
        Sha256,

        /// <summary>
        /// Indicates that the authenticator uses HMAC-SHA512.
        /// </summary>
        Sha512

    }

}
