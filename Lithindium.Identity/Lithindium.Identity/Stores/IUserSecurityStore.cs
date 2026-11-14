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
        /// Gets a user's security state.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The security state, or <see langword="null"/> if not found.</returns>
        Task<UserSecurity?> GetSecurityAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally creates or updates security state.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected security state, or <see langword="null"/> if absent.</param>
        /// <param name="security">Updated security state.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares the entire expected state atomically; a <see langword="null"/> expectation requires no existing row.
        /// Binds or clears the recovery challenge in the same transaction as its creation or consumption.
        /// Rotation changes are committed with required revocations. Rotation never wraps or decreases.
        /// </remarks>
        /// <returns><see langword="true"/> if the security state was saved, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateSecurityAsync(User user, UserSecurity? expected, UserSecurity security, CancellationToken cancellationToken = default);

    }

}
