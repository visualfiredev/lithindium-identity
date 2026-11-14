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
    /// Represents an active identity store transaction.
    /// </summary>
    /// <remarks>
    /// All supported store capabilities share one atomic transaction. Disposal rolls back unsaved changes.
    /// Saving or rolling back ends the transaction; the transaction and its store cannot be reused.
    /// An aborted transaction cannot save. Retry known transient conflicts only in a new transaction using fresh state.
    /// </remarks>
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Gets the store bound to this transaction.
        /// </summary>
        /// <remarks>
        /// Implements the same store interfaces as the store that began the transaction.
        /// All reads and writes must use this store while the transaction is active.
        /// </remarks>
        IUserStore Store { get; }

        /// <summary>
        /// Asynchronously saves all changes and commits the transaction.
        /// </summary>
        /// <remarks>
        /// Validates all persistence invariants before committing. A known rejection or commit conflict rolls back
        /// all changes and returns <see langword="false"/>. An uncertain commit outcome throws;
        /// callers must not assume rollback or blindly retry.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the transaction committed; otherwise, <see langword="false"/> for a known rejection.</returns>
        /// <exception cref="InvalidOperationException">The transaction is no longer active.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> SaveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously rolls back all unsaved changes and ends the transaction.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="InvalidOperationException">The transaction is no longer active.</exception>
        /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
