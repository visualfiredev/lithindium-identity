// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the action performed during an auditable event.
    /// </summary>
    public enum UserAuditAction {

        /// <summary>
        /// Indicates that an entity was created.
        /// </summary>
        Created,

        /// <summary>
        /// Indicates that an existing entity was modified.
        /// </summary>
        Updated,

        /// <summary>
        /// Indicates that an entity was successfully verified.
        /// </summary>
        Verified,

        /// <summary>
        /// Indicates that a single-use entity was successfully consumed.
        /// </summary>
        Consumed,

        /// <summary>
        /// Indicates that an entity reached its expiration.
        /// </summary>
        Expired,

        /// <summary>
        /// Indicates that an entity was removed from its association with a user.
        /// </summary>
        Removed,

        /// <summary>
        /// Indicates that an entity was deleted.
        /// </summary>
        Deleted

    }

}
