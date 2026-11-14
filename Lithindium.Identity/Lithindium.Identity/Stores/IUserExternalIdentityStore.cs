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
    /// Defines the persistence contract for identities associated with external issuers.
    /// </summary>
    public interface IUserExternalIdentityStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's external identities.
        /// </summary>
        /// <param name="user">The user whose external identities are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's external identities.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously gets a user's external identity by unique identifier.
        /// </summary>
        /// <param name="user">The user to whom the external identity belongs.</param>
        /// <param name="uuid">The unique identifier of the external identity to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the external identity, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds a user by external issuer and subject.
        /// </summary>
        /// <remarks>
        /// Compares <paramref name="issuer"/> and <paramref name="subject"/> exactly and case-sensitively.
        /// </remarks>
        /// <param name="issuer">The issuer of the external identity to find.</param>
        /// <param name="subject">The unique subject identifier assigned by the issuer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add an external identity asynchronously.
        /// </summary>
        /// <remarks>
        /// Issuer and subject pairs are unique across the store.
        /// </remarks>
        /// <param name="user">The user to whom the external identity belongs.</param>
        /// <param name="identity">The external identity to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the external identity was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to conditionally update external identity metadata asynchronously.
        /// </summary>
        /// <remarks>
        /// Compares the entire persisted external identity against <paramref name="expected"/> atomically. The identifier, issuer, and subject are immutable.
        /// </remarks>
        /// <param name="user">The user to whom the external identity belongs.</param>
        /// <param name="expected">The expected persisted external identity.</param>
        /// <param name="identity">The replacement external identity.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the external identity was updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove an external identity asynchronously.
        /// </summary>
        /// <param name="user">The user to whom the external identity belongs.</param>
        /// <param name="identity">The external identity to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the external identity was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
