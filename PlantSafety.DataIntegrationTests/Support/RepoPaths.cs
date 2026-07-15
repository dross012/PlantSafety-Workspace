// ============================================================================
//  Module:  DataIntegrationTests.Support
//  Purpose: Locates checked-in SQL script folders relative to the solution
//           root, independent of build output directory depth.
// ============================================================================
namespace PlantSafety.DataIntegrationTests.Support;

internal static class RepoPaths
{
    public static string ScriptsRoot => Path.Combine(SolutionRoot, "PlantSafety.Infrastructure", "Persistence", "Scripts");

    private static string SolutionRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "PlantSafety.sln")))
                dir = dir.Parent;

            if (dir is null)
                throw new InvalidOperationException("Could not locate PlantSafety.sln walking up from " + AppContext.BaseDirectory);

            return dir.FullName;
        }
    }
}
