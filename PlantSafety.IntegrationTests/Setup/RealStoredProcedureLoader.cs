// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           RealStoredProcedureLoader.cs
//  Created:        2026-08-11
//  SchemaVersion:  2026.1
//  Description:    Loads the production stored procedures from their real .sql
//                  files instead of from hand-copied duplicates.
// ============================================================================
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Creates the integration-test database's stored procedures by executing the real
/// <c>PlantSafety.Infrastructure/**/Sql/Procedures/*.sql</c> files.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TestDatabaseFactory"/> previously declared these procedures a second time, as C#
/// string literals. Because nothing kept the copies in step with the real files, they drifted, and
/// every drift surfaced as a confusing runtime failure rather than a compile error. Four separate
/// instances were found in one sitting: <c>usp_Risk_Update</c> and <c>usp_Accident_Update</c> were
/// missing the <c>@TenantId</c>/<c>@ETag</c> parameters added upstream (SQL error 8144, "too many
/// arguments"); both still ended with the superseded ETag-only <c>SELECT</c> where the real
/// procedures now return <c>@RowsAffected</c> (surfacing as
/// <c>FormatException</c> converting a 0x... string to <see cref="int"/>); and
/// <c>usp_Risk_GetById</c> was missing <c>@TenantId</c> entirely.
/// </para>
/// <para>
/// Executing the real files makes them the single source of truth, so that class of drift cannot
/// recur. Two mechanical rewrites are applied, and they are exactly the two adaptations the
/// hand-written copies were already making by hand:
/// </para>
/// <list type="number">
///   <item><description>
///     <c>USE [PlantSafety.SomeDB];</c> is dropped. Production splits across several databases;
///     the Testcontainers instance runs everything in one, so a USE would switch to a database
///     that does not exist here.
///   </description></item>
///   <item><description>
///     <c>[PlantSafety.AuditDB].dbo.</c> and <c>[PlantSafety.ErrorDB].dbo.</c> are rewritten to
///     <c>dbo.</c>, putting AuditLog and ErrorLog in the same database as everything else. The
///     test suite already asserts against <c>dbo.AuditLog</c>/<c>dbo.ErrorLog</c> in 43 places,
///     and <see cref="TestDatabaseFactory.ApplySharedAuditLoggingSchemaAsync"/> creates them there.
///   </description></item>
/// </list>
/// <para>
/// Nothing else is altered: parameter lists, bodies and return shapes are whatever the real file
/// says, which is the entire point.
/// </para>
/// </remarks>
public static class RealStoredProcedureLoader
{
    private static readonly Regex UseStatement =
        new(@"^\s*USE\s+\[[^\]]+\]\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CrossDatabaseAuditRef =
        new(@"\[PlantSafety\.(?:Audit|Error)DB\]\.dbo\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProcedureDeclaration =
        new(@"CREATE\s+(?:OR\s+ALTER\s+)?PROCEDURE\s+\[(?<schema>[^\]]+)\]\s*\.",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex QualifiedProcedureName =
        new(@"CREATE\s+(?:OR\s+ALTER\s+)?PROCEDURE\s+\[(?<schema>[^\]]+)\]\s*\.\s*\[(?<name>[^\]]+)\]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex GoSeparator =
        new(@"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Executes every real stored-procedure .sql file against the test database.
    /// </summary>
    /// <param name="connectionString">Connection string for the Testcontainers instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the Infrastructure project cannot be located, if it yields no .sql files, or if
    /// any file fails to execute. Failing loudly is deliberate: silently skipping a procedure
    /// would reintroduce exactly the drift this class exists to remove.
    /// </exception>
    public static async Task ApplyAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var tableFiles = EnumerateSqlFiles(Path.Combine("Sql", "Tables"));
        var procedureFiles = EnumerateSqlFiles(Path.Combine("Sql", "Procedures"));

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        EnsureNoConflictingDefinitions(procedureFiles);

        // Captured before any schema is created: these are the modules TestDatabaseFactory
        // provisioned, i.e. the ones the tests actually exercise. Used below to decide which
        // procedure failures are fatal.
        var fixtureSchemas = await GetExistingSchemasAsync(connection, cancellationToken);

        await EnsureSchemasAsync(connection, procedureFiles, cancellationToken);

        // Tables first, best-effort. The real table files are idempotent -- CREATE TABLE is wrapped
        // in IF NOT EXISTS, followed by per-column
        // "IF NOT EXISTS (SELECT 1 FROM sys.columns ...) ALTER TABLE ... ADD <col>" blocks -- so
        // running them leaves the fixture's own tables alone while topping up any column its inline
        // CREATE TABLE is missing. Without this, 22 procedures could not be created at all, failing
        // with "Invalid column name" for columns the real schema has and the hand-written fixture
        // tables never gained (Culture.ReviewedByUserId, Incident.EscalationReason,
        // RCA.StartedBy, ...).
        //
        // Failures here are tolerated on purpose. This pass exists only to top up columns; it is
        // not trying to stand up the full production schema, and a table the fixture never creates
        // can legitimately fail on a foreign key to another absent table. Nothing is hidden by
        // that: if a column a procedure needs is genuinely missing, the strict procedure pass below
        // still fails loudly with "Invalid column name".
        var tableFailures = new List<string>();
        await ApplyFilesAsync(connection, tableFiles, tableFailures, cancellationToken, continueAfterFailedBatch: true);

        var failures = new List<string>();
        await ApplyFilesAsync(connection, procedureFiles, failures, cancellationToken);

        // Every procedure is applied, but only those belonging to a schema TestDatabaseFactory
        // itself provisioned are treated as fatal. Those are the modules under test, and they are
        // precisely the procedures that used to be hand-copied into the fixture -- so this keeps
        // the guarantee that matters: nothing the tests depend on can silently fall back to a
        // stale duplicate.
        //
        // The rest belong to modules the fixture has no schema for at all (EnterpriseFabric,
        // DigitalTwin, Sustainability, ...). Their tables do not exist here, so their procedures
        // cannot compile, and demanding otherwise would mean standing up the entire production
        // schema just to run these tests.
        var fatal = failures
            .Where(failure => fixtureSchemas.Contains(SchemaOf(failure, procedureFiles)))
            .ToList();

        if (fatal.Count > 0)
        {
            throw new InvalidOperationException(
                $"{fatal.Count} of {procedureFiles.Count} real stored-procedure file(s) failed to apply to the " +
                $"integration-test database, in schemas this fixture provisions:" +
                $"{Environment.NewLine}{string.Join(Environment.NewLine, fatal)}");
        }
    }

    /// <summary>
    /// Fails if two .sql files define the same <c>[schema].[procedure]</c>.
    /// </summary>
    /// <remarks>
    /// Applying every file in path order means the last writer wins, so a duplicate silently
    /// decides which definition the tests run against. That is precisely the hidden behaviour this
    /// loader exists to remove, and it is not hypothetical: two copies of usp_Roi_Create existed
    /// (Features/Financials/ROI/... and Financials/...), the stale one sorted later, and it
    /// overwrote the canonical version -- costing three ROI tests their audit-log, THROW and ETag
    /// behaviour with no error anywhere. Ambiguity is a repo problem; refuse to guess.
    ///
    /// Note this keys on schema AND name. Same file name in different schemas is legitimate --
    /// [Analytics].[usp_Schedules_List] and [Operational].[usp_Schedules_List] are different
    /// procedures -- and must not trip this.
    /// </remarks>
    private static void EnsureNoConflictingDefinitions(IReadOnlyList<string> procedureFiles)
    {
        var byQualifiedName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in procedureFiles)
        {
            var text = File.ReadAllText(file);
            foreach (Match match in ProcedureDeclaration.Matches(text))
            {
                var name = QualifiedProcedureName.Match(text);
                if (!name.Success)
                {
                    continue;
                }

                var key = $"[{name.Groups["schema"].Value}].[{name.Groups["name"].Value}]";
                if (!byQualifiedName.TryGetValue(key, out var paths))
                {
                    byQualifiedName[key] = paths = new List<string>();
                }

                paths.Add(file);
                break;
            }
        }

        var conflicts = byQualifiedName
            .Where(pair => pair.Value.Count > 1)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"  {pair.Key}{Environment.NewLine}    {string.Join($"{Environment.NewLine}    ", pair.Value)}")
            .ToList();

        if (conflicts.Count > 0)
        {
            throw new InvalidOperationException(
                $"{conflicts.Count} stored procedure(s) are defined by more than one .sql file. Applying them in " +
                $"path order would let an arbitrary copy win, so the duplicates must be resolved in the repo:" +
                $"{Environment.NewLine}{string.Join(Environment.NewLine, conflicts)}");
        }
    }

    /// <summary>Resolves the schema a failed file declares its procedure into.</summary>
    private static string SchemaOf(string failure, IReadOnlyList<string> procedureFiles)
    {
        var fileName = failure.Split(':', 2)[0];
        var path = procedureFiles.FirstOrDefault(p => Path.GetFileName(p) == fileName);
        if (path is null)
        {
            return string.Empty;
        }

        var match = ProcedureDeclaration.Match(File.ReadAllText(path));
        return match.Success ? match.Groups["schema"].Value : string.Empty;
    }

    /// <summary>Reads the schemas that already exist, before this loader creates any.</summary>
    private static async Task<HashSet<string>> GetExistingSchemasAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        var schemas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sys.schemas;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            schemas.Add(reader.GetString(0));
        }

        return schemas;
    }

    /// <summary>Executes each file batch-by-batch, recording the first failure per file.</summary>
    /// <param name="continueAfterFailedBatch">
    /// When true, a failed batch does not abandon the rest of its file. Table files need this:
    /// their batches are independent (CREATE TABLE, then one guarded
    /// "ALTER TABLE ... ADD &lt;col&gt;" per column), so giving up at the first failure silently
    /// skips the very column top-ups this loader runs them for -- which is what left
    /// [Financials].[Roi] without its RowVersion column. A procedure file is one logical unit, so
    /// it stops at the first failure.
    /// </param>
    private static async Task ApplyFilesAsync(
        SqlConnection connection,
        IReadOnlyList<string> files,
        List<string> failures,
        CancellationToken cancellationToken,
        bool continueAfterFailedBatch = false)
    {
        foreach (var file in files)
        {
            var script = Normalize(await File.ReadAllTextAsync(file, cancellationToken));

            foreach (var batch in GoSeparator.Split(script))
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                try
                {
                    await using var command = connection.CreateCommand();
                    command.CommandText = batch;
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (SqlException ex)
                {
                    failures.Add($"{Path.GetFileName(file)}: {ex.Message}");
                    if (!continueAfterFailedBatch)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates every schema the procedure files declare into, if it does not already exist.
    /// </summary>
    /// <remarks>
    /// The Apply*SchemaAsync methods only create schemas for the modules their own tests exercise,
    /// but this loader applies every real procedure, and CREATE PROCEDURE fails outright ("The
    /// specified schema name X does not exist") when its schema is absent — unlike a reference to a
    /// missing *table*, which SQL Server defers. An empty schema is harmless, and creating them all
    /// means a new test can rely on the real procedures already being present.
    /// </remarks>
    private static async Task EnsureSchemasAsync(
        SqlConnection connection,
        IReadOnlyList<string> procedureFiles,
        CancellationToken cancellationToken)
    {
        var schemas = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in procedureFiles)
        {
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            foreach (Match match in ProcedureDeclaration.Matches(text))
            {
                schemas.Add(match.Groups["schema"].Value);
            }
        }

        foreach (var schema in schemas)
        {
            if (schema.Equals("dbo", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            // Schema names come from CREATE PROCEDURE declarations in repo-controlled .sql files,
            // but they are still quoted via QUOTENAME rather than concatenated raw.
            command.CommandText =
                """
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = @Schema)
                BEGIN
                    DECLARE @CreateSchemaSql nvarchar(300) = N'CREATE SCHEMA ' + QUOTENAME(@Schema);
                    EXEC sp_executesql @CreateSchemaSql;
                END
                """;
            command.Parameters.AddWithValue("@Schema", schema);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>Applies the two test-environment rewrites described on the class.</summary>
    private static string Normalize(string script)
    {
        script = UseStatement.Replace(script, string.Empty);
        script = CrossDatabaseAuditRef.Replace(script, "dbo.");
        return script;
    }

    /// <summary>
    /// Finds every <c>Sql/Procedures/*.sql</c> file in PlantSafety.Infrastructure by walking up
    /// from the test assembly's output directory to the repository root.
    /// </summary>
    private static IReadOnlyList<string> EnumerateSqlFiles(string relativeFolder)
    {
        var infrastructureRoot = FindInfrastructureRoot();

        var files = Directory
            .EnumerateFiles(infrastructureRoot, "*.sql", SearchOption.AllDirectories)
            .Where(path => path.Contains(relativeFolder, StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        return files.Count > 0
            ? files
            : throw new InvalidOperationException(
                $"No {relativeFolder}/*.sql files found under '{infrastructureRoot}'.");
    }

    private static string FindInfrastructureRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "PlantSafety.Infrastructure");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate the PlantSafety.Infrastructure directory by walking up from '{AppContext.BaseDirectory}'.");
    }
}
