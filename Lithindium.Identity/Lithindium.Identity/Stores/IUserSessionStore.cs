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
    /// Never reuse session UUIDs. Revocation or expiration permanently invalidates a session.
    /// Commit authentication changes and required revocations together.
    /// </remarks>
    public interface IUserSessionStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and authenticated sessions.
        /// </summary>
        /// <returns>The user's pending and authenticated sessions.</returns>
        IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a session by UUID.
        /// </summary>
        /// <remarks>
        /// Only an existing, unexpired session with no challenge can authenticate its owner.
        /// </remarks>
        /// <returns>The session, or <see langword="null"/> if not found.</returns>
        Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a pending or authenticated session.
        /// </summary>
        /// <remarks>
        /// Expiration must follow creation. Issue within the transaction validating the authorizing authentication state.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a session.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically, including authentication methods by content.
        /// UUID and creation time are immutable. Clear a pending challenge and update authentication evidence only after successful proof, atomically with consumption.
        /// Never reactivate an expired session.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateSessionAsync(User user, UserSession expected, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a session.
        /// </summary>
        /// <remarks>
        /// Remove its pending challenge atomically.
        /// </remarks>
        /// <returns><see langword="true"/> if the session was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user's sessions, optionally preserving one UUID.
        /// </summary>
        /// <remarks>
        /// Remove matching sessions and associated pending challenges atomically; no matches is success.
        /// </remarks>
        /// <returns><see langword="true"/> if the removal succeeded, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes sessions expiring at or before the UTC cutoff.
        /// </summary>
        /// <remarks>
        /// Remove associated pending challenges atomically.
        /// </remarks>
        /// <returns>The number of sessions removed.</returns>
        Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
