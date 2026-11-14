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
        /// <returns>The user's external identities.</returns>
        IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an external identity by UUID.
        /// </summary>
        /// <returns>The external identity, or <see langword="null"/> if not found.</returns>
        Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by external issuer and subject.
        /// </summary>
        /// <remarks>
        /// Compare issuer and subject exactly and case-sensitively.
        /// </remarks>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an external identity.
        /// </summary>
        /// <remarks>
        /// Issuer and subject pairs must be unique across the store.
        /// </remarks>
        /// <returns><see langword="true"/> if the external identity was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates external identity metadata.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically. UUID, issuer and subject are immutable.
        /// </remarks>
        /// <returns><see langword="true"/> if the external identity was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity.
        /// </summary>
        /// <returns><see langword="true"/> if the external identity was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
