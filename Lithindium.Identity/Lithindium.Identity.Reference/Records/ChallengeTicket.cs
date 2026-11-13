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
    /// Newly issued challenge and its delivery secret or enrollment key.
    /// </summary>
    /// <remarks>
    /// Sensitive; return only to the authorized recipient and never log or persist the secret.
    /// </remarks>
    public sealed class ChallengeTicket {

        /// <summary>
        /// Challenge UUID.
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// Challenge purpose.
        /// </summary>
        public UserChallengeType Type { get; }

        /// <summary>
        /// Delivery proof, MFA continuation proof, or Base32 enrollment key.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// UTC expiration.
        /// </summary>
        public DateTime ExpiresAt { get; }

        internal ChallengeTicket(Guid uuid, UserChallengeType type, string secret, DateTime expiresAt) {
            Uuid = uuid;
            Type = type;
            Secret = secret;
            ExpiresAt = expiresAt;
        }

    }

}
