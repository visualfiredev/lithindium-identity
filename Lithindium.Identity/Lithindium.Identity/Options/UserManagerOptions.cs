// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// User management policy.
    /// </summary>
    public sealed class UserManagerOptions {

        /// <summary>
        /// Gets the minimum password length in UTF-16 code units.
        /// </summary>
        public int MinimumPasswordLength { get; init; } = 15;

        /// <summary>
        /// Gets the maximum password length in UTF-16 code units.
        /// </summary>
        public int MaximumPasswordLength { get; init; } = 1024;

        /// <summary>
        /// Gets the default number of recovery codes.
        /// </summary>
        public int RecoveryCodeCount { get; init; } = 10;

    }

}
