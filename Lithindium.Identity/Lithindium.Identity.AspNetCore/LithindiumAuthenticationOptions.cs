// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication;

namespace Lithindium.Identity.AspNetCore {

    /// <summary>
    /// Lithindium cookie authentication options.
    /// </summary>
    public sealed class LithindiumAuthenticationOptions : AuthenticationSchemeOptions {

        /// <summary>
        /// Cookie name. Must use the __Host- prefix.
        /// </summary>
        public string CookieName { get; set; } = "__Host-Lithindium";

        /// <summary>
        /// Persists the cookie until session expiration rather than browser close.
        /// </summary>
        public bool PersistentCookie { get; set; } = true;

    }

}
