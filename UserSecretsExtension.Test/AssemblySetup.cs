using UserSecretsExtension.Test.Internals;

// Not in any namespace: NUnit runs a [SetUpFixture]'s [OneTimeSetUp] once for the entire assembly
// when the fixture itself has no namespace, regardless of which single test ends up being run.
[SetUpFixture]
public class AssemblySetup
{
    [OneTimeSetUp]
    public async Task PackPackagesOnce()
    {
        // Delete all *.nupkg files in the "_dist" folder to ensure that the test runs with a clean slate and does not pick up stale content from previous runs.
        var distDir = Path.Combine(PathUtils.SolutionDir, "_dist");
        Directory.GetFiles(distDir, "*.nupkg").ToList().ForEach(File.Delete);

        await PackageBuilder.PackAsync("UserSecretsExtension/UserSecretsExtension.csproj");
    }
}
