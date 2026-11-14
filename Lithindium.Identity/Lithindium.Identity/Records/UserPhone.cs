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
    /// Immutable verified user phone snapshot.
    /// </summary>
    public readonly record struct UserPhone {

        /// <summary>
        /// Gets the verified E.164 phone number.
        /// </summary>
        public required string Number { get; init; }

        /// <summary>
        /// Gets the extension, or <see langword="null"/> if unset.
        /// </summary>
        public required string? Extension { get; init; }

        /// <summary>
        /// Gets whether the phone is primary.
        /// </summary>
        public required bool Primary { get; init; }

        /// <summary>
        /// Gets the phone verification challenge UUID, or <see langword="null"/> if none.
        /// </summary>
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
