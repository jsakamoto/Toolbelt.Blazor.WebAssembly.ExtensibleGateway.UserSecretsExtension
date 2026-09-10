namespace UserSecretsExtension.Test.Internals;

/// <summary>
/// The packages that these tests have to restore but this repository does not build.
/// <para>
/// The sample app references "Toolbelt.Blazor.WebAssembly.ExtensibleGateway", which is built and
/// released from a repository of its own. While the version the sample app asks for is still
/// unreleased there is nothing on nuget.org to restore, so testing against it means building that
/// repository by hand and dropping the package it produces into the folder this class reads.
/// </para>
/// <para>
/// Everything found in that folder is copied into "_dist", the local NuGet feed that this
/// repository's "nuget.config" registers, so a package put there by hand is restored the same way as
/// the package this repository builds. That copy has to happen after "_dist" has been emptied for
/// the run, which is why the drop folder is a folder of its own rather than "_dist" itself.
/// </para>
/// </summary>
internal static class LocalPackages
{
    /// <summary>The folder to drop a package that was built somewhere else into.</summary>
    public const string FolderName = "_local-packages";

    /// <summary>Copies every package in the drop folder into the local NuGet feed.</summary>
    public static void CopyToLocalFeed()
    {
        var dropDir = Path.Combine(PathUtils.SolutionDir, FolderName);
        var distDir = Path.Combine(PathUtils.SolutionDir, "_dist");
        Directory.CreateDirectory(distDir);

        var packages = Directory.Exists(dropDir) ? Directory.GetFiles(dropDir, "*.nupkg") : [];
        foreach (var package in packages)
        {
            File.Copy(package, Path.Combine(distDir, Path.GetFileName(package)), overwrite: true);
            TestContext.Progress.WriteLine($"Restoring \"{Path.GetFileName(package)}\" from \"{FolderName}\" instead of nuget.org.");
        }

        if (packages.Length == 0)
        {
            TestContext.Progress.WriteLine(
                $"\"{dropDir}\" holds no packages, so everything this repository does not build is " +
                "restored from nuget.org. If the sample app asks for a version of " +
                "\"Toolbelt.Blazor.WebAssembly.ExtensibleGateway\" that is not released yet, build " +
                "that package from its own repository and copy the .nupkg into that folder.");
        }
    }
}
