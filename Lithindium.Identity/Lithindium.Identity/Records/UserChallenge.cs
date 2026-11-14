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
    /// Immutable challenge for a pending user operation.
    /// </summary>
    public readonly record struct UserChallenge {

        /// <summary>
        /// Gets the challenge UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the cryptographic verifier for the challenge proof.
        /// </summary>
        public required string Secret { get; init; }

        /// <summary>
        /// Gets the reserved attempt count.
        /// </summary>
        public required uint Attempts { get; init; }

        /// <summary>
        /// Gets the UTC creation time.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Gets the UTC expiration time.
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Creates a <see cref="UserChallenge"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserChallenge(Guid uuid, string secret, uint attempts, DateTime createdAt, DateTime expiresAt) {
            Uuid = uuid;
            Secret = secret;
            Attempts = attempts;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Creates a <see cref="UserChallenge"/>.
        /// </summary>
        public UserChallenge() {
            // ...
        }

    }

}
