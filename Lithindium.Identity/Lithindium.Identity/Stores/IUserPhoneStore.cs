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
    /// Defines the persistence contract for pending and verified user phone numbers.
    /// </summary>
    /// <remarks>
    /// Phone number and extension pairs are unique per user, and verified pairs are unique across users. Pending numbers are not credentials and do not prevent another user from being found by a verified number.
    /// </remarks>
    public interface IUserPhoneStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's pending and verified phone numbers.
        /// </summary>
        /// <param name="user">The user whose phone numbers are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's pending and verified phone numbers.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds a user by verified phone number and extension.
        /// </summary>
        /// <param name="number">The verified phone number to find.</param>
        /// <param name="extension">The extension of the phone number, or <see langword="null"/> if the number has no extension.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add a phone number asynchronously.
        /// </summary>
        /// <remarks>
        /// Rejects a pending phone number marked as primary. Adding a verified primary number atomically clears the previous primary number.
        /// </remarks>
        /// <param name="user">The user to whom the phone number belongs.</param>
        /// <param name="phone">The phone number to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the phone number was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to conditionally update a phone number asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted phone record against <paramref name="expected"/> atomically. The number and extension are immutable. The caller binds or consumes the associated challenge in the same transaction when replacing or clearing its reference.</para>
        /// <para>Only verified phone numbers can be primary. Setting a primary number atomically clears the previous primary number.</para>
        /// </remarks>
        /// <param name="user">The user to whom the phone number belongs.</param>
        /// <param name="expected">The expected persisted record.</param>
        /// <param name="phone">The replacement record.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the phone number was updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdatePhoneAsync(User user, UserPhone expected, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove a phone number asynchronously.
        /// </summary>
        /// <remarks>
        /// Atomically removes the associated pending challenge, if present. Removing a verified recovery contact invalidates outstanding account recovery.
        /// </remarks>
        /// <param name="user">The user to whom the phone number belongs.</param>
        /// <param name="phone">The phone number to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the phone number was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

    }

}
