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
        /// Creates a <see cref="UserPhone"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserPhone(string number, string? extension, bool primary) {
            Number = number;
            Extension = extension;
            Primary = primary;
        }

        /// <summary>
        /// Creates a <see cref="UserPhone"/>.
        /// </summary>
        public UserPhone() {
            // ...
        }

        /// <summary>
        /// Returns a redacted diagnostic representation.
        /// </summary>
        public override string ToString() => nameof(UserPhone);

    }

}
