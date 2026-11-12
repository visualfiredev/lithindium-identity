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
    /// Represents an immutable snapshot of an authentication session associated with a user.
    /// </summary>
    public readonly record struct UserSession {

        /// <summary>
        /// Gets the universally unique identifier of the session.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name of the device associated with the session,
        /// or <see langword="null"/> if no device name is configured.
        /// </summary>
        public required string? DeviceName { get; init; }

        /// <summary>
        /// Gets the IP address associated with the session,
        /// or <see langword="null"/> if no IP address is available.
        /// </summary>
        public required string? IpAddress { get; init; }

        /// <summary>
        /// Gets the user agent associated with the session,
        /// or <see langword="null"/> if no user agent is available.
        /// </summary>
        public required string? UserAgent { get; init; }

        /// <summary>
        /// Gets the authentication methods used to establish the session.
        /// </summary>
        public required ImmutableArray<string> Amr { get; init; }

        /// <summary>
        /// Gets the authentication context class reference associated with the session,
        /// or <see langword="null"/> if no context class is specified.
        /// </summary>
        public required string? Acr { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the user authenticated.
        /// </summary>
        public required DateTime AuthenticatedAt { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the session expires.
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserSession"/>
        /// with the specified parameters.
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
        /// Constructs a new <see cref="UserSession"/>.
        /// </summary>
        public UserSession() {
            // ...
        }

    }

}
