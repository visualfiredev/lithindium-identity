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
    /// Defines the persistence contract for user account security state.
    /// </summary>
    /// <remarks>
    /// Each user has at most one security record. A pending recovery challenge does not disable the current password.
    /// </remarks>
    public interface IUserSecurityStore : IUserStore {

        /// <summary>
        /// Asynchronously gets a user's account security state.
        /// </summary>
        /// <param name="user">The user to whom the security state belongs.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the security state, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserSecurity?> GetSecurityAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to create or conditionally update a user's security state asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted security state against <paramref name="expected"/> atomically. A <see langword="null"/> expectation requires that no security record exists.</para>
        /// <para>Binding or clearing the recovery challenge is atomic with its creation or consumption. Changes to <see cref="UserSecurity.Rotation"/> are atomic with required revocations. The rotation value never wraps or decreases.</para>
        /// </remarks>
        /// <param name="user">The user to whom the security state belongs.</param>
        /// <param name="expected">The expected persisted security state, or <see langword="null"/> if no record is expected to exist.</param>
        /// <param name="security">The replacement security state.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the security state was saved; otherwise, <see langword="false"/> (for example, if the expected state does not match).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateSecurityAsync(User user, UserSecurity? expected, UserSecurity security, CancellationToken cancellationToken = default);

    }

}
