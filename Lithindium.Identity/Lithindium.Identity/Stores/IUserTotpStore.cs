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
    /// <remarks>
    /// Pending authenticators cannot satisfy authentication. Authenticator UUIDs are never reused.
    /// </remarks>
    public interface IUserTotpStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified authenticators.
        /// </summary>
        /// <returns>The user's pending and verified authenticators.</returns>
        IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an authenticator by UUID.
        /// </summary>
        /// <returns>The authenticator, or <see langword="null"/> if not found.</returns>
        Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an authenticator.
        /// </summary>
        /// <returns><see langword="true"/> if the authenticator was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates an authenticator.
        /// </summary>
        /// <remarks>
        /// Compares the entire expected state atomically, including the secret by content.
        /// The UUID, secret, algorithm, digits and period are immutable. LastStep never decreases or resets after acceptance.
        /// Accepting a code requires a strictly greater LastStep. Successful enrollment clears the challenge in the same transaction as consumption.
        /// </remarks>
        /// <returns><see langword="true"/> if the authenticator was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateTotpAsync(User user, UserTotp expected, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an authenticator.
        /// </summary>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// </remarks>
        /// <returns><see langword="true"/> if the authenticator was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

    }

}
