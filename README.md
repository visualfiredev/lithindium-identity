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

All identity store reads and writes require an explicit transaction. Each store instance owns its transaction, shared by all of its supported store interfaces. Mutation attempts return `bool`: `true` on acceptance within the transaction, `false` on expected rejection. Unexpected failures throw. Only `SaveAsync()` commits changes; `RollbackAsync()` or store disposal rolls back unsaved changes. A known commit rejection returns `false` and rolls back. Transactions provide serializable isolation, cannot be nested, and end on save or rollback. Do not share a store instance across concurrent callers.

```csharp
var security = (IUserSecurityStore)store;
await store.BeginTransactionAsync(cancellationToken);
try {
    if (!await store.TryCreateAsync(user, cancellationToken))
        return false;

    if (!await security.TryUpdateSecurityAsync(user, null, userSecurity, cancellationToken))
        return false;

    return await store.SaveAsync(cancellationToken);
} finally {
    await store.RollbackAsync();
}
```

Rollback is a no-op when no transaction is active. A store can begin another transaction after a completed save or rollback. Read-only transactions also require explicit completion.

The interfaces define persistence outcomes, conditional updates, and atomic mutations; authentication and verification policy belong to the calling code. Pending credentials and sessions are not usable until their challenges are satisfied. Related challenge bindings, consumption, and authentication-state changes must be saved together. Failed authentication attempts that update lockout or challenge counters must also be explicitly saved.

## Contributing

Issues, proposals and pull requests are welcome. Report suspected vulnerabilities privately.

## AI Assisted Development

Human contributors and AI coding agents collaborate under human direction, engineering oversight and review. All contributions must meet the same correctness, maintainability, security, attribution and licensing standards.

## License

Apache License 2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
