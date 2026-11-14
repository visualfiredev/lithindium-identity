# Lithindium Identity

A modern identity and authentication library for .NET.

> [!IMPORTANT]
> In active development. Review deployment configuration and validate security before production use.

## Projects

- `Lithindium.Identity` — identity records and store contracts.
- `Lithindium.Identity.Reference` — previous managers, options, results and extensions, retained as source files for reference only. Not compiled or packed.
- `Lithindium.Identity.Tests` — empty test project reserved for future tests.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/visualfiredev/lithindium-identity.git
cd lithindium-identity
dotnet build Lithindium.Identity/Lithindium.Identity.sln
```

The library is being redesigned and does not yet provide a working authentication implementation.

All identity store reads and writes require an explicit transaction. `IUserStore.BeginTransactionAsync()` exposes a transaction-bound store with the same supported interfaces. Mutation attempts return `bool`: `true` on acceptance within the transaction, `false` on expected rejection. Unexpected failures throw. Only `SaveAsync()` commits changes; disposal rolls back unsaved changes. A known commit rejection returns `false` and rolls back. Transactions provide serializable isolation and cannot be reused after completion.

```csharp
await using var transaction = await store.BeginTransactionAsync(cancellationToken);
var users = transaction.Store;
var security = (IUserSecurityStore)users;

if (!await users.TryCreateAsync(user, cancellationToken))
    return false;

if (!await security.TryUpdateSecurityAsync(user, null, userSecurity, cancellationToken))
    return false;

return await transaction.SaveAsync(cancellationToken);
```

The interfaces define persistence outcomes, conditional updates, and atomic mutations; authentication and verification policy belong to the calling code. Pending credentials and sessions are not usable until their challenges are satisfied. Related challenge bindings, consumption, and authentication-state changes must be saved together. Failed authentication attempts that update lockout or challenge counters must also be explicitly saved.

## Contributing

Issues, proposals and pull requests are welcome. Report suspected vulnerabilities privately.

## AI Assisted Development

Human contributors and AI coding agents collaborate under human direction, engineering oversight and review. All contributions must meet the same correctness, maintainability, security, attribution and licensing standards.

## License

Apache License 2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
