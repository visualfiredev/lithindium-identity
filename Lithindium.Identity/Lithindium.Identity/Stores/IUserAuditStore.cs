// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Audit record persistence.
    /// </summary>
    public interface IUserAuditStore : IUserStore {

        /// <summary>
        /// Gets audits by affected user.
        /// </summary>
        /// <param name="user">Affected user UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audit records.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by actor.
        /// </summary>
        /// <param name="actor">Acting user UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audit records.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by affected entity.
        /// </summary>
        /// <param name="entity">Affected entity UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audit records.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an audit record.
        /// </summary>
        /// <param name="audit">Audit record.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <see cref="UserStoreError.None"/> on success;
        /// <see cref="UserStoreError.DuplicateIdentifier"/>.
        /// </returns>
        /// <remarks>
        /// Preserve audits even after associated users or entities are deleted.
        /// </remarks>
        Task<UserStoreError> AddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default);

    }

}
