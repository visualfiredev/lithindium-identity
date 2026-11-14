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
    /// Represents an atomic transaction across identity store operations.
    /// </summary>
    /// <remarks>
    /// Disposing of an uncommitted transaction rolls back its changes. Completed or aborted transactions cannot be reused.
    /// </remarks>
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Attempts to commit the transaction asynchronously.
        /// </summary>
        /// <remarks>
        /// An uncertain commit outcome or an unexpected failure results in an exception.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the transaction was committed; otherwise, <see langword="false"/> if the transaction is known to have aborted without committing any changes.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously rolls back the transaction.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous rollback operation.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
