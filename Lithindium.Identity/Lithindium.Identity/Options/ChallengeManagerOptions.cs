// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Challenge lifetimes and attempt limits.
    /// </summary>
    public sealed class ChallengeManagerOptions {

        /// <summary>
        /// Contact verification lifetime.
        /// </summary>
        public TimeSpan ContactLifetime { get; init; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Password reset lifetime.
        /// </summary>
        public TimeSpan PasswordResetLifetime { get; init; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// TOTP enrollment lifetime.
        /// </summary>
        public TimeSpan TotpEnrollmentLifetime { get; init; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Pending sign-in lifetime.
        /// </summary>
        public TimeSpan MultiFactorLifetime { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Maximum attempts per challenge.
        /// </summary>
        public uint MaxAttempts { get; init; } = 5;

    }

}
