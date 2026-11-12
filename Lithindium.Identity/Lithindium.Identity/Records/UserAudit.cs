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
    /// Represents an immutable record of an auditable event within the identity system.
    /// </summary>
    public readonly record struct UserAudit {

        /// <summary>
        /// Gets the universally unique identifier of the audit record.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the universally unique identifier of the user affected by the event.
        /// </summary>
        public required Guid User { get; init; }

        /// <summary>
        /// Gets the universally unique identifier of the user responsible for the event,
        /// or <see langword="null"/> if no user is associated with the action.
        /// </summary>
        public required Guid? Actor { get; init; }

        /// <summary>
        /// Gets the universally unique identifier of the entity affected by the event,
        /// or <see langword="null"/> if no entity identifier is applicable.
        /// </summary>
        public required Guid? Entity { get; init; }

        /// <summary>
        /// Gets the type of entity associated with the event.
        /// </summary>
        public required UserAuditType Type { get; init; }

        /// <summary>
        /// Gets the action performed during the event.
        /// </summary>
        public required UserAuditAction Action { get; init; }

        /// <summary>
        /// Gets additional information associated with the event as a JSON object.
        /// </summary>
        /// <remarks>
        /// Must not contain credentials or other sensitive authentication material.
        /// </remarks>
        public required string Info { get; init; }

        /// <summary>
        /// Gets the UTC date and time at which the event occurred.
        /// </summary>
        public required DateTime Date { get; init; }

        /// <summary>
        /// Constructs a new <see cref="UserAudit"/>
        /// with the specified parameters.
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
        /// Constructs a new <see cref="UserAudit"/>.
        /// </summary>
        public UserAudit() {
            // ...
        }

    }

}
