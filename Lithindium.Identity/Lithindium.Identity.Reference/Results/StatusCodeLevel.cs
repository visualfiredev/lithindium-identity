// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Status level encoded in the upper two bits.
    /// </summary>
    public enum StatusCodeLevel : byte {

        /// <summary>
        /// Successful completion.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Completion with qualifications.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Recoverable failure.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Unrecoverable failure.
        /// </summary>
        Fatal = 3

    }

}
