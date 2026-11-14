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
    /// Represents an immutable snapshot of a pending or verified user email address.
    /// </summary>
    public readonly record struct UserEmail {

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        public required string Address { get; init; }

        /// <summary>
        /// Gets a value indicating whether the email address is the primary address.
        /// </summary>
        /// <value><see langword="true"/> if the email address is primary; otherwise, <see langword="false"/>.</value>
        public required bool Primary { get; init; }

        /// <summary>
        /// Gets the unique identifier of the pending email verification challenge.
        /// </summary>
        /// <value>The challenge UUID for a pending email address, or <see langword="null"/> for a verified email address.</value>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Creates a <see cref="UserEmail"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserEmail(string address, bool primary, Guid? challenge) {
            Address = address;
            Primary = primary;
            Challenge = challenge;
        }

        /// <summary>
        /// Creates a <see cref="UserEmail"/>.
        /// </summary>
        public UserEmail() {
            // ...
        }

    }

}
