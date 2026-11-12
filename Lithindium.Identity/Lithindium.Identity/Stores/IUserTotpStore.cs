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
    /// Provides persistence operations for user TOTP authenticators.
    /// </summary>
    public interface IUserTotpStore : IUserStore {

        /// <summary>
        /// Retrieves the TOTP authenticators associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose authenticators to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The authenticators associated with the user.</returns>
        IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the specified TOTP authenticator associated with a user.
        /// </summary>
        /// <param name="user">The user associated with the authenticator.</param>
        /// <param name="uuid">The universally unique identifier of the authenticator.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The authenticator, or <see langword="null"/> if no matching authenticator exists.</returns>
        Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a TOTP authenticator to the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the authenticator.</param>
        /// <param name="totp">The authenticator to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>, as applicable.
        /// </returns>
        Task<UserStoreError> AddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a TOTP authenticator from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the authenticator.</param>
        /// <param name="totp">The authenticator to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        Task<UserStoreError> RemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to change the name of a TOTP authenticator associated with the specified user.
        /// </summary>
        /// <param name="user">The user associated with the authenticator.</param>
        /// <param name="totp">The authenticator containing the expected existing name.</param>
        /// <param name="name">The new name, or <see langword="null"/> to remove the name.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The comparison and update must be performed atomically. Only the name is compared
        /// and changed; concurrent advances of the accepted time step must be preserved.
        /// </remarks>
        Task<UserStoreError> TryRenameTotpAsync(User user, UserTotp totp, string? name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to advance the most recently accepted authentication time step
        /// of a TOTP authenticator.
        /// </summary>
        /// <param name="user">The user associated with the authenticator.</param>
        /// <param name="totp">The authenticator whose time step to advance.</param>
        /// <param name="step">The accepted authentication time step.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.TotpStepAlreadyUsed"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The comparison and update must be performed atomically.
        /// The supplied step must be strictly greater than the currently persisted step, if one exists.
        /// </remarks>
        Task<UserStoreError> TryAdvanceTotpStepAsync(User user, UserTotp totp, ulong step, CancellationToken cancellationToken = default);

    }

}
