// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RegressionTests.Support
//  Layer:          Tests (Regression)
//  Description:    Locates the workspace root so corpus-scanning regression
//                  tests can read the real .sql and .csproj files rather than
//                  a copy. Several of the defects this suite pins were drift
//                  between a source file and a hand-maintained duplicate, so
//                  reading anything other than the real file would defeat the
//                  purpose of the test.
// ============================================================================
namespace PlantSafety.RegressionTests.Support;

/// <summary>Resolves the workspace root directory from the test assembly's location.</summary>
public static class WorkspaceRoot
{
    private static readonly Lazy<DirectoryInfo> Root = new(Locate);

    /// <summary>Gets the workspace root (the directory holding Directory.Build.props and the module directories).</summary>
    public static DirectoryInfo Directory => Root.Value;

    /// <summary>Gets the PlantSafety module directory.</summary>
    public static DirectoryInfo PlantSafetyModule => new(Path.Combine(Root.Value.FullName, "PlantSafety"));

    private static DirectoryInfo Locate()
    {
        // Walking up from the test binary rather than using a compile-time constant: the same
        // assembly runs from bin/Debug and bin/Release, and from a git worktree whose absolute
        // path differs from the main tree's.
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var hasBuildProps = File.Exists(Path.Combine(current.FullName, "Directory.Build.props"));
            var hasModule = System.IO.Directory.Exists(Path.Combine(current.FullName, "PlantSafety"));

            // Both markers, not either: Directory.Build.props alone also exists in some module
            // directories, and a "PlantSafety" folder alone matches the module's own parent.
            if (hasBuildProps && hasModule)
            {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate the workspace root by walking up from '{AppContext.BaseDirectory}'. " +
            "Expected an ancestor directory containing both Directory.Build.props and a PlantSafety subdirectory.");
    }
}
