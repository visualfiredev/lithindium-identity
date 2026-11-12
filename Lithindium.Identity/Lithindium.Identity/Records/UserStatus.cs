// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the account status of a user within the identity system.
    /// </summary>
    public enum UserStatus {

        /// <summary>
        /// Indicates that the user is permitted to authenticate and access the system.
        /// </summary>
        Active,

        /// <summary>
        /// Indicates that the user is prohibited from authenticating and accessing the system.
        /// </summary>
        Suspended

    }

}
