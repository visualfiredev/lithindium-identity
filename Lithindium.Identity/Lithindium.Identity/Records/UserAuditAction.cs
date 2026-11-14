// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Specifies the action performed on an audited entity.
    /// </summary>
    public enum UserAuditAction {

        /// <summary>
        /// The entity was created.
        /// </summary>
        Created,

        /// <summary>
        /// The entity was updated.
        /// </summary>
        Updated,

        /// <summary>
        /// The entity was verified.
        /// </summary>
        Verified,

        /// <summary>
        /// The single-use entity was consumed.
        /// </summary>
        Consumed,

        /// <summary>
        /// The entity expired.
        /// </summary>
        Expired,

        /// <summary>
        /// The entity was unlinked from a user.
        /// </summary>
        Removed,

        /// <summary>
        /// The entity was deleted.
        /// </summary>
        Deleted

    }

}
