// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Provides persistence operations for user recovery codes.
    /// </summary>
    public interface IUserRecoveryCodeStore : IUserStore {

        /// <summary>
        /// Retrieves the unused recovery codes associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose recovery codes to retrieve.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The unused recovery codes associated with the user.</returns>
        IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces all existing recovery codes associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose recovery codes to replace.</param>
        /// <param name="codes">The new recovery codes.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>,
        /// <see cref="UserStoreError.DuplicateRecoveryCode"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// Existing recovery codes must be removed and the new recovery codes
        /// must be added atomically. An empty collection removes all existing codes.
        /// </remarks>
        Task<UserStoreError> ReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a recovery code associated with the specified user.
        /// </summary>
        /// <param name="user">The user associated with the recovery code.</param>
        /// <param name="codeHash">The cryptographic hash of the recovery code to consume.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The lookup and removal of the recovery code must be performed atomically
        /// to prevent the same recovery code from being consumed more than once.
        /// A missing, previously consumed, or differently owned code is reported as <see cref="UserStoreError.EntityNotFound"/>.
        /// </remarks>
        Task<UserStoreError> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the number of unused recovery codes associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose recovery codes to count.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The number of unused recovery codes associated with the user.</returns>
        Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

    }

}
