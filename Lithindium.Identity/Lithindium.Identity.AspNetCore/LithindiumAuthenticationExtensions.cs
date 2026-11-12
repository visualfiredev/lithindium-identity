// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lithindium.Identity.AspNetCore {

    /// <summary>
    /// HTTP session authentication registration and cookie operations.
    /// </summary>
    public static class LithindiumAuthenticationExtensions {

        /// <summary>
        /// Registers the authentication scheme and data protection. Configure a shared
        /// key ring and application name for multi-instance deployments.
        /// </summary>
        public static AuthenticationBuilder AddLithindiumAuthentication(this IServiceCollection services, Action<LithindiumAuthenticationOptions>? configure = null) {
            services.AddLithindiumIdentity();
            services.AddDataProtection();
            services.AddOptions<LithindiumAuthenticationOptions>(LithindiumAuthenticationDefaults.Scheme)
                .Validate(options => ValidCookieName(options.CookieName), "Cookie name must use the __Host- prefix.")
                .ValidateOnStart();
            return services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme ??= LithindiumAuthenticationDefaults.Scheme;
                options.DefaultChallengeScheme ??= LithindiumAuthenticationDefaults.Scheme;
            }).AddScheme<LithindiumAuthenticationOptions, LithindiumAuthenticationHandler>(
                LithindiumAuthenticationDefaults.Scheme, configure ?? (_ => { }));
        }

        /// <summary>
        /// Writes the secure cookie for a completed sign-in.
        /// </summary>
        /// <remarks>
        /// Use HTTPS and apply antiforgery protection to cookie-authenticated mutations.
        /// </remarks>
        public static void SetLithindiumSession(this HttpContext context, SignInTicket ticket) {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(ticket);
            if (!context.Request.IsHttps) throw new InvalidOperationException("Lithindium cookies require HTTPS.");
            if (!ticket.IsAuthenticated || ticket.Session is not { } authenticated || ticket.SessionToken is not { } token)
                throw new InvalidOperationException("Full authentication is required.");

            LithindiumAuthenticationOptions options = GetOptions(context);
            CookieOptions cookie = CreateCookieOptions();
            if (options.PersistentCookie) cookie.Expires = new DateTimeOffset(authenticated.Session.ExpiresAt);
            context.Response.Cookies.Append(options.CookieName, token, cookie);
        }

        /// <summary>
        /// Revokes the current session and removes its cookie.
        /// </summary>
        public static async Task<OperationResult> SignOutLithindiumAsync(this HttpContext context) {
            ArgumentNullException.ThrowIfNull(context);
            if (!context.Request.IsHttps) throw new InvalidOperationException("Lithindium cookies require HTTPS.");
            LithindiumAuthenticationOptions options = GetOptions(context);
            if (!context.Request.Cookies.TryGetValue(options.CookieName, out string? token) || string.IsNullOrEmpty(token)) {
                context.Response.Cookies.Delete(options.CookieName, CreateCookieOptions());
                return new OperationResult(StatusCode.NoOp, operation: OperationCode.SignInSignOut);
            }

            SignInManager manager = context.RequestServices.GetRequiredService<SignInManager>();
            OperationResult result = await manager.SignOutAsync(token, context.RequestAborted).ConfigureAwait(false);
            if (result.IsSuccess || result.Status == StatusCode.InvalidCredential)
                context.Response.Cookies.Delete(options.CookieName, CreateCookieOptions());
            return result;
        }

        private static LithindiumAuthenticationOptions GetOptions(HttpContext context) =>
            context.RequestServices.GetRequiredService<IOptionsMonitor<LithindiumAuthenticationOptions>>().Get(LithindiumAuthenticationDefaults.Scheme);

        private static CookieOptions CreateCookieOptions() => new() {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true
        };

        private static bool ValidCookieName(string name) =>
            !string.IsNullOrEmpty(name) && name.StartsWith("__Host-", StringComparison.Ordinal) && name.Length > 7;

    }

}
