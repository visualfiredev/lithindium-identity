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
        /// Return true on commit, or false only for a known transaction abort with no committed changes.
        /// Throw if the commit outcome is uncertain or an unexpected failure occurs.
        /// </remarks>
        Task<bool> TryCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
