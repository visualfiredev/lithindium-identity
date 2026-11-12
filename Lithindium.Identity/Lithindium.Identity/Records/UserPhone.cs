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
    /// Represents an immutable snapshot of a verified phone number associated with a user.
    /// </summary>
    public readonly record struct UserPhone {

        /// <summary>
        /// Gets the verified phone number associated with the user in E.164 format.
        /// </summary>
        public required string Number { get; init; }

        /// <summary>
        /// Gets the phone number extension, or <see langword="null"/> if no extension is configured.
        /// </summary>
        public required string? Extension { get; init; }

        /// <summary>
        /// Gets a value indicating whether this is the user's primary phone number.
        /// </summary>
        public required bool Primary { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserPhone"/>
        /// with the specified parameters.
        /// </summary>
        [SetsRequiredMembers]
        public UserPhone(string number, string? extension, bool primary) {
            Number = number;
            Extension = extension;
            Primary = primary;
        }

        /// <summary>
        /// Constructs a new <see cref="UserPhone"/>.
        /// </summary>
        public UserPhone() {
            // ...
        }

    }

}
