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
    /// Represents an immutable snapshot of an identity associated with an external issuer.
    /// </summary>
    public readonly record struct UserExternalIdentity {

        /// <summary>
        /// Gets the unique identifier of the external identity.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name associated with the external identity.
        /// </summary>
        /// <value>The name, or <see langword="null"/> if not set.</value>
        public required string? Name { get; init; }

        /// <summary>
        /// Gets the issuer of the external identity.
        /// </summary>
        public required string Issuer { get; init; }

        /// <summary>
        /// Gets the unique subject identifier assigned by the issuer.
        /// </summary>
        public required string Subject { get; init; }

        /// <summary>
        /// Gets the email address provided by the issuer.
        /// </summary>
        /// <remarks>
        /// This address is informational and does not establish verified email ownership.
        /// </remarks>
        /// <value>The email address, or <see langword="null"/> if not provided.</value>
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

    }

}
