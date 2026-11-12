// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents an immutable snapshot of a verified email address associated with a user.
    /// </summary>
    public readonly record struct UserEmail {

        /// <summary>
        /// Gets the verified email address associated with the user.
        /// </summary>
        public required string Address { get; init; }

        /// <summary>
        /// Gets a value indicating whether this is the user's primary email address.
        /// </summary>
        public required bool Primary { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserEmail"/>
        /// with the specified parameters.
        /// </summary>
        [SetsRequiredMembers]
        public UserEmail(string address, bool primary) {
            Address = address;
            Primary = primary;
        }

        /// <summary>
        /// Constructs a new <see cref="UserEmail"/>.
        /// </summary>
        public UserEmail() {
            // ...
        }

    }

}
