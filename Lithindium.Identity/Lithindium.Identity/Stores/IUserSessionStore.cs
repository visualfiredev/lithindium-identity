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
    /// Defines the persistence contract for pending and authenticated user sessions.
    /// </summary>
    /// <remarks>
    /// Session identifiers are never reused. Revocation or expiration permanently invalidates a session. Authentication changes and required revocations are committed together.
    /// </remarks>
    public interface IUserSessionStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's pending and authenticated sessions.
        /// </summary>
        /// <param name="user">The user whose sessions are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's pending and authenticated sessions.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets a user's session by unique identifier.
        /// </summary>
        /// <remarks>
        /// Retrieving a session does not establish authentication. Only an existing, unexpired session without a pending challenge can authenticate its owner.
        /// </remarks>
        /// <param name="user">The user to whom the session belongs.</param>
        /// <param name="uuid">The unique identifier of the session to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the session, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add a pending or authenticated session asynchronously.
        /// </summary>
        /// <remarks>
        /// Requires an expiration time after creation. The session is issued in the transaction that validates the authorizing authentication state.
        /// </remarks>
        /// <param name="user">The user to whom the session belongs.</param>
        /// <param name="session">The session to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the session was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to conditionally update a session asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted session against <paramref name="expected"/> atomically, including authentication methods by content. The identifier and creation time are immutable.</para>
        /// <para>Successful proof clears the pending challenge and updates authentication evidence in the same transaction as consumption. Expired sessions cannot be reactivated.</para>
        /// </remarks>
        /// <param name="user">The user to whom the session belongs.</param>
        /// <param name="expected">The expected persisted session.</param>
        /// <param name="session">The replacement session.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the session was updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateSessionAsync(User user, UserSession expected, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove a session asynchronously.
        /// </summary>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// </remarks>
        /// <param name="user">The user to whom the session belongs.</param>
        /// <param name="session">The session to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the session was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove a user's sessions asynchronously, optionally preserving one session.
        /// </summary>
        /// <remarks>
        /// Removes matching sessions and their pending challenges in the same transaction. Having no matching sessions is a successful outcome.
        /// </remarks>
        /// <param name="user">The user whose sessions are removed.</param>
        /// <param name="exceptUuid">The unique identifier of the session to preserve, or <see langword="null"/> to remove all sessions.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the removal succeeded, including when no sessions match; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes sessions that expire at or before a specified time.
        /// </summary>
        /// <remarks>
        /// Removes associated pending challenges in the same transaction.
        /// </remarks>
        /// <param name="expiresAt">The inclusive expiration cutoff, in UTC.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of sessions removed.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
