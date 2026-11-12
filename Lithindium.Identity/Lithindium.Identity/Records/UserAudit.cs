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
    /// Immutable audit record.
    /// </summary>
    public readonly record struct UserAudit {

        /// <summary>
        /// Gets the audit UUID.
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Gets the affected user UUID.
        /// </summary>
        public required Guid User { get; init; }

        /// <summary>
        /// Gets the acting user UUID, or <see langword="null"/> if none.
        /// </summary>
        public required Guid? Actor { get; init; }

        /// <summary>
        /// Gets the affected entity UUID, or <see langword="null"/> if none.
        /// </summary>
        public required Guid? Entity { get; init; }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public required UserAuditType Type { get; init; }

        /// <summary>
        /// Gets the action.
        /// </summary>
        public required UserAuditAction Action { get; init; }

        /// <summary>
        /// Gets event metadata as a JSON object.
        /// </summary>
        /// <remarks>
        /// Never include credentials or sensitive authentication material.
        /// </remarks>
        public required string Info { get; init; }

        /// <summary>
        /// Gets the UTC event time.
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
