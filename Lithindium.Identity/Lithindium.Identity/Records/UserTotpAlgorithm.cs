// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Specifies the hashing algorithm used by a TOTP authenticator.
    /// </summary>
    public enum UserTotpAlgorithm {

        /// <summary>
        /// Uses HMAC-SHA1.
        /// </summary>
        Sha1,

        /// <summary>
        /// Uses HMAC-SHA256.
        /// </summary>
        Sha256,

        /// <summary>
        /// Uses HMAC-SHA512.
        /// </summary>
        Sha512

    }

}
