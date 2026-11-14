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
    /// Represents an immutable snapshot of a pending or verified user phone number.
    /// </summary>
    public readonly record struct UserPhone {

        /// <summary>
        /// Gets the phone number of the user in E.164 format.
        /// </summary>
        public required string Number { get; init; }

        /// <summary>
        /// Gets the extension of the phone number.
        /// </summary>
        /// <value>The extension, or <see langword="null"/> if not set.</value>
        public required string? Extension { get; init; }

        /// <summary>
        /// Gets a value indicating whether the phone number is the primary number.
        /// </summary>
        /// <value><see langword="true"/> if the phone number is primary; otherwise, <see langword="false"/>.</value>
        public required bool Primary { get; init; }

        /// <summary>
        /// Gets the unique identifier of the pending phone verification challenge.
        /// </summary>
        /// <value>The challenge UUID for a pending phone number, or <see langword="null"/> for a verified phone number.</value>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Creates a <see cref="UserPhone"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserPhone(string number, string? extension, bool primary, Guid? challenge) {
            Number = number;
            Extension = extension;
            Primary = primary;
            Challenge = challenge;
        }

        /// <summary>
        /// Creates a <see cref="UserPhone"/>.
        /// </summary>
        public UserPhone() {
            // ...
        }

    }

}
