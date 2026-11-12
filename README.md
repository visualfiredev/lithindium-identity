# Lithindium Identity

A modern identity and authentication library for .NET.

> [!IMPORTANT]
> In active development. Review deployment configuration and validate security before production use.

## Packages

| Package | Responsibility |
| :--- | :--- |
| `Lithindium.Identity` | Identity records, stores, unified results and managers. |
| `Lithindium.Identity.EntityFrameworkCore` | SQLite and PostgreSQL persistence. |
| `Lithindium.Identity.AspNetCore` | Protected session cookies and HTTP authentication. |

The framework supports native and external accounts, verified contacts, passwords, TOTP, recovery codes, lockout, sessions and auditing. It is not an OAuth or OpenID Connect authorization server.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/visualfiredev/lithindium-identity.git
cd lithindium-identity
dotnet build Lithindium.Identity/Lithindium.Identity.sln
dotnet test Lithindium.Identity/Lithindium.Identity.sln
```

Register the framework in an ASP.NET Core host:

```csharp
builder.Services.AddLithindiumIdentity(
    users: new UserManagerOptions { MinimumPasswordLength = 15 });
builder.Services.AddLithindiumEntityFrameworkCore(options =>
    options.UseSqlite("Data Source=identity.db;Foreign Keys=True"));
builder.Services.AddLithindiumAuthentication();
builder.Services.AddAuthorization();
```

Use `UseNpgsql(connectionString)` for PostgreSQL. Configure shared Data Protection keys and an application name before deploying multiple instances. Resolve managers from the request scope.

For a fresh, dedicated development database, use `IdentityDbContext.Database.EnsureCreatedAsync()`. For production, generate and review provider-specific EF Core migrations against an empty schema; do not combine EnsureCreated with migrations. The design-time factory accepts `LITHINDIUM_DB_PROVIDER` (`sqlite` or `postgres`) and `LITHINDIUM_DB_CONNECTION`. Example for SQLite:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
$env:LITHINDIUM_DB_PROVIDER = "sqlite"
$env:LITHINDIUM_DB_CONNECTION = "Data Source=identity.db;Foreign Keys=True"
dotnet ef migrations add InitialIdentity --project Lithindium.Identity/Lithindium.Identity.EntityFrameworkCore
dotnet ef database update --project Lithindium.Identity/Lithindium.Identity.EntityFrameworkCore
```

Maintain separate migrations for each provider. The core is designed for Native AOT; EF Core and the HTTP/sample layers do not claim Native AOT compatibility.

## Sample

```powershell
dotnet dev-certs https --trust
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project Lithindium.Identity/Lithindium.Identity.Sample -- --urls https://localhost:7243
```

The sample provides `GET /csrf`, `POST /auth/register`, `POST /auth/login`, `POST /auth/mfa/totp`, `POST /auth/mfa/recovery`, `POST /auth/logout` and protected `GET /me`. Supply the antiforgery token in `X-CSRF-TOKEN` with the cookie on POSTs; refresh it after sign-in. The sample is development-only.

## Security boundaries

- Use HTTPS, secure shared key storage, authorization, antiforgery and distributed rate limits. External assertions and message delivery remain host responsibilities.
- Managers are trusted backend services; never expose raw user records, hashes, protected payloads or bearer credentials in logs or public APIs.
- Use one scoped store, primary database reads and serializable transactions. Retry known aborted operations from fresh state; never retry unknown commit outcomes blindly. Preserve the identifier ledger, including during migration and backup restoration.
- SQLite uses Unicode NFC/invariant uppercase lookup keys; PostgreSQL uses the same keys with deterministic collation. Issuer/subject are exact. The new schema is not a migration from the original OIDC database.
- Password policy defaults are configurable; production deployments also need compromised-password screening, recovery/delivery policy and operational monitoring.

CI builds and exercises SQLite and PostgreSQL. Set `LITHINDIUM_TEST_POSTGRES` to a disposable PostgreSQL server connection with database creation privileges to run PostgreSQL tests locally.

## Contributing

Issues, proposals and pull requests are welcome. Report suspected vulnerabilities privately.

## AI Assisted Development

Human contributors and AI coding agents collaborate under human direction, engineering oversight and review. All contributions must meet the same correctness, maintainability, security, attribution and licensing standards.

## License

Apache License 2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
