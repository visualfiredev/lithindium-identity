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
    /// User persistence.
    /// </summary>
    /// <remarks>
    /// All writes are atomic. Mutations return <see langword="false"/> for expected rejections, including missing entities, uniqueness violations and stale expectations, without leaving partial changes.
    /// Invalid arguments, cancellation and unexpected infrastructure failures throw exceptions.
    /// Successful mutations persist immediately outside transactions and on commit within transactions.
    /// Associated state is scoped to the owner's UUID; snapshots do not establish authorization.
    /// A known transaction abort prevents commit. Retries require an entirely new transaction with fresh state; uncertain commits are not retryable.
    /// </remarks>
    public interface IUserStore {

        /// <summary>
        /// Begins an atomic transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// All capabilities on the same instance participate until commit, rollback or disposal.
        /// The transaction provides serializable isolation or equivalent guarantees for current reads, writes and entity sets. Nested transactions are unsupported.
        /// </remarks>
        /// <returns>The transaction.</returns>
        Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by UUID.
        /// </summary>
        /// <param name="uuid">User UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by username.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="user">New user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// UUIDs are never reused, and usernames are unique.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was created, otherwise <see langword="false"/>.</returns>
        Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a user.
        /// </summary>
        /// <param name="expected">Expected user.</param>
        /// <param name="user">Updated user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares all expected persisted values atomically. The UUID is immutable.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="uuid">User UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes all dependent identity state, including challenges, in the same transaction. Historical audits are preserved.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was deleted, otherwise <see langword="false"/>.</returns>
        Task<bool> TryDeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
