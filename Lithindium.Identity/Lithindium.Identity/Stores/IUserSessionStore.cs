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
    /// Provides persistence operations for user authentication sessions.
    /// </summary>
    public interface IUserSessionStore : IUserStore {

        /// <summary>
        /// Retrieves the authentication sessions associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose sessions to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The persisted sessions associated with the user.</returns>
        IAsyncEnumerable<UserSession> GetSessionsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the specified authentication session associated with a user.
        /// </summary>
        /// <param name="user">The user associated with the session.</param>
        /// <param name="uuid">The universally unique identifier of the session.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The session, or <see langword="null"/> if no matching session exists.</returns>
        Task<UserSession?> GetSessionAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an authentication session to the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the session.</param>
        /// <param name="session">The session to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>, as applicable.
        /// </returns>
        Task<UserStoreError> AddSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an authentication session from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the session.</param>
        /// <param name="session">The session to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        Task<UserStoreError> RemoveSessionAsync(User user, UserSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the authentication sessions associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose sessions to remove.</param>
        /// <param name="exceptUuid">The universally unique identifier of a session to preserve,
        /// or <see langword="null"/> to remove all sessions.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>.
        /// </returns>
        /// <remarks>
        /// The removal of matching sessions must be performed atomically.
        /// If no sessions match, the operation succeeds without removing anything.
        /// </remarks>
        Task<UserStoreError> RemoveSessionsAsync(User user, Guid? exceptUuid = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes expired authentication sessions from the identity store.
        /// </summary>
        /// <param name="expiresAt">The UTC date and time at or before which sessions must have expired.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The number of sessions removed.</returns>
        Task<int> RemoveExpiredSessionsAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
