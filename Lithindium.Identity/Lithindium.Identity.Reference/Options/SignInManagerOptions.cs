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
    /// Sign-in and session policy.
    /// </summary>
    public sealed class SignInManagerOptions {

        /// <summary>
        /// Fixed session lifetime.
        /// </summary>
        public TimeSpan SessionLifetime { get; init; } = TimeSpan.FromDays(14);

        /// <summary>
        /// Consecutive failures before lockout.
        /// </summary>
        public uint MaxFailures { get; init; } = 5;

        /// <summary>
        /// Account lockout duration.
        /// </summary>
        public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Requires lockout persistence.
        /// </summary>
        public bool RequireLockoutStore { get; init; } = true;

        /// <summary>
        /// Requires enrolled MFA for all sign-ins; unenrolled accounts are denied.
        /// </summary>
        public bool RequireMultiFactor { get; init; }

    }

}
