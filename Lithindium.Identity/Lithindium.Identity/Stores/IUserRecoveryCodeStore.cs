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
    /// Recovery code persistence.
    /// </summary>
    public interface IUserRecoveryCodeStore : IUserStore {

        /// <summary>
        /// Gets a user's unused recovery codes.
        /// </summary>
        /// <returns>The user's unused recovery codes.</returns>
        IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces all of a user's recovery codes.
        /// </summary>
        /// <remarks>
        /// Replaces the set atomically. An empty set removes all codes. UUIDs and code hashes are unique as applicable.
        /// </remarks>
        /// <returns><see langword="true"/> if the recovery codes were replaced, otherwise <see langword="false"/>.</returns>
        Task<bool> TryReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a recovery code.
        /// </summary>
        /// <remarks>
        /// Finds and deletes the matching code atomically by hash content, scoped to the owner.
        /// </remarks>
        /// <returns><see langword="true"/> if the recovery code was consumed, otherwise <see langword="false"/>.</returns>
        Task<bool> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts a user's unused recovery codes.
        /// </summary>
        /// <returns>The number of unused recovery codes.</returns>
        Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

    }

}
