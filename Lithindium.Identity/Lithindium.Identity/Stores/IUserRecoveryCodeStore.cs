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
        IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces all of a user's recovery codes.
        /// </summary>
        /// <remarks>
        /// Replace the set atomically; an empty set removes all codes. UUIDs and code hashes must be unique as applicable.
        /// </remarks>
        Task<bool> TryReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a recovery code.
        /// </summary>
        /// <remarks>
        /// Atomically find and delete the matching code by hash content. Missing or differently owned codes return false.
        /// </remarks>
        Task<bool> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts a user's unused recovery codes.
        /// </summary>
        Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

    }

}
