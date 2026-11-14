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
    /// Defines the persistence contract for user accounts.
    /// </summary>
    /// <remarks>
    /// <para>All reads and writes, including those on derived store interfaces, require an active transaction.
    /// Operations outside an active transaction, including enumeration of asynchronous sequences, throw <see cref="InvalidOperationException"/>.</para>
    /// <para>Each store instance owns at most one active transaction. All supported store capabilities on that instance share it.
    /// Store instances must not be shared by concurrent callers. Disposing the store rolls back any unsaved changes.</para>
    /// <para>All writes are atomic within the transaction. A mutation returns <see langword="false"/> for an expected rejection, such as a missing entity, a uniqueness violation, or a stale expectation, without leaving partial changes from that mutation.
    /// A successful mutation is not durable until <see cref="SaveAsync"/> succeeds.
    /// Invalid arguments, cancellation, and unexpected infrastructure failures throw exceptions.</para>
    /// <para>Associated state is scoped to the owner's UUID. Record snapshots do not establish authorization.</para>
    /// </remarks>
    public interface IUserStore : IAsyncDisposable {

        /// <summary>
        /// Asynchronously begins an identity store transaction.
        /// </summary>
        /// <remarks>
        /// The transaction provides serializable isolation. All operations on this store participate in the
        /// transaction and observe its own changes. Nested transactions and concurrent operations are not supported.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="InvalidOperationException">A transaction is already active.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously saves all changes and commits the active transaction.
        /// </summary>
        /// <remarks>
        /// Validates all persistence invariants before committing. A known rejection or commit conflict rolls back
        /// all changes and returns <see langword="false"/>. An uncertain commit outcome throws and renders the store unusable;
        /// callers must not assume rollback or blindly retry. A successful save or known rejection ends the transaction.
        /// An aborted transaction cannot save. Retry known transient conflicts only in a new transaction using fresh state.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the transaction committed; otherwise, <see langword="false"/> for a known rejection.</returns>
        /// <exception cref="InvalidOperationException">No usable transaction is active.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> SaveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously rolls back unsaved changes and ends the active transaction.
        /// </summary>
        /// <remarks>
        /// Does nothing if no transaction is active. After rollback, the store may begin a new transaction
        /// unless an unexpected failure has rendered the store unusable.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
        Task RollbackAsync(CancellationToken cancellationToken = default);

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
        /// Atomically removes all dependent identity state, including challenges. Historical audit records are preserved.
        /// </remarks>
        /// <param name="uuid">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was deleted; otherwise, <see langword="false"/> (for example, if the user does not exist).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryDeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
