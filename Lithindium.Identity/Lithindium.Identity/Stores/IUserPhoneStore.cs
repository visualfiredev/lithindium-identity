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
    /// User phone persistence.
    /// </summary>
    /// <remarks>
    /// Phone number and extension pairs are unique per owner, and verified pairs are unique across users.
    /// Pending phones are not credentials and do not reserve another user's verified lookup.
    /// </remarks>
    public interface IUserPhoneStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified phones.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user's pending and verified phones.</returns>
        IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by verified phone number and extension.
        /// </summary>
        /// <param name="number">Verified phone number.</param>
        /// <param name="extension">Phone extension, or <see langword="null"/> if absent.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user, or <see langword="null"/> if not found.</returns>
        Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a phone.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="phone">New phone.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Rejects pending primary phones. Adding a verified primary clears the previous primary in the same transaction.
        /// </remarks>
        /// <returns><see langword="true"/> if the phone was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a phone.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected phone.</param>
        /// <param name="phone">Updated phone.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Compares the entire expected state atomically. The number and extension are immutable.
        /// A non-null challenge is replaced or cleared only in the transaction binding or consuming it.
        /// Only verified phones can be primary; setting a primary clears the previous primary in the same transaction.
        /// </remarks>
        /// <returns><see langword="true"/> if the phone was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdatePhoneAsync(User user, UserPhone expected, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a phone.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="phone">Phone.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present.
        /// Removal of a verified recovery contact invalidates outstanding account recovery.
        /// </remarks>
        /// <returns><see langword="true"/> if the phone was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

    }

}
