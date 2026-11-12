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
    /// Authentication lockout persistence.
    /// </summary>
    public interface IUserLockoutStore : IUserStore {

        /// <summary>
        /// Gets a user's lockout state.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Lockout state, or <see langword="null"/> if absent.</returns>
        Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a conditional lockout update.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="expected">Expected lockout, or <see langword="null"/> if absent.</param>
        /// <param name="lockout">Updated lockout state.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>.
        /// </returns>
        /// <remarks>
        /// Atomically compare and update; a <see langword="null"/> expectation requires no existing state.
        /// </remarks>
        Task<UserStoreError> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default);

    }

}
