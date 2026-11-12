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
    /// Authentication and verification challenge persistence.
    /// </summary>
    /// <remarks>
    /// Never reuse challenge UUIDs. Consumption or revocation permanently deletes challenges.
    /// Remove all outstanding user challenges atomically when authentication generation changes.
    /// </remarks>
    public interface IUserChallengeStore : IUserStore {

        /// <summary>
        /// Gets a user's challenge by UUID.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="uuid">Challenge UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Challenge, or <see langword="null"/> if absent.</returns>
        /// <remarks>
        /// Retrieval alone does not establish validity.
        /// </remarks>
        Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a challenge.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="challenge">Challenge.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreAddChallenge"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Identifier"/>.
        /// </returns>
        /// <remarks>
        /// Require zero attempts, a positive attempt limit and expiration after creation.
        /// Issue within the transaction validating the authorizing user state.
        /// </remarks>
        Task<OperationResult> AddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to reserve a verification attempt.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="challenge">Challenge.</param>
        /// <param name="now">Current UTC time.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreReserveChallengeAttempt"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// <see cref="StatusCode.Expired"/> / <see cref="ContextCode.Challenge"/>.
        /// <see cref="StatusCode.AttemptsExhausted"/> / <see cref="ContextCode.Challenge"/>.
        /// </returns>
        /// <remarks>
        /// Atomically validate existence, expiration and remaining attempts, then increment
        /// the persisted count, not the supplied snapshot. Commit failed verification attempts
        /// without applying the challenged operation. Rolled-back reservations do not count;
        /// retry with a new reservation.
        /// </remarks>
        Task<OperationResult> TryReserveChallengeAttemptAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a verified challenge.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="challenge">Challenge.</param>
        /// <param name="now">Current UTC time.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreConsumeChallenge"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// <see cref="StatusCode.Expired"/> / <see cref="ContextCode.Challenge"/>.
        /// </returns>
        /// <remarks>
        /// Atomically validate existence and expiration, then delete. Reserve and verify successfully in
        /// the same transaction before consuming, including on the final attempt.
        /// Commit consumption and the resulting identity mutation together. Use consumption time,
        /// not reservation time.
        /// </remarks>
        Task<OperationResult> TryConsumeChallengeAsync(User user, UserChallenge challenge, DateTime now, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a challenge.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="challenge">Challenge.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveChallenge"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        Task<OperationResult> RemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user's challenges.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="type">Challenge type, or <see langword="null"/> for all.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreRemoveChallenges"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// </returns>
        /// <remarks>
        /// Remove matching challenges atomically; no matches is success.
        /// </remarks>
        Task<OperationResult> RemoveChallengesAsync(User user, UserChallengeType? type = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes expired challenges.
        /// </summary>
        /// <param name="expiresAt">Inclusive UTC expiration cutoff.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of challenges removed.</returns>
        Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
