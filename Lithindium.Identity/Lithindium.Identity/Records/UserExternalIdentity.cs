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
    /// Immutable external identity snapshot.
    /// </summary>
    public readonly record struct UserExternalIdentity {

        /// <summary>
        /// Gets the identity UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the identity name, or <see langword="null"/> if unset.
        /// </summary>
        public required string? Name { get; init; }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        public required string Issuer { get; init; }

        /// <summary>
        /// Gets the issuer-assigned unique subject.
        /// </summary>
        public required string Subject { get; init; }

        /// <summary>
        /// Gets the issuer-provided email, or <see langword="null"/> if absent.
        /// </summary>
        /// <remarks>
        /// Informational only; does not establish verified email ownership.
        /// </remarks>
        public required string? Email { get; init; }

        /// <summary>
        /// Creates a <see cref="UserExternalIdentity"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserExternalIdentity(Guid uuid, string? name, string issuer, string subject, string? email) {
            Uuid = uuid;
            Name = name;
            Issuer = issuer;
            Subject = subject;
            Email = email;
        }

        /// <summary>
        /// Creates a <see cref="UserExternalIdentity"/>.
        /// </summary>
        public UserExternalIdentity() {
            // ...
        }

        /// <summary>
        /// Returns a redacted diagnostic representation.
        /// </summary>
        public override string ToString() => $"UserExternalIdentity {{ Uuid = {Uuid} }}";

    }

}
