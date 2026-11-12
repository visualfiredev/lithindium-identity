// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Immutable user authentication session snapshot.
    /// </summary>
    public readonly record struct UserSession {

        /// <summary>
        /// Gets the session UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the device name, or <see langword="null"/> if unset.
        /// </summary>
        public required string? DeviceName { get; init; }

        /// <summary>
        /// Gets the IP address, or <see langword="null"/> if unavailable.
        /// </summary>
        public required string? IpAddress { get; init; }

        /// <summary>
        /// Gets the user agent, or <see langword="null"/> if unavailable.
        /// </summary>
        public required string? UserAgent { get; init; }

        /// <summary>
        /// Gets the authentication methods used.
        /// </summary>
        public required ImmutableArray<string> Amr { get; init; }

        /// <summary>
        /// Gets the authentication context class reference, or <see langword="null"/> if unspecified.
        /// </summary>
        public required string? Acr { get; init; }

        /// <summary>
        /// Gets the UTC authentication time.
        /// </summary>
        public required DateTime AuthenticatedAt { get; init; }

        /// <summary>
        /// Gets the UTC expiration time.
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Creates a <see cref="UserSession"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserSession(Guid uuid, string? deviceName, string? ipAddress, string? userAgent, ImmutableArray<string> amr, string? acr, DateTime authenticatedAt, DateTime expiresAt) {
            Uuid = uuid;
            DeviceName = deviceName;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Amr = amr;
            Acr = acr;
            AuthenticatedAt = authenticatedAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Creates a <see cref="UserSession"/>.
        /// </summary>
        public UserSession() {
            // ...
        }

        /// <summary>
        /// Returns a redacted diagnostic representation.
        /// </summary>
        public override string ToString() => $"UserSession {{ Uuid = {Uuid} }}";

    }

}
