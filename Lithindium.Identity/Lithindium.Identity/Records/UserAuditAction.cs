// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// An audited action.
    /// </summary>
    public enum UserAuditAction {

        /// <summary>
        /// Entity created.
        /// </summary>
        Created,

        /// <summary>
        /// Entity updated.
        /// </summary>
        Updated,

        /// <summary>
        /// Entity verified.
        /// </summary>
        Verified,

        /// <summary>
        /// Single-use entity consumed.
        /// </summary>
        Consumed,

        /// <summary>
        /// Entity expired.
        /// </summary>
        Expired,

        /// <summary>
        /// Entity unlinked from a user.
        /// </summary>
        Removed,

        /// <summary>
        /// Entity deleted.
        /// </summary>
        Deleted

    }

}
