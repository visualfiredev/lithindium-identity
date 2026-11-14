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
    /// Represents an immutable snapshot of a pending or verified time-based one-time password (TOTP) authenticator.
    /// </summary>
    public readonly record struct UserTotp {

        /// <summary>
        /// Gets the unique identifier of the authenticator.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name of the authenticator.
        /// </summary>
        /// <value>The name, or <see langword="null"/> if not set.</value>
        public required string? Name { get; init; }

        /// <summary>
        /// Gets the cryptographically protected shared secret of the authenticator.
        /// </summary>
        public required ImmutableArray<byte> Secret { get; init; }

        /// <summary>
        /// Gets the hashing algorithm used to generate one-time passwords.
        /// </summary>
        public required UserTotpAlgorithm Algorithm { get; init; }

        /// <summary>
        /// Gets the number of digits in a one-time password.
        /// </summary>
        public required byte Digits { get; init; }

        /// <summary>
        /// Gets the validity period of a one-time password, in seconds.
        /// </summary>
        public required byte Period { get; init; }

        /// <summary>
        /// Gets the unique identifier of the pending enrollment challenge.
        /// </summary>
        /// <value>The challenge UUID for a pending authenticator, or <see langword="null"/> for a verified authenticator.</value>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Gets the last accepted time step.
        /// </summary>
        /// <remarks>
        /// This value prevents replay of accepted authentication codes.
        /// </remarks>
        /// <value>The last accepted time step, or <see langword="null"/> if no time step has been accepted.</value>
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
