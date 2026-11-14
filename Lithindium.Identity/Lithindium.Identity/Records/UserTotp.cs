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
    /// Immutable user TOTP authenticator snapshot.
    /// </summary>
    public readonly record struct UserTotp {

        /// <summary>
        /// Gets the authenticator UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name, or <see langword="null"/> if unset.
        /// </summary>
        public required string? Name { get; init; }

        /// <summary>
        /// Gets the cryptographically protected shared secret.
        /// </summary>
        public required ImmutableArray<byte> Secret { get; init; }

        /// <summary>
        /// Gets the hashing algorithm.
        /// </summary>
        public required UserTotpAlgorithm Algorithm { get; init; }

        /// <summary>
        /// Gets the code digit count.
        /// </summary>
        public required byte Digits { get; init; }

        /// <summary>
        /// Gets the code validity period in seconds.
        /// </summary>
        public required byte Period { get; init; }

        /// <summary>
        /// Gets the TOTP verification challenge UUID, or <see langword="null"/> if none.
        /// </summary>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Gets the last accepted time step, or <see langword="null"/> if none.
        /// </summary>
        /// <remarks>
        /// Prevents replay of accepted authentication codes.
        /// </remarks>
        public required ulong? LastStep { get; init; }

        /// <summary>
        /// Creates a <see cref="UserTotp"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserTotp(Guid uuid, string? name, ImmutableArray<byte> secret, UserTotpAlgorithm algorithm, byte digits, byte period, Guid? challenge, ulong? lastStep) {
            Uuid = uuid;
            Name = name;
            Secret = secret;
            Algorithm = algorithm;
            Digits = digits;
            Period = period;
            Challenge = challenge;
            LastStep = lastStep;
        }

        /// <summary>
        /// Creates a <see cref="UserTotp"/>.
        /// </summary>
        public UserTotp() {
            // ...
        }

    }

}
