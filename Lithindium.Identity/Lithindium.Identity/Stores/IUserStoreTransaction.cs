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
    /// An atomic identity store transaction.
    /// </summary>
    /// <remarks>
    /// Disposal rolls back uncommitted changes. Completed or rolled-back transactions cannot be reused.
    /// Follow <see cref="IUserStore.BeginTransactionAsync"/> isolation guarantees.
    /// Aborted transactions cannot commit; retry the entire transaction from fresh state
    /// after retryable serialization conflicts, not just the failed operation.
    /// </remarks>
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.ConcurrencyConflict"/>, or another applicable persistence conflict.
        /// </returns>
        /// <remarks>
        /// On a known commit conflict, return its error and roll back.
        /// Throw on uncertain commit outcomes; do not assume rollback or automatically retry.
        /// </remarks>
        Task<UserStoreError> CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
