// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// TOTP hashing algorithm.
    /// </summary>
    public enum UserTotpAlgorithm {

        /// <summary>
        /// HMAC-SHA1.
        /// </summary>
        Sha1,

        /// <summary>
        /// HMAC-SHA256.
        /// </summary>
        Sha256,

        /// <summary>
        /// HMAC-SHA512.
        /// </summary>
        Sha512

    }

}
