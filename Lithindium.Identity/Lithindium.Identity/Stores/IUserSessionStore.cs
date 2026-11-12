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
    /// Commit authentication state changes and policy-required revocations together.
    /// </remarks>
    public interface IUserSessionStore : IUserStore {

        /// <summary>
        /// Gets a user's persisted sessions.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Persisted sessions.</returns>
        IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user's session by UUID.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="uuid">Session UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Session, or <see langword="null"/> if absent.</returns>
        /// <remarks>
        /// Retrieval alone does not establish validity. Authentication requires an existing,
        /// unexpired session belonging to the authenticated user.
        /// </remarks>
        Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an authentication session.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="session">Session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreAddSession"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Identifier"/>.
        /// </returns>
        /// <remarks>
        /// Expiration must follow authentication. Issue within the transaction validating
        /// the authorizing authentication state, including required verification state changes.
        /// </remarks>
        Task<OperationResult> AddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a session.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="session">Session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveSession"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        Task<OperationResult> RemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user's sessions.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="exceptUuid">Session UUID to preserve, or <see langword="null"/> for all.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveSessions"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// </returns>
        /// <remarks>
        /// Remove matching sessions atomically; no matches is success.
        /// </remarks>
        Task<OperationResult> RemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes expired sessions.
        /// </summary>
        /// <param name="expiresAt">Inclusive UTC expiration cutoff.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of sessions removed.</returns>
        Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
