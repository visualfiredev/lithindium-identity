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
    /// Defines the persistence contract for identity audit records.
    /// </summary>
    public interface IUserAuditStore : IUserStore {

        /// <summary>
        /// Gets an asynchronous sequence of audit records for an affected user.
        /// </summary>
        /// <param name="user">The unique identifier of the affected user.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of the affected user's audit records.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserAudit> GetAuditsAsync(Guid user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an asynchronous sequence of audit records for an acting user.
        /// </summary>
        /// <param name="actor">The unique identifier of the user who performed the audited action.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of audit records for the acting user.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserAudit> GetAuditsByActorAsync(Guid actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an asynchronous sequence of audit records for an affected entity.
        /// </summary>
        /// <param name="entity">The unique identifier of the entity affected by the audited action.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of audit records for the affected entity.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        IAsyncEnumerable<UserAudit> GetAuditsByEntityAsync(Guid entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to add an audit record asynchronously.
        /// </summary>
        /// <remarks>
        /// Audit records are preserved after associated users or entities are deleted.
        /// </remarks>
        /// <param name="audit">The audit record to add.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the audit record was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.OperationCanceledException">The cancellation token was canceled.</exception>
        Task<bool> TryAddAuditAsync(UserAudit audit, CancellationToken cancellationToken = default);

    }

}
