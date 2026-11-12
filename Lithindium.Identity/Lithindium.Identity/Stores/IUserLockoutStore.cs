// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Provides persistence operations for user authentication lockout states.
    /// </summary>
    public interface IUserLockoutStore : IUserStore {

        /// <summary>
        /// Retrieves the authentication lockout state of the specified user.
        /// </summary>
        /// <param name="user">The user whose lockout state to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The lockout state, or <see langword="null"/> if no state exists.</returns>
        Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update the authentication lockout state of the specified user.
        /// </summary>
        /// <param name="user">The user whose lockout state to update.</param>
        /// <param name="expected">The expected existing lockout state, or <see langword="null"/> if no state exists.</param>
        /// <param name="lockout">The updated lockout state.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see langword="true"/> if the state was successfully updated;
        /// otherwise, <see langword="false"/> if the existing state did not match
        /// the expected state.
        /// </returns>
        /// <remarks>
        /// The comparison and update must be performed atomically.
        /// </remarks>
        Task<bool> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default);

    }

}
