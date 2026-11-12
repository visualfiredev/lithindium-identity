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
    /// TOTP authenticator persistence.
    /// </summary>
    public interface IUserTotpStore : IUserStore {

        /// <summary>
        /// Gets a user's TOTP authenticators.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Authenticators.</returns>
        IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user's TOTP authenticator by UUID.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="uuid">Authenticator UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Authenticator, or <see langword="null"/> if absent.</returns>
        Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a TOTP authenticator.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="totp">Authenticator.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>.
        /// </returns>
        Task<UserStoreError> AddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a TOTP authenticator.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="totp">Authenticator.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>.
        /// </returns>
        Task<UserStoreError> RemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a conditional authenticator rename.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="totp">Authenticator with expected name.</param>
        /// <param name="name">New name, or <see langword="null"/> to clear.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ConcurrencyConflict"/>.
        /// </returns>
        /// <remarks>
        /// Atomically compare and change only the name; preserve concurrent step advances.
        /// </remarks>
        Task<UserStoreError> TryRenameTotpAsync(User user, UserTotp totp, string? name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to advance the accepted TOTP time step.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="totp">Authenticator.</param>
        /// <param name="step">Accepted time step.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.TotpStepAlreadyUsed"/>.
        /// </returns>
        /// <remarks>
        /// Atomically accept only a step strictly greater than the persisted step, if any.
        /// </remarks>
        Task<UserStoreError> TryAdvanceTotpStepAsync(User user, UserTotp totp, ulong step, CancellationToken cancellationToken = default);

    }

}
