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
    /// Represents an immutable snapshot of a user within the identity system.
    /// </summary>
    public readonly record struct User {

        /// <summary>
        /// Gets the universally unique identifier of the user.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the unique username associated with the user.
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// Gets the hashed password of the user, or <see langword="null"/> if no password is configured.
        /// </summary>
        public required string? Password { get; init; }

        /// <summary>
        /// Gets the display name of the user, or <see langword="null"/> if no display name is configured.
        /// </summary>
        public required string? DisplayName { get; init; }

        /// <summary>
        /// Gets the current authentication generation of the user.
        /// </summary>
        /// <remarks>
        /// Incremented to invalidate previously issued generation-bound credentials.
        /// </remarks>
        public required ushort Rotation { get; init; }

        /// <summary>
        /// Gets the current account status of the user.
        /// </summary>
        public required UserStatus Status { get; init; }

        /// <summary>
        /// Constructs a new <see cref="User"/>
        /// with the specified parameters.
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
        /// Constructs a new <see cref="User"/>.
        /// </summary>
        public User() {
            // ...
        }

    }

}
