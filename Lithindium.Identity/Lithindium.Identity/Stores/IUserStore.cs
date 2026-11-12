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
    /// Provides persistence operations for users within the identity system.
    /// </summary>
    /// <remarks>
    /// Methods returning <see cref="UserStoreError"/> return
    /// <see cref="UserStoreError.None"/> on success and an appropriate error for
    /// expected persistence conflicts, including <see cref="UserStoreError.ConcurrencyConflict"/>
    /// when a concurrent operation prevents the requested change. Other methods retain
    /// their documented return values. Failed mutations must not leave partial changes.
    /// Invalid arguments, cancellation, and unexpected infrastructure failures are
    /// reported as exceptions.
    /// Operations persist immediately unless participating in a transaction;
    /// successful operations within a transaction are not durable until committed.
    /// Store capabilities identify users by their UUID, not by snapshot equality.
    /// Associated entities must be scoped to the specified user.
    /// </remarks>
    public interface IUserStore {

        /// <summary>
        /// Begins an atomic transaction within the identity store.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The transaction associated with this store instance.</returns>
        /// <remarks>
        /// All operations on this store instance, including its implemented capabilities,
        /// must participate in the transaction until it is completed or disposed.
        /// Transactions must provide serializable isolation or equivalent guarantees
        /// for identity invariants, including concurrent reads, writes, and changes
        /// to associated entity sets. Security-sensitive decisions must not rely on
        /// stale replica reads or snapshots obtained outside the transaction.
        /// A known serialization conflict that aborts a transaction must be reported as
        /// <see cref="UserStoreError.ConcurrencyConflict"/> by an error-returning operation
        /// or by <see cref="IUserStoreTransaction.CommitAsync"/>.
        /// Disposing an uncommitted transaction must roll back its changes.
        /// </remarks>
        Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user with the specified universally unique identifier.
        /// </summary>
        /// <param name="uuid">The universally unique identifier of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user with the specified username.
        /// </summary>
        /// <param name="username">The unique username of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user within the identity store.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.DuplicateIdentifier"/>,
        /// <see cref="UserStoreError.DuplicateUsername"/>, as applicable.
        /// </returns>
        Task<UserStoreError> CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update an existing user within the identity store.
        /// </summary>
        /// <param name="expected">The expected existing user state.</param>
        /// <param name="user">The user containing the updated values.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>,
        /// <see cref="UserStoreError.DuplicateUsername"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The comparison and update must be performed atomically.
        /// Both records must identify the same user.
        /// The expected persisted values must match exactly before applying the replacement.
        /// </remarks>
        Task<UserStoreError> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an existing user from the identity store.
        /// </summary>
        /// <param name="uuid">The universally unique identifier of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>.
        /// </returns>
        Task<UserStoreError> DeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
