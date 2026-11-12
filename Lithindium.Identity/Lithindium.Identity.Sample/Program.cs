// -------------------------------------------------------------------------------------------------
// visualfiredev.com - https://visualfiredev.com/
// Copyright (c) 2026 Visual Fire Development LLC
//
// SPDX-License-Identifier: Apache-2.0
// -------------------------------------------------------------------------------------------------

using System;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using Lithindium.Identity;
using Lithindium.Identity.AspNetCore;
using Lithindium.Identity.EntityFrameworkCore;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLithindiumIdentity();
builder.Services.AddLithindiumEntityFrameworkCore(options =>
    options.UseSqlite("Data Source=lithindium.db;Foreign Keys=True"));
builder.Services.AddLithindiumAuthentication();
builder.Services.AddDataProtection().SetApplicationName("Lithindium.Sample");
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();
// Development example only. Production deployments require managed migrations,
// distributed rate limits, delivery services, and a protected shared key ring.
if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Run this example in Development.");
await using (AsyncServiceScope scope = app.Services.CreateAsyncScope()) {
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) => {
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });
});

var auth = app.MapGroup("/auth").RequireRateLimiting("auth").AddEndpointFilter(async (invocation, next) => {
    HttpContext context = invocation.HttpContext;
    context.Response.Headers.CacheControl = "no-store";
    if (!context.Request.IsHttps) return Results.BadRequest();
    try {
        await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
    } catch (AntiforgeryValidationException) {
        return Results.BadRequest();
    }
    return await next(invocation);
});

auth.MapPost("/register", async (RegisterRequest request, UserManager users, HttpContext context) => {
    var result = await users.CreateAsync(request.Username, request.Password, cancellationToken: context.RequestAborted);
    return result.HasValue ? Results.Ok() : Results.BadRequest();
});

auth.MapPost("/login", async (LoginRequest request, SignInManager signIn, HttpContext context) =>
    Respond(context, await signIn.PasswordSignInAsync(request.Username, request.Password, Metadata(context), context.RequestAborted)));

auth.MapPost("/mfa/totp", async (TotpRequest request, SignInManager signIn, HttpContext context) =>
    Respond(context, await signIn.CompleteTotpAsync(request.UserUuid, request.ChallengeUuid, request.Proof, request.TotpUuid, request.Code, Metadata(context), context.RequestAborted)));

auth.MapPost("/mfa/recovery", async (RecoveryRequest request, SignInManager signIn, HttpContext context) =>
    Respond(context, await signIn.CompleteRecoveryCodeAsync(request.UserUuid, request.ChallengeUuid, request.Proof, request.Code, Metadata(context), context.RequestAborted)));

auth.MapPost("/logout", async (HttpContext context) => {
    OperationResult result = await context.SignOutLithindiumAsync();
    return result.IsSuccess || result.Status == StatusCode.InvalidCredential ? Results.NoContent() : Results.StatusCode(503);
});

app.MapGet("/me", (ClaimsPrincipal principal) => Results.Ok(new {
    userUuid = principal.FindFirstValue(ClaimTypes.NameIdentifier),
    sessionUuid = principal.FindFirstValue("sid")
})).RequireAuthorization();

app.Run();

static SessionMetadata Metadata(HttpContext context) => new() {
    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
    UserAgent = context.Request.Headers.UserAgent.ToString() is { Length: <= 2048 } agent ? agent : null
};

static IResult Respond(HttpContext context, OperationResult<SignInTicket> result) {
    if (!result.HasValue) return Results.Unauthorized();
    SignInTicket ticket = result.Value;
    if (ticket.IsAuthenticated) {
        context.SetLithindiumSession(ticket);
        return Results.Ok(new { authenticated = true });
    }
    return Results.Ok(new {
        authenticated = false,
        userUuid = ticket.UserUuid,
        challengeUuid = ticket.Challenge!.Uuid,
        proof = ticket.Challenge.Secret
    });
}

internal sealed record RegisterRequest(string Username, string Password);
internal sealed record LoginRequest(string Username, string Password);
internal sealed record TotpRequest(Guid UserUuid, Guid ChallengeUuid, string Proof, Guid TotpUuid, string Code);
internal sealed record RecoveryRequest(Guid UserUuid, Guid ChallengeUuid, string Proof, string Code);
