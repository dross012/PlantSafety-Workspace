// ============================================================================
//  Test IDs:  PLNT-REG-PLAT-001..003
//  Purpose:   Pins two platform decisions that were expensive to establish and
//             would be cheap to undo by accident:
//
//               * every project targets .NET 10 (the migration touched 257
//                 projects across 13 repositories; a single project left on
//                 net8.0/net9.0 still restores and builds, so nothing about
//                 normal development would reveal the drift), and
//               * no project takes an Entity Framework dependency. The data
//                 access stack is Dapper + stored procedures +
//                 Microsoft.Data.SqlClient. EF arriving as a transitive
//                 convenience in one project is how that decision erodes.
// ============================================================================
using PlantSafety.RegressionTests.Support;

namespace PlantSafety.RegressionTests.Platform;

public sealed class PlatformDriftRegressionTests
{
    private const string RequiredFrameworkPrefix = "net10.0";

    private static readonly Regex TargetFrameworkElement = new(
        @"<TargetFrameworks?>(?<value>[^<]*)</TargetFrameworks?>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static IReadOnlyList<string> ProjectFiles()
    {
        var files = Directory
            .EnumerateFiles(WorkspaceRoot.Directory.FullName, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            // .claude holds git worktrees, which are full checkouts of this same repository on
            // other branches. Scanning them would double-count every project and, worse, make
            // this test's result depend on whatever unrelated branch is checked out there.
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}.claude{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        files.Should().NotBeEmpty("the project scan must find something, or every assertion over it is vacuous");

        return files;
    }

    [Fact(DisplayName = "PLNT-REG-PLAT-001: Every project targets .NET 10")]
    public void AllProjects_ShouldTargetNet10()
    {
        var drifted = new List<string>();

        foreach (var project in ProjectFiles())
        {
            foreach (Match element in TargetFrameworkElement.Matches(File.ReadAllText(project)))
            {
                foreach (var framework in element.Groups["value"].Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    // MSBuild property references such as $(TargetFrameworks) appear in the MAUI
                    // head projects, which append platform TFMs to an inherited list. There is no
                    // literal framework to check in that fragment.
                    if (framework.StartsWith('$'))
                    {
                        continue;
                    }

                    // Platform-suffixed TFMs (net10.0-android, net10.0-windows10.0.19041.0) are
                    // still .NET 10, hence a prefix check rather than equality.
                    if (!framework.StartsWith(RequiredFrameworkPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        drifted.Add($"{Path.GetFileName(project)} targets {framework}");
                    }
                }
            }
        }

        drifted.Should().BeEmpty(
            "the platform is .NET 10 across every repository; a project left behind still builds, " +
            "so nothing but this check would surface it:\n{0}", string.Join("\n", drifted));
    }

    [Fact(DisplayName = "PLNT-REG-PLAT-002: No project references Entity Framework")]
    public void NoProject_ShouldReferenceEntityFramework()
    {
        var offenders = ProjectFiles()
            .Where(p => Regex.IsMatch(File.ReadAllText(p), @"Include=""(Microsoft\.EntityFrameworkCore|EntityFramework)", RegexOptions.IgnoreCase))
            .Select(Path.GetFileName)
            .ToArray();

        offenders.Should().BeEmpty(
            "data access is Dapper over stored procedures with Microsoft.Data.SqlClient. An EF " +
            "reference introduces a second, competing persistence model:\n{0}",
            string.Join("\n", offenders));
    }

    [Fact(DisplayName = "PLNT-REG-PLAT-003: No source file imports an Entity Framework namespace")]
    public void NoSourceFile_ShouldImportEntityFramework()
    {
        // Checked separately from the package reference: EF can arrive transitively through
        // another package, in which case there is no Include= line to find but the namespace is
        // still usable and the first `using` is the moment the decision has actually been undone.
        var offenders = Directory
            .EnumerateFiles(WorkspaceRoot.Directory.FullName, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}.claude{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            // This file names the namespace it forbids, which would otherwise match itself.
            .Where(p => !p.EndsWith(nameof(PlatformDriftRegressionTests) + ".cs", StringComparison.OrdinalIgnoreCase))
            .Where(p => Regex.IsMatch(File.ReadAllText(p), @"^\s*(global\s+)?using\s+Microsoft\.EntityFrameworkCore", RegexOptions.Multiline))
            .Select(Path.GetFileName)
            .ToArray();

        offenders.Should().BeEmpty(
            "an Entity Framework import means a second persistence model has entered the codebase:\n{0}",
            string.Join("\n", offenders));
    }
}
