// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Application-provided session metadata.
    /// </summary>
    public sealed class SessionMetadata {

        /// <summary>
        /// Device name.
        /// </summary>
        public string? DeviceName { get; init; }

        /// <summary>
        /// Client IP address.
        /// </summary>
        public string? IpAddress { get; init; }

        /// <summary>
        /// Client user agent.
        /// </summary>
        public string? UserAgent { get; init; }

    }

}
