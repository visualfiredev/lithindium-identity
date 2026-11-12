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
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Unused recovery codes.</returns>
        IAsyncEnumerable<UserRecoveryCode> GetRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces all of a user's recovery codes.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="codes">Replacement codes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreReplaceRecoveryCodes"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.Identifier"/>.
        /// <see cref="StatusCode.AlreadyExists"/> / <see cref="ContextCode.RecoveryCode"/>.
        /// </returns>
        /// <remarks>
        /// Replace the set atomically; an empty set removes all codes.
        /// </remarks>
        Task<OperationResult> ReplaceRecoveryCodesAsync(User user, IReadOnlyCollection<UserRecoveryCode> codes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to consume a recovery code.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="codeHash">Cryptographic code hash.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="OperationCode.StoreConsumeRecoveryCode"/>:
        /// <see cref="StatusCode.Success"/> on success.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.User"/>.
        /// <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </returns>
        /// <remarks>
        /// Atomically find and delete the code to prevent reuse. Missing, consumed or
        /// differently owned codes return <see cref="StatusCode.NotFound"/> / <see cref="ContextCode.Entity"/>.
        /// </remarks>
        Task<OperationResult> TryConsumeRecoveryCodeAsync(User user, ImmutableArray<byte> codeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts a user's unused recovery codes.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of unused recovery codes.</returns>
        Task<int> CountRecoveryCodesAsync(User user, CancellationToken cancellationToken = default);

    }

}
