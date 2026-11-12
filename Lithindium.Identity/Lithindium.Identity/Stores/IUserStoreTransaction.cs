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
    /// Represents an atomic transaction within an identity store.
    /// </summary>
    /// <remarks>
    /// Disposing an uncommitted transaction must roll back its changes.
    /// A completed or rolled-back transaction cannot be reused.
    /// </remarks>
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Commits the changes performed within the transaction.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.ConcurrencyConflict"/>, or another applicable persistence conflict.
        /// </returns>
        /// <remarks>
        /// If a known persistence conflict prevents commit, return the corresponding error
        /// and ensure the transaction is rolled back. An uncertain commit outcome must be
        /// reported as an exception.
        /// </remarks>
        Task<UserStoreError> CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the changes performed within the transaction.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
