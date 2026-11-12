// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Verified email persistence.
    /// </summary>
    public interface IUserEmailStore : IUserStore {

        /// <summary>
        /// Gets a user's verified emails.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Email addresses.</returns>
        IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by verified email.
        /// </summary>
        /// <param name="address">Verified email address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User, or <see langword="null"/> if absent.</returns>
        Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a verified email.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="email">Email address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreAddEmail"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Email"/>.
        /// </returns>
        /// <remarks>
        /// Adding a primary email must atomically clear the previous primary.
        /// </remarks>
        Task<OperationResult> AddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a verified email.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="email">Email address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveEmail"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        Task<OperationResult> RemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the primary email.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="email">New primary, or <see langword="null"/> to clear.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreSetPrimaryEmail"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        /// <remarks>
        /// Set the primary atomically. Clearing an absent primary succeeds; a non-null email
        /// must belong to the user.
        /// </remarks>
        Task<OperationResult> SetPrimaryEmailAsync(User user, UserEmail? email, CancellationToken cancellationToken = default);

    }

}
