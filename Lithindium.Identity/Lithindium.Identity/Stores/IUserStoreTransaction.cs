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
    /// Disposal rolls back uncommitted changes. Completed or aborted transactions cannot be reused.
    /// </remarks>
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <remarks>
        /// Throws an exception if the commit outcome is uncertain or an unexpected failure occurs.
        /// </remarks>
        /// <returns><see langword="true"/> if committed, or <see langword="false"/> for a known transaction abort with no committed changes.</returns>
        Task<bool> TryCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
