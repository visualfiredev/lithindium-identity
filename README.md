# Lithindium Identity

A modern identity and authentication library for .NET.

> [!IMPORTANT]
> In active development. Review deployment configuration and validate security before production use.

## Projects

- `Lithindium.Identity` — identity records and store contracts. Store contracts are temporarily excluded from compilation because they depend on the reference-only result system.
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

## Contributing

Issues, proposals and pull requests are welcome. Report suspected vulnerabilities privately.

## AI Assisted Development

Human contributors and AI coding agents collaborate under human direction, engineering oversight and review. All contributions must meet the same correctness, maintainability, security, attribution and licensing standards.

## License

Apache License 2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
