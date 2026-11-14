// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Defines the persistence contract for user time-based one-time password (TOTP) authenticators.
    /// </summary>
    /// <remarks>
    /// Pending authenticators cannot satisfy authentication. Authenticator identifiers are never reused.
    /// </remarks>
    public interface IUserTotpStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's pending and verified authenticators.
        /// </summary>
        /// <param name="user">The user whose authenticators are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's pending and verified authenticators.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets a user's authenticator by unique identifier.
        /// </summary>
        /// <param name="user">The user to whom the authenticator belongs.</param>
        /// <param name="uuid">The unique identifier of the authenticator to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authenticator, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add a pending or verified authenticator asynchronously.
        /// </summary>
        /// <param name="user">The user to whom the authenticator belongs.</param>
        /// <param name="totp">The authenticator to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the authenticator was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to conditionally update an authenticator asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted authenticator against <paramref name="expected"/> atomically, including the secret by content. The identifier, secret, algorithm, digit count, and period are immutable.</para>
        /// <para>The <see cref="UserTotp.LastStep"/> value never decreases or resets after acceptance. Accepting a code requires a strictly greater time step. Successful enrollment clears the challenge in the same transaction as consumption.</para>
        /// </remarks>
        /// <param name="user">The user to whom the authenticator belongs.</param>
        /// <param name="expected">The expected persisted authenticator.</param>
        /// <param name="totp">The replacement authenticator.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the authenticator was updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateTotpAsync(User user, UserTotp expected, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove an authenticator asynchronously.
        /// </summary>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// </remarks>
        /// <param name="user">The user to whom the authenticator belongs.</param>
        /// <param name="totp">The authenticator to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the authenticator was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

    }

}
