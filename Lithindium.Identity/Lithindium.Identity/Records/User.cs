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
    /// Immutable user snapshot.
    /// </summary>
    public readonly record struct User {

        /// <summary>
        /// Gets the user UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the unique username.
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// Gets the password hash, or <see langword="null"/> if unset.
        /// </summary>
        public required string? Password { get; init; }

        /// <summary>
        /// Gets the display name, or <see langword="null"/> if unset.
        /// </summary>
        public required string? DisplayName { get; init; }

        /// <summary>
        /// Gets the authentication generation.
        /// </summary>
        /// <remarks>
        /// Incremented to invalidate generation-bound credentials.
        /// </remarks>
        public required ushort Rotation { get; init; }

        /// <summary>
        /// Gets the account status.
        /// </summary>
        public required UserStatus Status { get; init; }

        /// <summary>
        /// Creates a <see cref="User"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public User(Guid uuid, string username, string? password, string? displayName, ushort rotation, UserStatus status) {
            Uuid = uuid;
            Username = username;
            Password = password;
            DisplayName = displayName;
            Rotation = rotation;
            Status = status;
        }

        /// <summary>
        /// Creates a <see cref="User"/>.
        /// </summary>
        public User() {
            // ...
        }

    }

}
