// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Specifies the status of a user account.
    /// </summary>
    public enum UserStatus {

        /// <summary>
        /// The account permits authentication and access, subject to other authentication requirements.
        /// </summary>
        Active,

        /// <summary>
        /// The account prohibits authentication and access.
        /// </summary>
        Suspended

    }

}
