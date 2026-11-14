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
    /// Defines the persistence contract for challenges associated with pending user operations.
    /// </summary>
    /// <remarks>
    /// <para>Challenge identifiers are never reused. Each challenge belongs to one user and one intended operation.</para>
    /// <para>Failed proof attempts persist without applying the operation. Successful completion applies the operation and consumes the challenge in the same transaction.</para>
    /// </remarks>
    public interface IUserChallengeStore : IUserStore {

        /// <summary>
        /// Asynchronously gets a user's challenge by unique identifier.
        /// </summary>
        /// <param name="user">The user to whom the challenge belongs.</param>
        /// <param name="uuid">The unique identifier of the challenge to find.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the challenge, or <see langword="null"/> if not found.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add a challenge asynchronously.
        /// </summary>
        /// <remarks>
        /// Requires an initial attempt count of zero, a nonempty proof verifier, and an expiration time after creation. The challenge is bound to exactly one operation of the same user in the same transaction.
        /// </remarks>
        /// <param name="user">The user to whom the challenge belongs.</param>
        /// <param name="challenge">The challenge to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the challenge was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to increment the reserved attempt count of a challenge asynchronously.
        /// </summary>
        /// <remarks>
        /// <para>Compares the entire persisted challenge against <paramref name="expected"/> atomically. The identifier, secret, and timestamps are immutable. The replacement increments <see cref="UserChallenge.Attempts"/> by exactly one without overflow.</para>
        /// <para>The caller applies attempt-limit and expiration policies using current transactional state.</para>
        /// </remarks>
        /// <param name="user">The user to whom the challenge belongs.</param>
        /// <param name="expected">The expected persisted challenge.</param>
        /// <param name="challenge">The replacement challenge.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the challenge was updated; otherwise, <see langword="false"/> (for example, if the expected state does not match).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryUpdateChallengeAsync(User user, UserChallenge expected, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to remove an unreferenced challenge asynchronously.
        /// </summary>
        /// <remarks>
        /// Compares the persisted challenge against <paramref name="challenge"/> before removal and rejects challenges that are still referenced. Successful verification clears the reference and applies the challenged operation in the same transaction.
        /// </remarks>
        /// <param name="user">The user to whom the challenge belongs.</param>
        /// <param name="challenge">The expected persisted challenge to remove.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the challenge was removed; otherwise, <see langword="false"/> (for example, if the challenge is missing, has changed, is referenced).</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryRemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously removes challenges that expire at or before a specified time.
        /// </summary>
        /// <remarks>
        /// Deletes the associated pending email addresses, phone numbers, authenticators, and sessions in the same transaction. For account recovery, only the recovery challenge reference is cleared. Expiration never clears a pending credential or session reference.
        /// </remarks>
        /// <param name="expiresAt">The inclusive expiration cutoff, in UTC.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of challenges removed.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
