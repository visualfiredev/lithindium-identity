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
    /// Pending records are not credentials. The email address must be unique per owner;
    /// verified emails must also be unique across users. Pending records must not reserve another user's verified lookup.
    /// </remarks>
    public interface IUserEmailStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified emails.
        /// </summary>
        IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by verified email address, or returns null.
        /// </summary>
        /// <remarks>
        /// Pending records are never returned by this lookup.
        /// </remarks>
        Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a email.
        /// </summary>
        /// <remarks>
        /// A pending email must not be primary. A verified primary atomically replaces the previous primary.
        /// </remarks>
        Task<StoreResult> AddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a email.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically; address is immutable.
        /// A non-null challenge may be replaced or cleared only in the transaction binding or consuming it.
        /// Only verified records may be primary; setting a primary atomically clears the previous primary.
        /// </remarks>
        Task<StoreResult> TryUpdateEmailAsync(User user, UserEmail expected, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a email.
        /// </summary>
        /// <remarks>
        /// Remove its pending challenge atomically. Removing a verified recovery contact must invalidate outstanding account recovery.
        /// </remarks>
        Task<StoreResult> RemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

    }

}
