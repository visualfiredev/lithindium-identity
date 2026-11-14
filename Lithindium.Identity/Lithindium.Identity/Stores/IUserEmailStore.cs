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
    /// Defines the persistence contract for pending and verified user email addresses.
    /// </summary>
    /// <remarks>
    /// Email addresses are unique per user, and verified addresses are unique across users. Pending addresses are not credentials and do not prevent another user from being found by a verified address.
    /// </remarks>
    public interface IUserEmailStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of a user's pending and verified email addresses.
        /// </summary>
        /// <param name="user">The user whose email addresses are retrieved.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the user's pending and verified email addresses.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserEmail> GetEmailsAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds a user by verified email address.
        /// </summary>
        /// <param name="address">The verified email address to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<User?> FindByEmailAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add an email address asynchronously.
        /// </summary>
        /// <remarks>
        /// Rejects a pending email address marked as primary. Adding a verified primary address clears the previous primary address in the same transaction.
        /// </remarks>
        /// <param name="user">The user to whom the email address belongs.</param>
        /// <param name="email">The email address to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email address was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to conditionally update an email address asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted email record against <paramref name="expected"/> atomically. The email address is immutable. A non-null challenge reference is replaced or cleared only in the transaction that binds or consumes it.</para>
        /// <para>Only verified email addresses can be primary. Setting a primary address clears the previous primary address in the same transaction.</para>
        /// </remarks>
        /// <param name="user">The user to whom the email address belongs.</param>
        /// <param name="expected">The expected persisted record.</param>
        /// <param name="email">The replacement record.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email address was updated; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateEmailAsync(User user, UserEmail expected, UserEmail email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove an email address asynchronously.
        /// </summary>
        /// <remarks>
        /// Removes the associated pending challenge in the same transaction, if present. Removing a verified recovery contact invalidates outstanding account recovery.
        /// </remarks>
        /// <param name="user">The user to whom the email address belongs.</param>
        /// <param name="email">The email address to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email address was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveEmailAsync(User user, UserEmail email, CancellationToken cancellationToken = default);

    }

}
