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
    /// All writes are atomic. Boolean mutation methods return true when the operation succeeds,
    /// or false for an expected rejection, including missing entities, uniqueness violations or stale expectations.
    /// A rejected mutation leaves no partial changes.
    /// Invalid arguments, cancellation and unexpected infrastructure failures throw.
    /// Outside a transaction, successful mutations persist immediately. Within a transaction, changes persist on commit.
    /// Scope associated state to the owner's UUID; snapshots do not establish authorization.
    /// A known transaction abort prevents commit; retry the entire transaction from fresh state,
    /// never an uncertain commit.
    /// </remarks>
    public interface IUserStore {

        /// <summary>
        /// Begins an atomic transaction.
        /// </summary>
        /// <remarks>
        /// All capabilities on the same instance participate until commit, rollback or disposal.
        /// Use serializable isolation or equivalent guarantees for current reads, writes and entity sets.
        /// Do not nest transactions.
        /// </remarks>
        /// <returns>The transaction.</returns>
        Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by UUID.
        /// </summary>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by username.
        /// </summary>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <remarks>
        /// UUIDs must never be reused; usernames must be unique.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was created, otherwise <see langword="false"/>.</returns>
        Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a user.
        /// </summary>
        /// <remarks>
        /// UUID is immutable. Compare all expected persisted values atomically.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <remarks>
        /// Atomically remove dependent identity state, including challenges; preserve historical audits.
        /// </remarks>
        /// <returns><see langword="true"/> if the user was deleted, otherwise <see langword="false"/>.</returns>
        Task<bool> TryDeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
