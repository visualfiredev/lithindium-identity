// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Lithindium.Identity.AspNetCore;
using Lithindium.Identity.EntityFrameworkCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Lithindium.Identity.Tests {

    public sealed class HttpIdentityTests {

        [Fact]
        public async Task CookieIsSecureAndRevocationTakesEffectOnNextRequest() {
            string file = Path.Combine(Path.GetTempPath(), "lithindium-http-" + Guid.NewGuid().ToString("N") + ".db");
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            builder.Services.AddLithindiumEntityFrameworkCore(options => options.UseSqlite("Data Source=" + file + ";Foreign Keys=True;Pooling=False"));
            builder.Services.AddLithindiumAuthentication();
            builder.Services.AddAuthorization();
            await using var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapPost("/login", async (HttpContext context, SignInManager signIn) => {
                var result = await signIn.PasswordSignInAsync("Alice", "a sufficiently long password");
                if (!result.HasValue || !result.Value.IsAuthenticated) return Results.Unauthorized();
                context.SetLithindiumSession(result.Value);
                return Results.Ok();
            });
            app.MapPost("/logout", async (HttpContext context) => {
                var result = await context.SignOutLithindiumAsync();
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest();
            });
            app.MapGet("/private", () => Results.Ok()).RequireAuthorization();
            try {
                await using (AsyncServiceScope scope = app.Services.CreateAsyncScope()) {
                    await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();
                    Assert.True((await scope.ServiceProvider.GetRequiredService<UserManager>().CreateAsync("Alice", "a sufficiently long password")).HasValue);
                }
                await app.StartAsync();
                using HttpClient client = app.GetTestClient();
                client.BaseAddress = new Uri("https://localhost");
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/private")).StatusCode);
                using HttpResponseMessage login = await client.PostAsync("/login", null);
                Assert.Equal(HttpStatusCode.OK, login.StatusCode);
                string header = Assert.Single(login.Headers.GetValues("Set-Cookie"));
                Assert.Contains("__Host-Lithindium=", header, StringComparison.Ordinal);
                Assert.Contains("httponly", header, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("secure", header, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("samesite=strict", header, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("path=/", header, StringComparison.OrdinalIgnoreCase);
                string cookie = header.Split(';')[0];
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/private")).StatusCode);
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", "__Host-Lithindium=invalid");
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/private")).StatusCode);
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/logout", null)).StatusCode);
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/private")).StatusCode);
                await app.StopAsync();
            } finally {
                await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        public void DiagnosticStringsDoNotContainCredentialMaterial() {
            User user = new(Guid.NewGuid(), "private-user", "secret-password-hash", "private-name", 0, UserStatus.Active);
            Assert.DoesNotContain("secret-password-hash", user.ToString());
            Assert.DoesNotContain("private-user", user.ToString());
            Assert.DoesNotContain("private@example.com", new UserEmail("private@example.com", true).ToString());
        }

    }

}
