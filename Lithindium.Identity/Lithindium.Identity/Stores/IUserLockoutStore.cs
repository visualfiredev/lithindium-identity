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
        /// <param name="user">Owning user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The lockout state, or <see langword="null"/> if not found.</returns>
        Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally creates or updates lockout state.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected lockout state, or <see langword="null"/> if absent.</param>
        /// <param name="lockout">Updated lockout state.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares the entire expected state atomically; a <see langword="null"/> expectation requires no existing row.
        /// </remarks>
        /// <returns><see langword="true"/> if the lockout state was saved, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default);

    }

}
