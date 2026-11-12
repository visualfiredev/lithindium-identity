// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents an immutable snapshot of a user's authentication lockout state.
    /// </summary>
    public readonly record struct UserLockout {

        /// <summary>
        /// Gets a value indicating whether authentication lockouts are enabled for the user.
        /// </summary>
        public required bool Enabled { get; init; }

        /// <summary>
        /// Gets the number of consecutive failed authentication attempts.
        /// </summary>
        public required uint Failures { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the lockout expires,
        /// or <see langword="null"/> if no lockout expiration is configured.
        /// </summary>
        public required DateTime? ReleaseAt { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserLockout"/>
        /// with the specified parameters.
        /// </summary>
        [SetsRequiredMembers]
        public UserLockout(bool enabled, uint failures, DateTime? releaseAt) {
            Enabled = enabled;
            Failures = failures;
            ReleaseAt = releaseAt;
        }

        /// <summary>
        /// Constructs a new <see cref="UserLockout"/>.
        /// </summary>
        public UserLockout() {
            // ...
        }

    }

}
