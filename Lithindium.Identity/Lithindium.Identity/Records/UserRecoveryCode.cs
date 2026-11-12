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
    /// Represents an immutable snapshot of a recovery code associated with a user.
    /// </summary>
    public readonly record struct UserRecoveryCode {

        /// <summary>
        /// Gets the universally unique identifier of the recovery code.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the cryptographic hash of the recovery code.
        /// </summary>
        public required ImmutableArray<byte> CodeHash { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserRecoveryCode"/>
        /// with the specified parameters.
        /// </summary>
        [SetsRequiredMembers]
        public UserRecoveryCode(Guid uuid, ImmutableArray<byte> codeHash) {
            Uuid = uuid;
            CodeHash = codeHash;
        }

        /// <summary>
        /// Constructs a new <see cref="UserRecoveryCode"/>.
        /// </summary>
        public UserRecoveryCode() {
            // ...
        }

    }

}
