// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Provides persistence operations for user authentication and verification challenges.
    /// </summary>
    /// <remarks>
    /// Challenge identifiers uniquely identify pending authentication
    /// or verification operations and must never be reused.
    /// Consuming or revoking a challenge permanently removes it from the store.
    /// All outstanding challenges associated with a user must be removed
    /// atomically whenever the user's authentication generation changes.
    /// </remarks>
    public interface IUserChallengeStore : IUserStore {

        /// <summary>
        /// Retrieves the specified challenge associated with a user.
        /// </summary>
        /// <param name="user">The user associated with the challenge.</param>
        /// <param name="uuid">The universally unique identifier of the challenge.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The challenge, or <see langword="null"/> if no matching challenge exists.</returns>
        /// <remarks>
        /// Retrieving a challenge does not establish its validity.
        /// </remarks>
        Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.DuplicateIdentifier"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The challenge must have no previous attempts, a positive maximum
        /// attempt count, and an expiration later than its creation.
        /// Challenge identifiers must never be reused.
        /// Challenge issuance must occur within the same transaction
        /// as the validation of the user state authorizing its creation.
        /// </remarks>
        Task<UserStoreError> AddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ChallengeExpired"/>,
        /// <see cref="UserStoreError.ChallengeAttemptsExhausted"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The challenge must exist, remain unexpired,
        /// and have remaining verification attempts.
        /// Validation and incrementing the attempt count must be performed atomically.
        /// Unsuccessful verification attempts must remain counted.
        /// </remarks>
        Task<UserStoreError> TryReserveChallengeAttemptAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default);

        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.UserNotFound"/>,
        /// <see cref="UserStoreError.EntityNotFound"/>,
        /// <see cref="UserStoreError.ChallengeExpired"/>, as applicable.
        /// </returns>
        /// <remarks>
        /// The challenge must exist and remain unexpired.
        /// Validation and removal must be performed atomically.
        /// This operation requires a successful attempt reservation within the same
        /// transaction and must only be invoked after successful verification.
        /// The final permitted attempt may still consume the challenge.
        /// </remarks>
        Task<UserStoreError> TryConsumeChallengeAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an authentication or verification challenge from the specified user.
        /// </summary>
        /// <param name="user">The user associated with the challenge.</param>
        /// <param name="challenge">The challenge to remove.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The outcome of the persistence operation.</returns>
        Task<UserStoreError> RemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the authentication and verification challenges associated with the specified user.
        /// </summary>
        /// <param name="user">The user whose challenges to remove.</param>
        /// <param name="type">The type of challenge to remove,
        /// or <see langword="null"/> to remove all challenges.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The outcome of the persistence operation.</returns>
        /// <remarks>
        /// The removal of matching challenges must be performed atomically.
        /// If no challenges match, the operation succeeds without removing anything.
        /// </remarks>
        Task<UserStoreError> RemoveChallengesAsync(User user, UserChallengeType? type = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes expired authentication and verification challenges from the identity store.
        /// </summary>
        /// <param name="expiresAt">The UTC date and time at or before which challenges must have expired.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The number of challenges removed.</returns>
        Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
