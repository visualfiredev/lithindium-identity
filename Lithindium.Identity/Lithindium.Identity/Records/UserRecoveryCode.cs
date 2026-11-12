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
    /// Immutable user recovery code snapshot.
    /// </summary>
    public readonly record struct UserRecoveryCode {

        /// <summary>
        /// Gets the recovery code UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the cryptographic code hash.
        /// </summary>
        public required ImmutableArray<byte> CodeHash { get; init; }

        /// <summary>
        /// Creates a <see cref="UserRecoveryCode"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserRecoveryCode(Guid uuid, ImmutableArray<byte> codeHash) {
            Uuid = uuid;
            CodeHash = codeHash;
        }

        /// <summary>
        /// Creates a <see cref="UserRecoveryCode"/>.
        /// </summary>
        public UserRecoveryCode() {
            // ...
        }

        /// <summary>
        /// Returns a redacted diagnostic representation.
        /// </summary>
        public override string ToString() => $"UserRecoveryCode {{ Uuid = {Uuid} }}";

    }

}
