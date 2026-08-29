using Toolbelt;

namespace UserSecretsExtension.Test.Internals;

internal static class PathUtils
{
    public static readonly string SolutionDir = FileIO.FindContainerDirToAncestor("*.slnx");
}
