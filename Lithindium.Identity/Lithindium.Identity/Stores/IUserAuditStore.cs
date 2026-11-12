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
    /// Provides persistence operations for user audit records.
    /// </summary>
    public interface IUserAuditStore : IUserStore {

        /// <summary>
        /// Retrieves the audit records associated with the specified user.
        /// </summary>
        /// <param name="user">The universally unique identifier of the affected user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The audit records associated with the user.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the audit records associated with actions performed by the specified user.
        /// </summary>
        /// <param name="actor">The universally unique identifier of the acting user.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The audit records associated with the actor.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the audit records associated with the specified entity.
        /// </summary>
        /// <param name="entity">The universally unique identifier of the affected entity.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <returns>The audit records associated with the entity.</returns>
        IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an audit record to the identity store.
        /// </summary>
        /// <param name="audit">The audit record to add.</param>
        /// <param name="cancellationToken">The token used to cancel the operation.</param>
        /// <remarks>
        /// Audit records must be preserved independently of the existence
        /// of their associated users and entities.
        /// </remarks>
        Task AddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default);

    }

}
