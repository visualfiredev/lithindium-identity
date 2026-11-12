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
    /// Immutable verified user email snapshot.
    /// </summary>
    public readonly record struct UserEmail {

        /// <summary>
        /// Gets the verified email address.
        /// </summary>
        public required string Address { get; init; }

        /// <summary>
        /// Gets whether the email is primary.
        /// </summary>
        public required bool Primary { get; init; }

        /// <summary>
        /// Creates a <see cref="UserEmail"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserEmail(string address, bool primary) {
            Address = address;
            Primary = primary;
        }

        /// <summary>
        /// Creates a <see cref="UserEmail"/>.
        /// </summary>
        public UserEmail() {
            // ...
        }

    }

}
