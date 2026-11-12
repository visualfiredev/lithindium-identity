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
    /// Result-returning methods report <see cref="StatusCode.Success"/> on success or an expected conflict,
    /// including <see cref="StatusCode.Conflict"/>; other methods retain their documented results.
    /// Use each method's documented operation code; success context must be zero.
    /// Failed mutations leave no partial changes. Invalid arguments, cancellation and unexpected
    /// infrastructure failures throw. Changes persist immediately outside transactions;
    /// transactional changes persist on commit. Identify users by UUID, not snapshot equality,
    /// and scope associated entities to their owner.
    /// </remarks>
    public interface IUserStore {

        /// <summary>
        /// Begins an atomic transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction.</returns>
        /// <remarks>
        /// All operations and capabilities on this instance participate in the transaction until completion or disposal.
        /// Require serializable isolation or equivalent identity invariants, including concurrent
        /// reads, writes and changes to entity sets. Security decisions require current transactional
        /// state, not stale replicas or external snapshots.
        /// Report known serialization aborts as <see cref="StatusCode.Conflict"/> from
        /// an error-returning operation or <see cref="IUserStoreTransaction.CommitAsync"/>.
        /// Disposal rolls back uncommitted changes.
        /// </remarks>
        Task<IUserStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by UUID.
        /// </summary>
        /// <param name="uuid">User UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User, or <see langword="null"/> if absent.</returns>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by username.
        /// </summary>
        /// <param name="username">Unique username.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User, or <see langword="null"/> if absent.</returns>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="user">New user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreCreateUser"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Identifier"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Username"/>.
        /// </returns>
        Task<OperationResult> CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a conditional user update.
        /// </summary>
        /// <param name="expected">Expected user.</param>
        /// <param name="user">Updated user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreUpdateUser"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.Conflict"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Username"/>.
        /// </returns>
        /// <remarks>
        /// Both snapshots must identify the same user. Atomically compare all expected persisted
        /// values exactly before replacing them.
        /// </remarks>
        Task<OperationResult> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <remarks>
        /// Atomically remove all dependent identity state; preserve historical audits.
        /// </remarks>
        /// <param name="uuid">User UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreDeleteUser"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// </returns>
        Task<OperationResult> DeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
