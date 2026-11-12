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
        /// <see cref="OperationCode.StoreAddExternalIdentity"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Identifier"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.ExternalIdentity"/>.
        /// </returns>
        /// <remarks>
        /// Issuer and subject pairs must be unique across the store.
        /// </remarks>
        Task<OperationResult> AddExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a conditional metadata update.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="expected">Expected identity.</param>
        /// <param name="identity">Identity with updated metadata.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreUpdateExternalIdentity"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// <see cref="StatusCode.Conflict"/>.
        /// </returns>
        /// <remarks>
        /// Atomically compare expected metadata exactly and update only name and email.
        /// Both snapshots must identify the same identity; UUID, issuer and subject are immutable.
        /// </remarks>
        Task<OperationResult> TryUpdateExternalIdentityAsync(User user, UserExternalIdentity expected, UserExternalIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an external identity.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="identity">External identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveExternalIdentity"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        Task<OperationResult> RemoveExternalIdentityAsync(User user, UserExternalIdentity identity, CancellationToken cancellationToken = default);

    }

}
