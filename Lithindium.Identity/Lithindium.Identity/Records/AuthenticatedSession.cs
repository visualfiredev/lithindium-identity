// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// A validated session and its owner.
    /// </summary>
    public readonly record struct AuthenticatedSession {

        /// <summary>
        /// User UUID.
        /// </summary>
        public Guid UserUuid { get; }

        /// <summary>
        /// Persisted session.
        /// </summary>
        public UserSession Session { get; }

        internal AuthenticatedSession(Guid userUuid, UserSession session) {
            UserUuid = userUuid;
            Session = session;
        }

    }

}
