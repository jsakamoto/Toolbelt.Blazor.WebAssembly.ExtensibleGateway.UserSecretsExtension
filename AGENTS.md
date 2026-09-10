# AGENTS.md

Guidance for AI coding agents (and anyone else) working in this repository.

## Testing against an unreleased ExtensibleGateway build

The sample app that `UserSecretsExtension.Test` runs against references
`Toolbelt.Blazor.WebAssembly.ExtensibleGateway`, which is built and released from its own
repository (`NET11/Gateway` in this checkout). When the version the sample app asks for
(`SampleApp/SampleApp.csproj`, driven by the `ExtensibleGatewayVersion` property in
`VersionInfo.props`) has not been published to nuget.org yet, there is nothing for `dotnet restore`
to pull down, and the test run fails to restore before it even gets to building anything.

To test against such an unreleased build:

1. Build the `ExtensibleGateway` package from its own repository, e.g. from `NET11/Gateway`:
   ```
   dotnet pack -c Release
   ```
   This writes a `.nupkg` into that repository's own `_dist` folder.
2. Copy that `.nupkg` file into this repository's `_local-packages` folder (see
   `_local-packages/README.md`).
3. Run the tests as usual (`dotnet test`, etc.).

### How it works

- `UserSecretsExtension.Test/Internals/LocalPackages.cs` defines `LocalPackages.CopyToLocalFeed()`.
- `AssemblySetup.cs`'s `[OneTimeSetUp]` calls it right after the `_dist` folder (this repository's
  local NuGet feed, registered in `nuget.config`) has been cleared, and before this repository's own
  package is packed.
- It copies every `.nupkg` found in `_local-packages` into `_dist`, so a package dropped there by
  hand restores exactly the way a package this repository builds does.
- `.nupkg` files are gitignored repository-wide, so nothing dropped into `_local-packages` for a
  test run ends up committed. Only `_local-packages/README.md` is tracked.

Once the version the sample app asks for is actually published on nuget.org, `_local-packages` can
be emptied again; the tests fall back to restoring from nuget.org when it holds nothing.

### A note on the test container's SDK

The E2E test in `SampleAppE2ETests.cs` runs the sample app inside a disposable
`mcr.microsoft.com/dotnet/sdk:11.0` container. That tag tracks the latest .NET 11 build, which is
what lets a gateway package built with today's local SDK actually start inside it. If the SDK on
your machine moves ahead of what that tag currently resolves to, the sample app can fail to start
with a "You must install or update .NET to run this application" error; pull the image again
(`docker pull mcr.microsoft.com/dotnet/sdk:11.0`) before assuming the extension itself is broken.
