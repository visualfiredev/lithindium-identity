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
        /// Gets audits by affected user UUID.
        /// </summary>
        /// <param name="user">Affected user UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The affected user's audit records.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by acting user UUID.
        /// </summary>
        /// <param name="actor">Acting user UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audit records for the acting user.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by affected entity UUID.
        /// </summary>
        /// <param name="entity">Affected entity UUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audit records for the affected entity.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an audit record.
        /// </summary>
        /// <remarks>
        /// Preserves audits after associated users or entities are deleted.
        /// </remarks>
        /// <param name="audit">Audit record.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see langword="true"/> if the audit record was added, otherwise <see langword="false"/>.</returns>
        Task<bool> TryAddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default);

    }

}
