// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Defines the persistence contract for user recovery codes.
    /// </summary>
    public interface IUserRecoveryCodeStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's unused recovery codes.
        /// </summary>
        /// <param name="user">The user whose recovery codes are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's unused recovery codes.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to replace all of a user's recovery codes asynchronously.
        /// </summary>
        /// <remarks>
        /// Replaces the collection atomically. Recovery code identifiers and code hashes are unique as applicable.
        /// </remarks>
        /// <param name="user">The user whose recovery codes are replaced.</param>
        /// <param name="codes">The collection of replacement recovery codes. An empty collection removes all existing codes.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the recovery codes were replaced; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a recovery code asynchronously.
        /// </summary>
        /// <remarks>
        /// Finds and deletes the matching code atomically by hash content, scoped to the user.
        /// </remarks>
        /// <param name="user">The user to whom the recovery code belongs.</param>
        /// <param name="codeHash">The cryptographic hash of the recovery code to consume.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the recovery code was consumed; otherwise, <see langword="false"/> (for example, if the code is missing, already consumed, belongs to another user).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously counts a user's unused recovery codes.
        /// </summary>
        /// <param name="user">The user whose recovery codes are counted.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of unused recovery codes.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

    }

}
