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
    /// User email persistence.
    /// </summary>
    /// <remarks>
    /// Email addresses are unique per owner, and verified addresses are unique across users.
    /// Pending emails are not credentials and do not reserve another user's verified lookup.
    /// </remarks>
    public interface IUserEmailStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified emails.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user's pending and verified emails.</returns>
        IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by verified email address.
        /// </summary>
        /// <param name="address">Verified email address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an email.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="email">New email.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Rejects pending primary emails. Adding a verified primary clears the previous primary in the same transaction.
        /// </remarks>
        /// <returns><see langword="true"/> if the email was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates an email.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected email.</param>
        /// <param name="email">Updated email.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares the entire expected state atomically. The address is immutable.
        /// A non-null challenge is replaced or cleared only in the transaction binding or consuming it.
        /// Only verified emails can be primary; setting a primary clears the previous primary in the same transaction.
        /// </remarks>
        /// <returns><see langword="true"/> if the email was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateEmailAsync(User user, UserEmail expected, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an email.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="email">Email.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// Removal of a verified recovery contact invalidates outstanding account recovery.
        /// </remarks>
        /// <returns><see langword="true"/> if the email was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

    }

}
