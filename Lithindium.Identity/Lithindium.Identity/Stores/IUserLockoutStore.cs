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
        /// Gets a user's lockout state, or returns null.
        /// </summary>
        Task<UserLockout?> GetLockoutAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally creates or updates lockout state.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically; a null expectation requires no existing row.
        /// </remarks>
        Task<bool> TryUpdateLockoutAsync(User user, UserLockout? expected, UserLockout lockout, CancellationToken cancellationToken = default);

    }

}
