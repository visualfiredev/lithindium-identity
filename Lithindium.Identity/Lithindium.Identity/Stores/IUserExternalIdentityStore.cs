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
    /// External identity persistence.
    /// </summary>
    public interface IUserExternalIdentityStore : IUserStore {

        /// <summary>
        /// Gets a user's external identities.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>External identities.</returns>
        IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user's external identity by UUID.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="uuid">External identity UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>External identity, or <see langword="null"/> if absent.</returns>
        Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by external issuer and subject.
        /// </summary>
        /// <param name="issuer">External issuer.</param>
        /// <param name="subject">Issuer-assigned unique subject.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User, or <see langword="null"/> if absent.</returns>
        /// <remarks>
        /// Compare issuer and subject exactly and case-sensitively.
        /// </remarks>
        Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an external identity.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="identity">External identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>,
        /// <see cref="UserStoreError.DuplicateExternalIdentity"/>.
        /// </returns>
        /// <remarks>
        /// Issuer and subject pairs must be unique across the store.
        /// </remarks>
        Task<UserStoreError> AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a conditional metadata update.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="expected">Expected identity.</param>
        /// <param name="identity">Identity with updated metadata.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>.
        /// </returns>
        /// <remarks>
        /// Atomically compare expected metadata exactly and update only name and email.
        /// Both snapshots must identify the same identity; UUID, issuer and subject are immutable.
        /// </remarks>
        Task<UserStoreError> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="identity">External identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>.
        /// </returns>
        Task<UserStoreError> RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
