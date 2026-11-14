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
        IAsyncEnumerable<UserExternalIdentity> GetExternalIdentitiesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an external identity by UUID, or returns null.
        /// </summary>
        Task<UserExternalIdentity?> GetExternalIdentityAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by external issuer and subject, or returns null.
        /// </summary>
        /// <remarks>
        /// Compare issuer and subject exactly and case-sensitively.
        /// </remarks>
        Task<User?> FindByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an external identity.
        /// </summary>
        /// <remarks>
        /// Issuer and subject pairs must be unique across the store.
        /// </remarks>
        Task<bool> TryAddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates external identity metadata.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically. UUID, issuer and subject are immutable.
        /// </remarks>
        Task<bool> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity.
        /// </summary>
        Task<bool> TryRemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
