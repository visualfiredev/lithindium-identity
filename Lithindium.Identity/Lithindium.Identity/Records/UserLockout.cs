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
    /// Immutable user lockout snapshot.
    /// </summary>
    public readonly record struct UserLockout {

        /// <summary>
        /// Gets whether lockout is enabled.
        /// </summary>
        public required bool Enabled { get; init; }

        /// <summary>
        /// Gets the consecutive authentication failure count.
        /// </summary>
        public required uint Failures { get; init; }

        /// <summary>
        /// Gets the UTC lockout start time, or <see langword="null"/> if unset.
        /// </summary>
        public required DateTime? LockedAt { get; init; }

        /// <summary>
        /// Gets the scheduled UTC unlock time, or <see langword="null"/> if unset.
        /// </summary>
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
