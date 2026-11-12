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
    /// Provides persistence operations for verified user phone numbers.
    /// </summary>
    public interface IUserPhoneStore : IUserStore {

        /// <summary>
        /// Retrieves the verified phone numbers associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose phone numbers to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The phone numbers associated with the user.</returns>
        IAsyncEnumerable<UserPhone> GetPhonesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the user associated with the specified phone number and extension.
        /// </summary>
        /// <param name="number">The verified phone number of the user in E.164 format.</param>
        /// <param name="extension">The phone number extension, or <see langword="null"/> if no extension is configured.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The user, or <see langword="null"/> if no matching user exists.</returns>
        Task<User?> FindByPhoneAsync(string number, string? extension = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a verified phone number to the specified user.
        /// </summary>
        /// <param name="user">The user to associate with the phone number.</param>
        /// <param name="phone">The phone number to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// If the phone number is designated as primary, any existing primary
        /// designation must be removed atomically.
        /// </remarks>
        Task AddPhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a verified phone number from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the phone number.</param>
        /// <param name="phone">The phone number to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        Task RemovePhoneAsync(User user, UserPhone phone, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the primary phone number of the specified user.
        /// </summary>
        /// <param name="user">The user whose primary phone number to set.</param>
        /// <param name="phone">The phone number to designate as primary, or <see langword="null"/> to clear the designation.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// Changes to the primary phone number must be performed atomically.
        /// </remarks>
        Task SetPrimaryPhoneAsync(User user, UserPhone? phone, CancellationToken cancellationToken = default);

    }

}
