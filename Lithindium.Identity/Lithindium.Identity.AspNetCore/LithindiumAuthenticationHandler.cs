// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithindium.Identity.AspNetCore {

    /// <summary>
    /// Validates persisted sessions on every authenticated request.
    /// </summary>
    public sealed class LithindiumAuthenticationHandler(
        IOptionsMonitor<LithindiumAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, SignInManager signIn)
        : AuthenticationHandler<LithindiumAuthenticationOptions>(options, logger, encoder) {

        /// <inheritdoc />
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Cookies.TryGetValue(Options.CookieName, out string? token) || string.IsNullOrEmpty(token))
                return AuthenticateResult.NoResult();

            OperationResult<AuthenticatedSession> result = await signIn.ValidateSessionAsync(token, Context.RequestAborted).ConfigureAwait(false);
            if (!result.HasValue) return AuthenticateResult.Fail("Invalid or unavailable session.");

            AuthenticatedSession authenticated = result.Value;
            UserSession session = authenticated.Session;
            var identity = new ClaimsIdentity(Scheme.Name, ClaimTypes.NameIdentifier, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authenticated.UserUuid.ToString("D")));
            identity.AddClaim(new Claim("sub", authenticated.UserUuid.ToString("D")));
            identity.AddClaim(new Claim(LithindiumAuthenticationDefaults.SessionClaim, session.Uuid.ToString("D")));
            identity.AddClaim(new Claim("auth_time", new DateTimeOffset(session.AuthenticatedAt).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));
            foreach (string method in session.Amr) identity.AddClaim(new Claim("amr", method));
            if (session.Acr is { } acr) identity.AddClaim(new Claim("acr", acr));

            var properties = new AuthenticationProperties {
                IssuedUtc = new DateTimeOffset(session.AuthenticatedAt),
                ExpiresUtc = new DateTimeOffset(session.ExpiresAt),
                IsPersistent = Options.PersistentCookie,
                AllowRefresh = false
            };
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Scheme.Name));
        }

    }

}
