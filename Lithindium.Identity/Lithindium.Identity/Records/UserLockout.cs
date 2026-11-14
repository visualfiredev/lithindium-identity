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
        /// Gets a value indicating whether lockout is enabled.
        /// </summary>
        /// <value><see langword="true"/> if lockout is enabled; otherwise, <see langword="false"/>.</value>
        public required bool Enabled { get; init; }

        /// <summary>
        /// Gets the number of consecutive authentication failures.
        /// </summary>
        public required uint Failures { get; init; }

        /// <summary>
        /// Gets the date and time at which the lockout began, in UTC.
        /// </summary>
        /// <value>The lockout start time, or <see langword="null"/> if not set.</value>
        public required DateTime? LockedAt { get; init; }

        /// <summary>
        /// Gets the scheduled date and time at which the lockout ends, in UTC.
        /// </summary>
        /// <value>The scheduled unlock time, or <see langword="null"/> if not set.</value>
        public required DateTime? UnlockedAt { get; init; }

        /// <summary>
        /// Creates a <see cref="UserLockout"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserLockout(bool enabled, uint failures, DateTime? lockedAt, DateTime? unlockedAt) {
            Enabled = enabled;
            Failures = failures;
            LockedAt = lockedAt;
            UnlockedAt = unlockedAt;
        }

        /// <summary>
        /// Creates a <see cref="UserLockout"/>.
        /// </summary>
        public UserLockout() {
            // ...
        }

    }

}
