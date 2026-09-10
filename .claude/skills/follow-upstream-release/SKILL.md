---
name: follow-upstream-release
description: Use when the user asks to align this extension's version with a new release of Toolbelt.Blazor.WebAssembly.ExtensibleGateway (the upstream gateway package), or asks to bump this package's own version to match — including vague prompts like "the upstream released a new version, follow up", "the gateway released a new version, catch up this extension too", or "bump this extension to match the latest Gateway version".
---

# Following a new Toolbelt.Blazor.WebAssembly.ExtensibleGateway release

This extension does not reference the stock `Microsoft.AspNetCore.Components.Gateway` package at
all — it depends on `Toolbelt.Blazor.WebAssembly.ExtensibleGateway`, built from the sibling
repository `NET11/Gateway`. When that repository's own version moves, this extension's version
number is conventionally realigned to match, purely so users aren't confused by mismatched version
numbers across the package family — even when nothing about this extension's own behavior changed.
Say so explicitly in the release notes (see below).

## Steps

1. Confirm the new `Toolbelt.Blazor.WebAssembly.ExtensibleGateway` version (from the `NET11/Gateway`
   repository's own version bump — see its `follow-upstream-release` skill; that repository should be
   updated, packed, and tested first). Use its short release-train label (e.g. `11.0.0-rc.1`), not the
   full build-numbered string.
2. Update `VersionInfo.props`: bump both `ExtensibleGatewayVersion` and `UserSecretsExtensionVersion`
   to that same label, even though only `ExtensibleGatewayVersion` strictly needs to move.
3. Update `SampleApp/SampleApp.csproj`'s `Microsoft.AspNetCore.Components.WebAssembly`
   `PackageReference`, which uses a floating wildcard like `11.0.0-rc.1.*` — bump the release-train
   label prefix to match. (The `Toolbelt.Blazor.WebAssembly.ExtensibleGateway` and
   `Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension` references already pull from
   the `VersionInfo.props` properties, so they update automatically.)
4. Add a new entry at the top of `RELEASE-NOTES.txt` (see the `release-notes` skill for the house
   style). When nothing functional changed, phrase it like:
   ```
   v.X.Y.Z
   - Improve: Aligned the version number with the Toolbelt.Blazor.WebAssembly.ExtensibleGateway package. This release contains no functional changes.
   ```
5. The new `ExtensibleGateway` version is typically **not yet published on nuget.org** at this point,
   so this repository's E2E test can't restore it normally. Build it from `NET11/Gateway`
   (`dotnet pack -c Release`) and copy the resulting `.nupkg` from that repo's `_dist/` folder into
   this repository's `_local-packages/` folder, replacing any older version already there — see
   `AGENTS.md` for the full mechanism (`LocalPackages.CopyToLocalFeed()`, wired into
   `AssemblySetup.cs`).
6. Run the tests and confirm everything passes:
   ```
   dotnet test --project ./UserSecretsExtension.Test/UserSecretsExtension.Test.csproj --output detailed
   ```
   (Packing this extension's own package happens automatically in `AssemblySetup`'s
   `[OneTimeSetUp]` — no need to `dotnet pack` by hand first.)

## Gotchas

- `UserSecretsExtension.Test/SampleAppE2ETests.cs` uses the **floating**
  `mcr.microsoft.com/dotnet/sdk:11.0` tag (tracks whatever prerelease build MCR currently publishes
  for .NET 11). If a test fails with "You must install or update .NET to run this application," the
  fix is usually `docker pull mcr.microsoft.com/dotnet/sdk:11.0` to refresh the cached image, not
  editing the tag.
- Files in this repository are stored as LF line endings in git; a normal checkout converts them to
  CRLF locally. Editing with a POSIX tool (`sed`, a bash heredoc) leaves LF, which is harmless but
  worth re-normalizing to CRLF before committing if you want a clean diff.
