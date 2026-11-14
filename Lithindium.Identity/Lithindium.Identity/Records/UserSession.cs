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
    /// Represents an immutable snapshot of a pending or authenticated user session.
    /// </summary>
    public readonly record struct UserSession {

        /// <summary>
        /// Gets the unique identifier of the session.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the name of the device associated with the session.
        /// </summary>
        /// <value>The device name, or <see langword="null"/> if not set.</value>
        public required string? DeviceName { get; init; }

        /// <summary>
        /// Gets the IP address associated with the session.
        /// </summary>
        /// <value>The IP address, or <see langword="null"/> if unavailable.</value>
        public required string? IpAddress { get; init; }

        /// <summary>
        /// Gets the user agent associated with the session.
        /// </summary>
        /// <value>The user agent, or <see langword="null"/> if unavailable.</value>
        public required string? UserAgent { get; init; }

        /// <summary>
        /// Gets the authentication methods used for the session.
        /// </summary>
        public required ImmutableArray<string> Amr { get; init; }

        /// <summary>
        /// Gets the authentication context class reference for the session.
        /// </summary>
        /// <value>The authentication context class reference, or <see langword="null"/> if unspecified.</value>
        public required string? Acr { get; init; }

        /// <summary>
        /// Gets the unique identifier of the pending authentication challenge.
        /// </summary>
        /// <value>The challenge UUID for a pending session, or <see langword="null"/> for an authenticated session.</value>
        public required Guid? Challenge { get; init; }

        /// <summary>
        /// Gets the date and time at which the session was created, in UTC.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Gets the date and time at which the session expires, in UTC.
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Creates a <see cref="UserSession"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserSession(Guid uuid, string? deviceName, string? ipAddress, string? userAgent, ImmutableArray<string> amr, string? acr, Guid? challenge, DateTime createdAt, DateTime expiresAt) {
            Uuid = uuid;
            DeviceName = deviceName;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Amr = amr;
            Acr = acr;
            Challenge = challenge;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Creates a <see cref="UserSession"/>.
        /// </summary>
        public UserSession() {
            // ...
        }

    }

}
