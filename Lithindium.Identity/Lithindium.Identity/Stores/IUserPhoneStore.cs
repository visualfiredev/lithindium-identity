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
    /// Pending records are not credentials. The phone number and extension must be unique per owner;
    /// verified phones must also be unique across users. Pending records must not reserve another user's verified lookup.
    /// </remarks>
    public interface IUserPhoneStore : IUserStore {

        /// <summary>
        /// Gets a user's pending and verified phones.
        /// </summary>
        IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by verified phone number and extension, or returns null.
        /// </summary>
        /// <remarks>
        /// Pending records are never returned by this lookup.
        /// </remarks>
        Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a phone.
        /// </summary>
        /// <remarks>
        /// A pending phone must not be primary. A verified primary atomically replaces the previous primary.
        /// </remarks>
        Task<bool> TryAddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a phone.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically; number and extension is immutable.
        /// A non-null challenge may be replaced or cleared only in the transaction binding or consuming it.
        /// Only verified records may be primary; setting a primary atomically clears the previous primary.
        /// </remarks>
        Task<bool> TryUpdatePhoneAsync(User user, UserPhone expected, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a phone.
        /// </summary>
        /// <remarks>
        /// Remove its pending challenge atomically. Removing a verified recovery contact must invalidate outstanding account recovery.
        /// </remarks>
        Task<bool> TryRemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

    }

}
