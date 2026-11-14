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
    /// Pending authenticators cannot satisfy authentication. Never reuse authenticator UUIDs.
    /// </remarks>
    public interface IUserTotpStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified authenticators.
        /// </summary>
        IAsyncEnumerable<UserTotp> GetTotpsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an authenticator by UUID, or returns null.
        /// </summary>
        Task<UserTotp?> GetTotpAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an authenticator.
        /// </summary>
        Task<bool> TryAddTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates an authenticator.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically, including the secret by content.
        /// UUID, secret, algorithm, digits and period are immutable. LastStep must never decrease or reset after acceptance.
        /// Accepting a code requires advancing LastStep strictly. Clear the challenge only on successful enrollment in the same transaction as consumption.
        /// </remarks>
        Task<bool> TryUpdateTotpAsync(User user, UserTotp expected, UserTotp totp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an authenticator.
        /// </summary>
        /// <remarks>
        /// Remove its pending challenge atomically.
        /// </remarks>
        Task<bool> TryRemoveTotpAsync(User user, UserTotp totp, CancellationToken cancellationToken = default);

    }

}
