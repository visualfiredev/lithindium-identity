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
        IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by acting user UUID.
        /// </summary>
        IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets audits by affected entity UUID.
        /// </summary>
        IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an audit record.
        /// </summary>
        /// <remarks>
        /// Preserve audits after associated users or entities are deleted.
        /// </remarks>
        Task<StoreResult> AddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default);

    }

}
