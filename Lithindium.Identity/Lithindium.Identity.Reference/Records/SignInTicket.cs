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
    /// Either a completed sign-in or a pending MFA challenge.
    /// </summary>
    /// <remarks>
    /// Contains sensitive bearer credentials. Never log or persist the ticket.
    /// A successful operation can return pending MFA instead of authentication.
    /// </remarks>
    public sealed class SignInTicket {

        /// <summary>
        /// User UUID.
        /// </summary>
        public Guid UserUuid { get; }

        /// <summary>
        /// Authenticated session, if sign-in is complete.
        /// </summary>
        public AuthenticatedSession? Session { get; }

        /// <summary>
        /// Protected bearer token, if sign-in is complete.
        /// </summary>
        public string? SessionToken { get; }

        /// <summary>
        /// Pending MFA challenge, if another factor is required.
        /// </summary>
        public ChallengeTicket? Challenge { get; }

        /// <summary>
        /// Whether full authentication is complete.
        /// </summary>
        public bool IsAuthenticated => Session.HasValue;

        internal SignInTicket(AuthenticatedSession session, string token) {
            UserUuid = session.UserUuid;
            Session = session;
            SessionToken = token;
        }

        internal SignInTicket(Guid userUuid, ChallengeTicket challenge) {
            UserUuid = userUuid;
            Challenge = challenge;
        }

    }

}
