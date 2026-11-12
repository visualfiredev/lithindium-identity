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
    /// Immutable pending authentication or verification challenge.
    /// </summary>
    public readonly record struct UserChallenge {

        /// <summary>
        /// Gets the challenge UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the challenge purpose.
        /// </summary>
        public required UserChallengeType Type { get; init; }

        /// <summary>
        /// Gets the cryptographic secret verifier, or <see langword="null"/> if not required.
        /// </summary>
        public required ImmutableArray<byte>? SecretHash { get; init; }

        /// <summary>
        /// Gets the cryptographically protected payload, or <see langword="null"/> if none.
        /// </summary>
        public required ImmutableArray<byte>? Payload { get; init; }

        /// <summary>
        /// Gets the reserved attempt count.
        /// </summary>
        public required uint Attempts { get; init; }

        /// <summary>
        /// Gets the attempt limit.
        /// </summary>
        public required uint MaxAttempts { get; init; }

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
        /// Creates a <see cref="UserChallenge"/>.
        /// </summary>
        public UserChallenge() {
            // ...
        }

        /// <summary>
        /// Returns a redacted diagnostic representation.
        /// </summary>
        public override string ToString() => $"UserChallenge {{ Uuid = {Uuid} }}";

    }

}
