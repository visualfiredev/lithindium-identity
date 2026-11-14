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
    /// Represents an immutable snapshot of a user account.
    /// </summary>
    public readonly record struct User {

        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the unique username of the user.
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        /// <value>The display name, or <see langword="null"/> if not set.</value>
        public required string? DisplayName { get; init; }

        /// <summary>
        /// Gets the status of the user account.
        /// </summary>
        public required UserStatus Status { get; init; }

        /// <summary>
        /// Creates a <see cref="User"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public User(Guid uuid, string username, string? displayName, UserStatus status) {
            Uuid = uuid;
            Username = username;
            DisplayName = displayName;
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
