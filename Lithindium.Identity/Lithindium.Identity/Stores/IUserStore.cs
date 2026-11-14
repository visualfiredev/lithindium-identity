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
    /// Defines the persistence contract for user accounts and shared store transactions.
    /// </summary>
    /// <remarks>
    /// <para>All writes are atomic. A mutation returns <see langword="false"/> for an expected rejection, such as a missing entity, a uniqueness violation, or a stale expectation, without leaving partial changes. Invalid arguments, cancellation, and unexpected infrastructure failures throw exceptions.</para>
    /// <para>Successful mutations persist immediately outside a transaction and on commit within a transaction. Associated state is scoped to the owner's UUID. Record snapshots do not establish authorization.</para>
    /// <para>A known transaction abort prevents commit. A retry requires a new transaction and fresh state. An uncertain commit outcome is not safe to retry automatically.</para>
    /// </remarks>
    public interface IUserStore {

        /// <summary>
        /// Asynchronously begins an atomic identity store transaction.
        /// </summary>
        /// <remarks>
        /// All capabilities on the same store instance participate in the transaction until it is committed, rolled back, or disposed. The transaction provides serializable isolation or equivalent guarantees for current reads, writes, and entity sets. Nested transactions are not supported.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the new transaction.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds a user by unique identifier.
        /// </summary>
        /// <param name="uuid">The unique identifier of the user to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds a user by username.
        /// </summary>
        /// <param name="username">The username of the user to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to create a user asynchronously.
        /// </summary>
        /// <remarks>
        /// User identifiers are never reused, and usernames are unique.
        /// </remarks>
        /// <param name="user">The user to create.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was created; otherwise, <see langword="false"/> (for example, if the identifier or username is already in use).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update a user asynchronously if the persisted state matches the expected state.
        /// </summary>
        /// <remarks>
        /// Compares all persisted values against <paramref name="expected"/> atomically before applying <paramref name="user"/>. The user identifier is immutable.
        /// </remarks>
        /// <param name="expected">The expected persisted user.</param>
        /// <param name="user">The replacement user record.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was updated; otherwise, <see langword="false"/> (for example, if the user no longer exists, the expected state does not match, or the update is rejected).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to delete a user asynchronously.
        /// </summary>
        /// <remarks>
        /// Removes all dependent identity state, including challenges, in the same transaction. Historical audit records are preserved.
        /// </remarks>
        /// <param name="uuid">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was deleted; otherwise, <see langword="false"/> (for example, if the user does not exist).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryDeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
