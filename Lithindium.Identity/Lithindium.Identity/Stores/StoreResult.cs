// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// The outcome of a store mutation.
    /// </summary>
    public enum StoreResult {

        /// <summary>
        /// Mutation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The user or entity was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// A uniqueness constraint was violated.
        /// </summary>
        AlreadyExists,

        /// <summary>
        /// The expected state changed or a persistence conflict occurred.
        /// </summary>
        Conflict

    }

}
