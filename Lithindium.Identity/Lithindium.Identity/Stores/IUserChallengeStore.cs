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
    /// User challenge persistence.
    /// </summary>
    /// <remarks>
    /// Challenge UUIDs are never reused. Each challenge belongs to one user and one intended operation.
    /// Failed proof attempts persist without applying the operation. Successful completion applies the operation and consumes the challenge in the same transaction.
    /// </remarks>
    public interface IUserChallengeStore : IUserStore {

        /// <summary>
        /// Gets a user's challenge by UUID.
        /// </summary>
        /// <param name="user">Owning user.</param>
        /// <param name="uuid">Challenge UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The challenge, or <see langword="null"/> if not found.</returns>
        Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a challenge.
        /// </summary>
        /// <remarks>
        /// Requires zero attempts, a nonempty verifier and expiration after creation.
        /// The challenge is bound to exactly one operation of the same owner within the same transaction.
        /// </remarks>
        /// <param name="user">Owning user.</param>
        /// <param name="challenge">New challenge.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see langword="true"/> if the challenge was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a challenge's attempt count.
        /// </summary>
        /// <remarks>
        /// Compares the entire expected state atomically. The UUID, secret and timestamps are immutable.
        /// The replacement increments attempts by exactly one without overflow.
        /// The caller supplies attempt-limit and expiration policy using current transactional state.
        /// </remarks>
        /// <param name="user">Owning user.</param>
        /// <param name="expected">Expected challenge.</param>
        /// <param name="challenge">Updated challenge.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see langword="true"/> if the challenge was updated, otherwise <see langword="false"/>.</returns>
        Task<bool> TryUpdateChallengeAsync(User user, UserChallenge expected, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an unreferenced challenge.
        /// </summary>
        /// <remarks>
        /// Compares the expected challenge before removal and rejects referenced challenges.
        /// Successful verification clears the reference and applies the operation in the same transaction.
        /// </remarks>
        /// <param name="user">Owning user.</param>
        /// <param name="challenge">Expected challenge.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see langword="true"/> if the challenge was removed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryRemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes challenges expiring at or before the UTC cutoff.
        /// </summary>
        /// <remarks>
        /// Deletes associated pending emails, phones, authenticators and sessions in the same transaction, but only clears recovery references.
        /// Expiration never clears a pending credential or session reference.
        /// </remarks>
        /// <param name="expiresAt">Inclusive UTC expiration cutoff.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of challenges removed.</returns>
        Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
