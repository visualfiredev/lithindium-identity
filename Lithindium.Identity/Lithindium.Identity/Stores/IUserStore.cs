// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Provides persistence operations for users within the identity system.
    /// </summary>
    public interface IUserStore {

        /// <summary>
        /// Retrieves the user with the specified universally unique identifier.
        /// </summary>
        /// <param name="uuid">The universally unique identifier of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user with the specified username.
        /// </summary>
        /// <param name="username">The unique username of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user within the identity store.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        Task CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update an existing user within the identity store.
        /// </summary>
        /// <param name="expected">The expected existing user state.</param>
        /// <param name="user">The user containing the updated values.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see langword="true"/> if the user was successfully updated;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The comparison and update must be performed atomically.
        /// Both records must identify the same user.
        /// </remarks>
        Task<bool> TryUpdateAsync(User expected, User user, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes an existing user from the identity store.
        /// </summary>
        /// <param name="uuid">The universally unique identifier of the user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        Task DeleteAsync(Guid uuid, CancellationToken cancellationToken = default);

    }

}
