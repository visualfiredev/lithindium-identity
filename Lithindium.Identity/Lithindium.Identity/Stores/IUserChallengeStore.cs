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
    /// Never reuse challenge UUIDs. A challenge belongs to one user and one intended operation.
    /// Failed proof attempts must persist without applying the operation; successful completion must apply the operation and consume its challenge atomically.
    /// </remarks>
    public interface IUserChallengeStore : IUserStore {

        /// <summary>
        /// Gets a user's challenge by UUID, or returns null.
        /// </summary>
        Task<UserChallenge?> GetChallengeAsync(User user, Guid uuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a challenge.
        /// </summary>
        /// <remarks>
        /// Require zero attempts, a nonempty verifier and expiration after creation.
        /// Bind it to exactly one operation of the same owner within the same transaction.
        /// </remarks>
        Task<StoreResult> AddChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Conditionally updates a challenge's attempt count.
        /// </summary>
        /// <remarks>
        /// Compare the entire expected state atomically. UUID, secret and timestamps are immutable.
        /// The replacement must increment attempts by exactly one without overflow.
        /// The caller supplies attempt-limit and expiration policy using current transactional state.
        /// </remarks>
        Task<StoreResult> TryUpdateChallengeAsync(User user, UserChallenge expected, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an unreferenced challenge.
        /// </summary>
        /// <remarks>
        /// Compare the expected challenge before removal. A referenced challenge must not be removed.
        /// Successful verification clears the reference and applies the operation in the same transaction.
        /// </remarks>
        Task<StoreResult> RemoveChallengeAsync(User user, UserChallenge challenge, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes challenges expiring at or before the UTC cutoff.
        /// </summary>
        /// <remarks>
        /// Atomically delete associated pending emails, phones, authenticators and sessions; clear only recovery references.
        /// Never clear a pending credential or session reference to expire it. Return the number of challenges removed.
        /// </remarks>
        Task<int> RemoveExpiredChallengesAsync(DateTime expiresAt, CancellationToken cancellationToken = default);

    }

}
