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
    public interface IUserStoreTransaction : IAsyncDisposable {

        /// <summary>
        /// Commits the changes performed within the transaction.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the changes performed within the transaction.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

    }

}
