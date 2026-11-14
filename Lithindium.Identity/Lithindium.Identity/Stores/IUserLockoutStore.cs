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
    /// Defines the persistence contract for user authentication lockout state.
    /// </summary>
    public interface IUserLockoutStore : IUserStore {

        /// <summary>
        /// Asynchronously gets a user's lockout state.
        /// </summary>
        /// <param name="user">The user to whom the lockout state belongs.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the lockout state, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to create or conditionally update a user's lockout state asynchronously.
        /// </summary>
        /// <remarks>
        /// Compares the entire persisted lockout state against <paramref name="expected"/> atomically. A <see langword="null"/> expectation requires that no lockout record exists.
        /// </remarks>
        /// <param name="user">The user to whom the lockout state belongs.</param>
        /// <param name="expected">The expected persisted lockout state, or <see langword="null"/> if no record is expected to exist.</param>
        /// <param name="lockout">The replacement lockout state.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the lockout state was updated within the transaction; otherwise, <see langword="false"/> (for example, if the expected state does not match).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default);

    }

}
