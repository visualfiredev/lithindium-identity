// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// Represents the purpose of an authentication or verification challenge.
    /// </summary>
    public enum UserChallengeType {

        /// <summary>
        /// Indicates a challenge to verify ownership of an email address.
        /// </summary>
        EmailVerification,

        /// <summary>
        /// Indicates a challenge to verify ownership of a phone number.
        /// </summary>
        PhoneVerification,

        /// <summary>
        /// Indicates a challenge to authorize the resetting of a user's password.
        /// </summary>
        PasswordReset,

        /// <summary>
        /// Indicates a challenge to verify and enroll a TOTP authenticator.
        /// </summary>
        TotpEnrollment,

        /// <summary>
        /// Indicates a challenge to complete a partially authenticated sign-in
        /// through multifactor authentication.
        /// </summary>
        MultiFactorAuthentication

    }

}
