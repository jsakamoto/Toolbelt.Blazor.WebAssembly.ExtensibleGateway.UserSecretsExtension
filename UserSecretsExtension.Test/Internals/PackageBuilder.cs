using Toolbelt.Diagnostics;

namespace UserSecretsExtension.Test.Internals;

internal static class PackageBuilder
{
    public static async Task PackAsync(string relativeProjectPath)
    {
        // Run "dotnet pack" to create the NuGet package
        var projectPath = Path.Combine([PathUtils.SolutionDir, .. relativeProjectPath.Split('/')]);
        using var process = await XProcess
            .Start("dotnet", $"pack \"{projectPath}\" -c Release")
            .WaitForExitAsync();

        process.ExitCode.Is(0, message: $"\"dotnet pack\" failed for \"{projectPath}\" (exit code {process.ExitCode}).\n{process.Output}");
    }
}
