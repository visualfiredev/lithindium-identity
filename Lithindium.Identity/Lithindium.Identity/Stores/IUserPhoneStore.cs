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
    /// Verified phone persistence.
    /// </summary>
    public interface IUserPhoneStore : IUserStore {

        /// <summary>
        /// Gets a user's verified phones.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Phone numbers.</returns>
        IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user by phone number and extension.
        /// </summary>
        /// <param name="number">Verified E.164 phone number.</param>
        /// <param name="extension">Extension, or <see langword="null"/> if unset.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User, or <see langword="null"/> if absent.</returns>
        Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a verified phone.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="phone">Phone number.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreAddPhone"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Phone"/>.
        /// </returns>
        /// <remarks>
        /// Adding a primary phone must atomically clear the previous primary.
        /// </remarks>
        Task<OperationResult> AddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a verified phone.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="phone">Phone number.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemovePhone"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        Task<OperationResult> RemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the primary phone.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="phone">New primary, or <see langword="null"/> to clear.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreSetPrimaryPhone"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        /// <remarks>
        /// Set the primary atomically. Clearing an absent primary succeeds; a non-null phone
        /// must belong to the user.
        /// </remarks>
        Task<OperationResult> SetPrimaryPhoneAsync(User user, UserPhone? phone, CancellationToken cancellationToken = default);

    }

}
