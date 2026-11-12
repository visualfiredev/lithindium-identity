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
    /// Represents an immutable snapshot of an external identity associated with a user.
    /// </summary>
    public readonly record struct UserExternalIdentity {

        /// <summary>
        /// Gets the universally unique identifier of the external identity.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name of the external identity, or <see langword="null"/> if no name is configured.
        /// </summary>
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
        /// Gets the email address provided by the issuer,
        /// or <see langword="null"/> if no email address was provided.
        /// </summary>
        /// <remarks>
        /// This email address is informational and does not establish
        /// ownership of a verified email address within the identity system.
        /// </remarks>
        public required string? Email { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserExternalIdentity"/>
        /// with the specified parameters.
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
        /// Constructs a new <see cref="UserExternalIdentity"/>.
        /// </summary>
        public UserExternalIdentity() {
            // ...
        }

    }

}
