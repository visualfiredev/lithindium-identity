// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Additional operation details.
    /// </summary>
    /// <remarks>
    /// Status-defined detail takes precedence, followed by context and operation.
    /// Named details apply only where documented; other byte values may be used.
    /// </remarks>
    public enum DetailCode : byte {

        /// <summary>
        /// No additional detail.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Additional authentication factor required.
        /// </summary>
        MultiFactorRequired = 0x10,

        /// <summary>
        /// Value below the minimum length.
        /// </summary>
        TooShort = 0x11,

        /// <summary>
        /// Account suspended.
        /// </summary>
        Suspended = 0x12,

        /// <summary>
        /// Account locked out.
        /// </summary>
        LockedOut = 0x13

    }

}
