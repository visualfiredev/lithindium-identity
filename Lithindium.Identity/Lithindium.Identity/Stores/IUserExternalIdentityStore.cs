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
    /// Provides persistence operations for user external identities.
    /// </summary>
    public interface IUserExternalIdentityStore : IUserStore {

        /// <summary>
        /// Retrieves the external identities associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose external identities to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The external identities associated with the user.</returns>
        IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the specified external identity associated with a user.
        /// </summary>
        /// <param name="user">The user associated with the external identity.</param>
        /// <param name="uuid">The universally unique identifier of the external identity.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The external identity, or <see langword="null"/> if no matching identity exists.</returns>
        Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user associated with the specified external issuer and subject.
        /// </summary>
        /// <param name="issuer">The issuer of the external identity.</param>
        /// <param name="subject">The unique subject identifier assigned by the issuer.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        /// <remarks>
        /// Issuer and subject comparisons must be exact and case-sensitive.
        /// </remarks>
        Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an external identity to the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the external identity.</param>
        /// <param name="identity">The external identity to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>,
        /// <see cref="UserStoreError.DuplicateExternalIdentity"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The combination of issuer and subject must be unique
        /// across all external identities within the identity store.
        /// </remarks>
        Task<UserStoreError> AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update the descriptive metadata of an external identity associated with the specified user.
        /// </summary>
        /// <param name="user">The user associated with the external identity.</param>
        /// <param name="expected">The expected existing external identity state.</param>
        /// <param name="identity">The external identity containing the updated metadata.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// Only the name and email address may be modified.
        /// The UUID, issuer and subject must remain unchanged.
        /// The comparison and update must be performed atomically.
        /// The expected persisted metadata must match exactly; both records must identify the same external identity.
        /// </remarks>
        Task<UserStoreError> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the external identity.</param>
        /// <param name="identity">The external identity to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        Task<UserStoreError> RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
