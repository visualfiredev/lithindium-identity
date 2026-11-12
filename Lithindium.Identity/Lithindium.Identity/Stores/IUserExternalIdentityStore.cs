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
        /// <remarks>
        /// The combination of issuer and subject must be unique
        /// across all external identities within the identity store.
        /// </remarks>
        Task AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the descriptive metadata of an external identity associated with the specified user.
        /// </summary>
        /// <param name="user">The user associated with the external identity.</param>
        /// <param name="identity">The external identity containing the updated metadata.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// Only the name and email address may be modified.
        /// The issuer and subject must remain unchanged.
        /// </remarks>
        Task UpdateExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the external identity.</param>
        /// <param name="identity">The external identity to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        Task RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
