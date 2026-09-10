# Locally built packages

Drop a NuGet package here that this repository has to restore but does not build.

The sample app that the E2E tests run references `Toolbelt.Blazor.WebAssembly.ExtensibleGateway`,
which is built and released from a repository of its own. While the version the sample app asks for
is still unreleased, there is nothing on nuget.org to restore, so build that repository and copy the
`.nupkg` it writes into its own `_dist` folder over to this folder.

A test run copies everything it finds here into `_dist`, the local NuGet feed that `nuget.config`
registers, so a package placed here is restored the same way as the package this repository builds.
The `.nupkg` files themselves are not tracked by git.

Once the version the sample app asks for is published on nuget.org, this folder can be emptied
again.
