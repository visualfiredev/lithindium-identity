// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Lithindium.Identity {

    /// <summary>
    /// An authentication or verification challenge purpose.
    /// </summary>
    public enum UserChallengeType {

        /// <summary>
        /// Verify email ownership.
        /// </summary>
        EmailVerification,

        /// <summary>
        /// Verify phone ownership.
        /// </summary>
        PhoneVerification,

        /// <summary>
        /// Authorize a password reset.
        /// </summary>
        PasswordReset,

        /// <summary>
        /// Verify and enroll a TOTP authenticator.
        /// </summary>
        TotpEnrollment,

        /// <summary>
        /// Complete a partially authenticated sign-in with MFA.
        /// </summary>
        MultiFactorAuthentication

    }

}
