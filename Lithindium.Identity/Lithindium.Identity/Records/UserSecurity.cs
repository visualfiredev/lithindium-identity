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
    /// Represents an immutable snapshot of a user's account security state.
    /// </summary>
    public readonly record struct UserSecurity {

        /// <summary>
        /// Gets the password hash of the user.
        /// </summary>
        /// <value>The password hash, or <see langword="null"/> if no password is set.</value>
        public required string? Password { get; init; }

        /// <summary>
        /// Gets the authentication generation of the user.
        /// </summary>
        /// <remarks>
        /// Incrementing this value invalidates generation-bound credentials.
        /// </remarks>
        public required ushort Rotation { get; init; }

        /// <summary>
        /// Gets the unique identifier of the pending account recovery challenge.
        /// </summary>
        /// <value>The recovery challenge UUID, or <see langword="null"/> if no recovery is pending.</value>
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
