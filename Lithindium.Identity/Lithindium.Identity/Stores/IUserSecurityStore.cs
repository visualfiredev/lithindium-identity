// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Account security persistence.
    /// </summary>
    /// <remarks>
    /// Security is one-to-one with the user. A recovery challenge does not disable the current password.
    /// </remarks>
    public interface IUserSecurityStore : IUserStore {

        /// <summary>
        /// Gets a user's security state, or returns null.
        /// </summary>
        Task<UserSecurity?> GetSecurityAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally creates or updates security state.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically; a null expectation requires no existing row.
        /// Bind or clear the recovery challenge in the same transaction as its creation or consumption.
        /// Rotation changes must commit with required revocations. Never wrap or decrease rotation.
        /// </remarks>
        Task<bool> TryUpdateSecurityAsync(User user, UserSecurity? expected, UserSecurity security, CancellationToken cancellationToken = default);

    }

}
