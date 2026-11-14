// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents an immutable record of an audited identity operation.
    /// </summary>
    public readonly record struct UserAudit {

        /// <summary>
        /// Gets the unique identifier of the audit record.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the unique identifier of the affected user.
        /// </summary>
        public required Guid User { get; init; }

        /// <summary>
        /// Gets the unique identifier of the user who performed the action.
        /// </summary>
        /// <value>The acting user's UUID, or <see langword="null"/> if there is no acting user.</value>
        public required Guid? Actor { get; init; }

        /// <summary>
        /// Gets the unique identifier of the affected entity.
        /// </summary>
        /// <value>The affected entity's UUID, or <see langword="null"/> if there is no affected entity.</value>
        public required Guid? Entity { get; init; }

        /// <summary>
        /// Gets the type of entity affected by the action.
        /// </summary>
        public required UserAuditType Type { get; init; }

        /// <summary>
        /// Gets the action performed on the entity.
        /// </summary>
        public required UserAuditAction Action { get; init; }

        /// <summary>
        /// Gets the event metadata as a JSON object.
        /// </summary>
        /// <remarks>
        /// The metadata excludes credentials and sensitive authentication material.
        /// </remarks>
        public required string Info { get; init; }

        /// <summary>
        /// Gets the date and time at which the action occurred, in UTC.
        /// </summary>
        public required DateTime Date { get; init; }

        /// <summary>
        /// Creates a <see cref="UserAudit"/> with the given values.
        /// </summary>
        [SetsRequiredMembers]
        public UserAudit(Guid uuid, Guid user, Guid? actor, Guid? entity, UserAuditType type, UserAuditAction action, string info, DateTime date) {
            Uuid = uuid;
            User = user;
            Actor = actor;
            Entity = entity;
            Type = type;
            Action = action;
            Info = info;
            Date = date;
        }

        /// <summary>
        /// Creates a <see cref="UserAudit"/>.
        /// </summary>
        public UserAudit() {
            // ...
        }

    }

}
