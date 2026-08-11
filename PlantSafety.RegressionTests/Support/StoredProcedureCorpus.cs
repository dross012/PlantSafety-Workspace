// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RegressionTests.Support
//  Layer:          Tests (Regression)
//  Description:    Reads the real stored-procedure .sql files and splits them
//                  into one unit per CREATE PROCEDURE, because several files
//                  hold more than one procedure separated by GO. Treating a
//                  multi-procedure file as a single unit is exactly the
//                  mistake that let the audit-logging gap hide: a file "has
//                  AuditLog" while three of its four procedures do not.
// ============================================================================
namespace PlantSafety.RegressionTests.Support;

/// <summary>A single CREATE PROCEDURE definition, isolated from its file.</summary>
/// <param name="Schema">The procedure's schema, e.g. "SafetyEvents".</param>
/// <param name="Name">The procedure's name, e.g. "usp_Accident_Insert".</param>
/// <param name="Body">The text of this procedure's batch only, not the whole file.</param>
/// <param name="FilePath">Absolute path of the file the procedure came from, for failure messages.</param>
public sealed record StoredProcedureDefinition(string Schema, string Name, string Body, string FilePath)
{
    /// <summary>Gets the fully-qualified "[schema].[name]" key.</summary>
    public string QualifiedName => $"[{Schema}].[{Name}]";

    /// <summary>Gets the file name alone, for compact assertion output.</summary>
    public string FileName => Path.GetFileName(FilePath);
}

/// <summary>Loads and parses every stored-procedure script in the PlantSafety module.</summary>
public static class StoredProcedureCorpus
{
    private static readonly Lazy<IReadOnlyList<StoredProcedureDefinition>> Cache = new(Load);

    private static readonly Regex ProcedureHeader = new(
        @"CREATE\s+(?:OR\s+ALTER\s+)?PROCEDURE\s+\[(?<schema>[^\]]+)\]\s*\.\s*\[(?<name>[^\]]+)\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // The \r? is load-bearing. In multiline mode $ matches before \n, so on a CRLF file the
    // carriage return sits between "GO" and the anchor and the separator never matches. Every
    // procedure in such a file then collapses into ONE segment, which silently mis-attributes
    // each finding to whichever procedure happens to be declared first -- and hides real gaps in
    // the later ones, because the first procedure's audit block satisfies the check for all of
    // them. This repository has files of both kinds.
    private static readonly Regex BatchSeparator = new(
        @"^[ \t]*GO[ \t]*\r?$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Gets every procedure defined under PlantSafety.Infrastructure's Sql/Procedures folders.</summary>
    public static IReadOnlyList<StoredProcedureDefinition> All => Cache.Value;

    private static IReadOnlyList<StoredProcedureDefinition> Load()
    {
        var infrastructure = Path.Combine(WorkspaceRoot.PlantSafetyModule.FullName, "PlantSafety.Infrastructure");

        var files = Directory
            .EnumerateFiles(infrastructure, "*.sql", SearchOption.AllDirectories)
            .Where(p => p.Contains(Path.Combine("Sql", "Procedures"), StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            // A corpus scan that silently finds nothing passes every assertion below and reports
            // green while checking nothing at all -- the worst possible failure mode for this
            // kind of test. Fail loudly instead.
            throw new InvalidOperationException(
                $"No stored-procedure scripts found under '{infrastructure}'. The regression corpus is empty, " +
                "which would make every corpus assertion vacuously true.");
        }

        var definitions = new List<StoredProcedureDefinition>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            foreach (var batch in BatchSeparator.Split(text))
            {
                var header = ProcedureHeader.Match(batch);

                if (header.Success)
                {
                    definitions.Add(new StoredProcedureDefinition(
                        header.Groups["schema"].Value,
                        header.Groups["name"].Value,
                        batch,
                        file));
                }
            }
        }

        return definitions;
    }
}
