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
    /// Immutable user security snapshot.
    /// </summary>
    public readonly record struct UserSecurity {

        /// <summary>
        /// Gets the password hash, or <see langword="null"/> if unset.
        /// </summary>
        public required string? Password { get; init; }

        /// <summary>
        /// Gets the authentication generation.
        /// </summary>
        /// <remarks>
        /// Incrementing the generation invalidates generation-bound credentials.
        /// </remarks>
        public required ushort Rotation { get; init; }

        /// <summary>
        /// Gets the recovery challenge UUID, or <see langword="null"/> if none.
        /// </summary>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Creates a <see cref="UserSecurity"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserSecurity(string? password, ushort rotation, Guid? challenge) {
            Password = password;
            Rotation = rotation;
            Challenge = challenge;
        }

        /// <summary>
        /// Creates a <see cref="UserSecurity"/>.
        /// </summary>
        public UserSecurity() {
            // ...
        }

    }

}
