// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents an immutable snapshot of a pending authentication or verification challenge.
    /// </summary>
    public readonly record struct UserChallenge {

        /// <summary>
        /// Gets the universally unique identifier of the challenge.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the purpose of the challenge.
        /// </summary>
        public required UserChallengeType Type { get; init; }

        /// <summary>
        /// Gets the cryptographic verifier of the challenge secret,
        /// or <see langword="null"/> if no separate secret is required.
        /// </summary>
        public required ImmutableArray<byte>? SecretHash { get; init; }

        /// <summary>
        /// Gets the cryptographically protected data associated with the challenge,
        /// or <see langword="null"/> if no additional data is required.
        /// </summary>
        public required ImmutableArray<byte>? Payload { get; init; }

        /// <summary>
        /// Gets the number of verification attempts reserved for the challenge.
        /// </summary>
        public required uint Attempts { get; init; }

        /// <summary>
        /// Gets the maximum number of verification attempts permitted for the challenge.
        /// </summary>
        public required uint MaxAttempts { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the challenge was created.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the challenge expires.
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserChallenge"/>
        /// with the specified parameters.
        /// </summary>
        [SetsRequiredMembers]
        public UserChallenge(Guid uuid, UserChallengeType type, ImmutableArray<byte>? secretHash, ImmutableArray<byte>? payload, uint attempts, uint maxAttempts, DateTime createdAt, DateTime expiresAt) {
            Uuid = uuid;
            Type = type;
            SecretHash = secretHash;
            Payload = payload;
            Attempts = attempts;
            MaxAttempts = maxAttempts;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Constructs a new <see cref="UserChallenge"/>.
        /// </summary>
        public UserChallenge() {
            // ...
        }

    }

}
