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
    /// Provides persistence operations for verified user email addresses.
    /// </summary>
    public interface IUserEmailStore : IUserStore {

        /// <summary>
        /// Retrieves the verified email addresses associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose email addresses to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The email addresses associated with the user.</returns>
        IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user associated with the specified email address.
        /// </summary>
        /// <param name="address">The verified email address of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a verified email address to the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the email address.</param>
        /// <param name="email">The email address to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateEmail"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// If the email address is designated as primary, any existing primary
        /// designation must be removed atomically.
        /// </remarks>
        Task<UserStoreError> AddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a verified email address from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the email address.</param>
        /// <param name="email">The email address to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        Task<UserStoreError> RemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the primary email address of the specified user.
        /// </summary>
        /// <param name="user">The user whose primary email address to set.</param>
        /// <param name="email">The email address to designate as primary, or <see langword="null"/> to clear the designation.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// Changes to the primary email address must be performed atomically.
        /// Clearing an absent primary designation succeeds; a non-null email must belong to the user.
        /// </remarks>
        Task<UserStoreError> SetPrimaryEmailAsync(User user, UserEmail? email, CancellationToken cancellationToken = default);

    }

}
