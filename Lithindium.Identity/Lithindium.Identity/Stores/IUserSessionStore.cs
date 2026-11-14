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
    /// Authentication session persistence.
    /// </summary>
    /// <remarks>
    /// Session UUIDs are never reused. Revocation or expiration permanently invalidates a session.
    /// Authentication changes and required revocations are committed together.
    /// </remarks>
    public interface IUserSessionStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and authenticated sessions.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user's pending and authenticated sessions.</returns>
        IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a session by UUID.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="uuid">Session UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Only an existing, unexpired session with no challenge can authenticate its owner.
        /// </remarks>
        /// <returns>The session, or <see langword="null"/> if not found.</returns>
        Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a pending or authenticated session.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="session">New session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Requires expiration after creation. The session is issued within the transaction validating the authorizing authentication state.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a session.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected session.</param>
        /// <param name="session">Updated session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares the entire expected state atomically, including authentication methods by content.
        /// The UUID and creation time are immutable. Successful proof clears the pending challenge and updates authentication evidence in the same transaction as consumption.
        /// Expired sessions cannot be reactivated.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateSessionAsync(User user, UserSession expected, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a session.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="session">Session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user's sessions, optionally preserving one UUID.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="exceptUuid">Session UUID to preserve, or <see langword="null"/> to remove all sessions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes matching sessions and their pending challenges in the same transaction. No matching sessions is a successful outcome.
        /// </remarks>
        /// <returns><see langword="true"/> if the removal succeeded, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes sessions expiring at or before the UTC cutoff.
        /// </summary>
        /// <param name="expiresAt">Inclusive UTC expiration cutoff.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes associated pending challenges in the same transaction.
        /// </remarks>
        /// <returns>The number of sessions removed.</returns>
        Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
