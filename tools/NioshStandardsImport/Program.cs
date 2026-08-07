// Standalone, one-off import tool. Not part of PlantSafety.sln and not referenced by any
// PlantSafety module -- it only writes to Compliance.NIOSHHazardProfile /
// Compliance.NIOSHRespiratorMatrix / Compliance.NIOSHPPEGuidance / Compliance.NIOSHHazardChangeLog /
// Compliance.NIOSHHazardModuleMapping in PlantSafety.CoreDB via the same schema the module code
// will read from.
//
// Unlike tools/OshaStandardsImport (which has a single canonical bulk data source, the eCFR
// Title 29 XML feed), NIOSH does not publish one bulk API for its hazard reference data (Pocket
// Guide REL/STEL values, health effects, respirator selection logic, PPE guidance). This tool
// therefore reads from a local, normalized seed file (data/niosh-hazards.seed.json) instead of
// downloading anything. The seed file is the swappable "input" boundary: everything from Phase 2
// onward (DDL, hashing, MERGE upsert, change-log, child-row replace, module mapping) operates on
// the in-memory ParsedHazardProfile list and has no idea whether that list came from a JSON seed
// file, a future scraper, or a future real NIOSH API client. To point this tool at a real source
// later, replace LoadSeedHazards (Phase 1) with a downloader/parser that produces the same
// List<ParsedHazardProfile> -- nothing downstream needs to change.
//
// Pipeline: read seed JSON -> run the Compliance/Sql/Tables NIOSH* DDL -> deploy the
// Compliance/Sql/Procedures usp_NIOSH* stored procedures -> upsert hazard profiles (SHA-256 delta
// detection) -> replace child rows (respirator matrix / PPE guidance) for changed hazards -> map
// hazards to PlantSafety modules (Phase 5).
//
// Phase 5's mapping ruleset (HazardCategory -> primary module, secondary module(s)) is
// deliberately explicit, inspectable C# data, not a black box -- every mapping written carries
// the MappingRuleCode/Rationale of the exact rule that produced it. It only names PlantSafety
// modules that actually exist as top-level PlantSafety.Infrastructure folders in this repository.
//
// Usage: dotnet run -- [--connection-string "..."] [--seed-file "..."]

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace NioshStandardsImport;

internal static class Program
{
    private static readonly Regex GoBatchSplitRegex = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static async Task Main(string[] args)
    {
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", ".."));
        var defaultSeedFile = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "data", "niosh-hazards.seed.json"));

        var connectionString = GetArg(args, "--connection-string")
            ?? "Server=localhost\\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";
        var seedFile = GetArg(args, "--seed-file") ?? defaultSeedFile;

        Console.WriteLine("=== PHASE 1: Load NIOSH hazard seed data ===");
        var hazards = LoadSeedHazards(seedFile);
        Console.WriteLine($"Loaded {hazards.Count} hazard profiles from {seedFile}");

        Console.WriteLine("=== PHASE 2: Build/verify NIOSH schema (Compliance.NIOSH* tables) ===");
        var sqlRoot = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Compliance", "Sql");
        RunNioshTableScripts(connectionString, Path.Combine(sqlRoot, "Tables"));

        Console.WriteLine("=== PHASE 2b: Deploy NIOSH stored procedures ===");
        RunNioshProcedureScripts(connectionString, Path.Combine(sqlRoot, "Procedures"));

        Console.WriteLine("=== PHASE 3: Diff against existing data and upsert changed hazard profiles ===");
        var diff = UpsertHazardProfiles(connectionString, hazards);
        Console.WriteLine($"New: {diff.NewCount}, Revised: {diff.RevisedCount}, Unchanged: {diff.UnchangedCount}, Removed: {diff.RemovedCount}");

        Console.WriteLine("=== PHASE 4: Replace respirator-matrix / PPE-guidance rows for changed hazards ===");
        ReplaceChildRows(connectionString, hazards, diff);

        Console.WriteLine("=== PHASE 5: Map hazard profiles to PlantSafety modules ===");
        ApplyModuleMapping(connectionString);

        Console.WriteLine("=== DONE ===");
    }

    private static string? GetArg(string[] args, string name)
    {
        var idx = Array.IndexOf(args, name);
        return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
    }

    // ------------------------------------------------------------------------------------------
    // Phase 1: input loading. This is the only phase that knows about the seed-file format --
    // a future real scraper/downloader replaces this method's body (and only this method's body)
    // and still returns List<ParsedHazardProfile>.
    // ------------------------------------------------------------------------------------------

    private static List<ParsedHazardProfile> LoadSeedHazards(string seedFile)
    {
        if (!File.Exists(seedFile))
            throw new FileNotFoundException($"NIOSH seed file not found: {seedFile}", seedFile);

        var json = File.ReadAllText(seedFile);
        var records = JsonSerializer.Deserialize<List<SeedHazardRecord>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Seed file {seedFile} did not deserialize to a hazard array.");

        return records.Select(r =>
        {
            // The seed source expresses RELs as free text with units/qualifiers (e.g. "100 ppm
            // (375 mg/m3)", "0.05 mg/m3 (respirable dust, TWA)") since NIOSH mixes ppm and mg/m3
            // across substances -- but NIOSHHazardProfile.REL_TWA/REL_STEL are FLOAT (no unit
            // column), specifically so callers can do numeric REL comparisons ("hazards with
            // REL < X"). We store the leading numeric value in the float column and fold the
            // original unit-qualified text into RecommendedControls so the unit/qualifier isn't
            // silently lost.
            var relTwaRaw = NullIfBlank(r.RelTwa);
            var relStelRaw = NullIfBlank(r.RelStel);
            var recommendedControls = NullIfBlank(r.RecommendedControls);
            if (relTwaRaw is not null || relStelRaw is not null)
            {
                var relNote = "NIOSH REL:" +
                    (relTwaRaw is not null ? $" TWA {relTwaRaw};" : "") +
                    (relStelRaw is not null ? $" STEL {relStelRaw};" : "");
                recommendedControls = recommendedControls is null ? relNote : $"{relNote} {recommendedControls}";
            }

            return new ParsedHazardProfile(
                ChemicalName: r.ChemicalName?.Trim() ?? throw new InvalidDataException("Seed hazard record is missing chemicalName."),
                CASNumber: string.IsNullOrWhiteSpace(r.CasNumber) ? null : r.CasNumber.Trim(),
                HazardCategory: r.HazardCategory?.Trim() ?? "General",
                REL_TWA: ParseLeadingDouble(relTwaRaw),
                REL_STEL: ParseLeadingDouble(relStelRaw),
                HealthEffects: NullIfBlank(r.HealthEffects),
                RecommendedControls: recommendedControls,
                PPERecommendations: NullIfBlank(r.PpeRecommendations),
                AtmosphericMonitoring: NullIfBlank(r.AtmosphericMonitoring),
                ErgonomicFactors: NullIfBlank(r.ErgonomicFactors),
                SourceUrl: NullIfBlank(r.SourceUrl),
                RespiratorMatrix: (r.RespiratorMatrix ?? [])
                    .Select(m => new ParsedRespiratorMatrixRow(m.HazardType, m.ConcentrationRange, m.RecommendedRespirator, NullIfBlank(m.Notes)))
                    .ToList(),
                PPEGuidance: (r.PpeGuidance ?? [])
                    .Select(p => new ParsedPpeGuidanceRow(p.PPEType, p.PerformanceCriteria, NullIfBlank(p.Limitations)))
                    .ToList());
        }).ToList();
    }

    private static readonly Regex LeadingNumberRegex = new(@"-?\d+(\.\d+)?", RegexOptions.Compiled);

    private static double? ParseLeadingDouble(string? text) =>
        text is not null && LeadingNumberRegex.Match(text) is { Success: true } m
            ? double.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture)
            : null;

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // ------------------------------------------------------------------------------------------
    // Phase 2: DDL runner. Globs NIOSH*.sql from the Compliance table-scripts folder instead of
    // hardcoding filenames, since the NIOSH table scripts are authored independently of this tool.
    // ------------------------------------------------------------------------------------------

    private static void RunNioshTableScripts(string connectionString, string tablesDir)
    {
        if (!Directory.Exists(tablesDir))
        {
            Console.WriteLine($"WARNING: Tables directory not found: {tablesDir} -- skipping table step.");
            return;
        }

        // NIOSHHazardProfile is the parent table every other NIOSH table has an FK to,
        // so it must run first regardless of alphabetical filename order.
        var scripts = Directory.GetFiles(tablesDir, "NIOSH*.sql")
            .OrderBy(f => Path.GetFileName(f).Equals("NIOSHHazardProfile.sql", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (scripts.Count == 0)
        {
            Console.WriteLine($"WARNING: No NIOSH*.sql files found under {tablesDir} -- skipping table step.");
            return;
        }

        foreach (var script in scripts)
        {
            RunDdlScript(connectionString, script);
        }
    }

    private static void RunNioshProcedureScripts(string connectionString, string proceduresDir)
    {
        if (!Directory.Exists(proceduresDir))
        {
            Console.WriteLine($"WARNING: Procedures directory not found: {proceduresDir} -- skipping procedure step.");
            return;
        }

        // Every NIOSH procedure is CREATE OR ALTER (idempotent) and none reference each other,
        // so deployment order doesn't matter here the way it does for the FK'd tables.
        var scripts = Directory.GetFiles(proceduresDir, "usp_NIOSH*.sql")
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (scripts.Count == 0)
        {
            Console.WriteLine($"WARNING: No usp_NIOSH*.sql files found under {proceduresDir} -- skipping procedure step.");
            return;
        }

        foreach (var script in scripts)
        {
            RunDdlScript(connectionString, script);
        }
    }

    private static void RunDdlScript(string connectionString, string scriptPath)
    {
        var script = File.ReadAllText(scriptPath);
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        foreach (var batch in GoBatchSplitRegex.Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            using var command = new SqlCommand(batch, connection);
            command.ExecuteNonQuery();
        }
        Console.WriteLine($"Applied {Path.GetFileName(scriptPath)}");
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string ComputeProfileHash(ParsedHazardProfile p)
    {
        // Normalized concatenation of every profile field that constitutes the hazard's
        // substantive content -- excludes CASNumber/ChemicalName only in the sense that changes
        // to those are keying changes (a new identity), not a "Revised" delta on the same hazard.
        // They're still included here since a corrected name/CAS on the same seed key should also
        // register as Revised.
        var sb = new StringBuilder();
        sb.Append(p.ChemicalName).Append('|');
        sb.Append(p.CASNumber).Append('|');
        sb.Append(p.HazardCategory).Append('|');
        sb.Append(p.REL_TWA).Append('|');
        sb.Append(p.REL_STEL).Append('|');
        sb.Append(p.HealthEffects).Append('|');
        sb.Append(p.RecommendedControls).Append('|');
        sb.Append(p.PPERecommendations).Append('|');
        sb.Append(p.AtmosphericMonitoring).Append('|');
        sb.Append(p.ErgonomicFactors).Append('|');
        sb.Append(p.SourceUrl).Append('|');
        foreach (var m in p.RespiratorMatrix.OrderBy(x => x.HazardType, StringComparer.Ordinal).ThenBy(x => x.ConcentrationRange, StringComparer.Ordinal))
            sb.Append(m.HazardType).Append(':').Append(m.ConcentrationRange).Append(':').Append(m.RecommendedRespirator).Append(':').Append(m.Notes).Append(';');
        foreach (var g in p.PPEGuidance.OrderBy(x => x.PPEType, StringComparer.Ordinal))
            sb.Append(g.PPEType).Append(':').Append(g.PerformanceCriteria).Append(':').Append(g.Limitations).Append(';');
        return ComputeHash(sb.ToString());
    }

    // A hazard's identity key: CASNumber when present (the more stable identifier), else
    // ChemicalName. Matches the task's stated upsert key.
    private static string KeyFor(ParsedHazardProfile p) => p.CASNumber ?? p.ChemicalName;

    // ------------------------------------------------------------------------------------------
    // Phase 3: upsert + change classification.
    // ------------------------------------------------------------------------------------------

    private static Dictionary<string, ExistingHazard> LoadExistingHazards(SqlConnection connection, SqlTransaction transaction)
    {
        var existing = new Dictionary<string, ExistingHazard>();
        const string sql = """
            SELECT HazardId, ChemicalName, CASNumber, ProfileHash
            FROM [Compliance].[NIOSHHazardProfile];
            """;
        using var command = new SqlCommand(sql, connection, transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetGuid(0);
            var chemicalName = reader.GetString(1);
            var cas = reader.IsDBNull(2) ? null : reader.GetString(2);
            var hash = reader.IsDBNull(3) ? null : reader.GetString(3);
            var key = cas ?? chemicalName;
            existing[key] = new ExistingHazard(id, chemicalName, cas, hash);
        }
        return existing;
    }

    private static NioshDiffResult UpsertHazardProfiles(string connectionString, List<ParsedHazardProfile> hazards)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = LoadExistingHazards(connection, transaction);
        var hazardIds = new Dictionary<string, Guid>();
        var changedHazardKeys = new HashSet<string>();
        var seenKeys = new HashSet<string>();
        int newCount = 0, revisedCount = 0, unchangedCount = 0, removedCount = 0;

        const string upsertSql = """
            MERGE [Compliance].[NIOSHHazardProfile] AS target
            USING (SELECT @Key AS HazardKey) AS source
            ON (target.CASNumber IS NOT NULL AND target.CASNumber = @CASNumber)
               OR (target.CASNumber IS NULL AND target.ChemicalName = @ChemicalName AND @CASNumber IS NULL)
            WHEN MATCHED THEN UPDATE SET
                ChemicalName = @ChemicalName, CASNumber = @CASNumber, REL_TWA = @REL_TWA, REL_STEL = @REL_STEL,
                HealthEffects = @HealthEffects, RecommendedControls = @RecommendedControls,
                PPERecommendations = @PPERecommendations, AtmosphericMonitoring = @AtmosphericMonitoring,
                ErgonomicFactors = @ErgonomicFactors, SourceUrl = @SourceUrl, ProfileHash = @ProfileHash,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                (HazardId, ChemicalName, CASNumber, REL_TWA, REL_STEL, HealthEffects, RecommendedControls,
                 PPERecommendations, AtmosphericMonitoring, ErgonomicFactors, SourceUrl, ProfileHash)
                VALUES (@HazardProfileId, @ChemicalName, @CASNumber, @REL_TWA, @REL_STEL, @HealthEffects, @RecommendedControls,
                        @PPERecommendations, @AtmosphericMonitoring, @ErgonomicFactors, @SourceUrl, @ProfileHash)
            OUTPUT inserted.HazardId;
            """;
        const string changeLogSql = """
            INSERT INTO [Compliance].[NIOSHHazardChangeLog]
                (ChangeLogId, HazardId, ChemicalName, ChangeType, PreviousProfileHash, NewProfileHash, Summary)
            VALUES
                (@ChangeLogId, @HazardProfileId, @ChemicalName, @ChangeType, @PreviousHash, @NewHash, @Summary);
            """;

        foreach (var h in hazards)
        {
            var key = KeyFor(h);
            seenKeys.Add(key);
            var hash = ComputeProfileHash(h);
            existing.TryGetValue(key, out var prior);

            var changeType = prior is null ? "New" : prior.ProfileHash != hash ? "Revised" : null;

            using var command = new SqlCommand(upsertSql, connection, transaction);
            var newId = Guid.NewGuid();
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@HazardProfileId", newId);
            command.Parameters.AddWithValue("@ChemicalName", h.ChemicalName);
            command.Parameters.AddWithValue("@CASNumber", (object?)h.CASNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@REL_TWA", (object?)h.REL_TWA ?? DBNull.Value);
            command.Parameters.AddWithValue("@REL_STEL", (object?)h.REL_STEL ?? DBNull.Value);
            command.Parameters.AddWithValue("@HealthEffects", (object?)h.HealthEffects ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecommendedControls", (object?)h.RecommendedControls ?? DBNull.Value);
            command.Parameters.AddWithValue("@PPERecommendations", (object?)h.PPERecommendations ?? DBNull.Value);
            command.Parameters.AddWithValue("@AtmosphericMonitoring", (object?)h.AtmosphericMonitoring ?? DBNull.Value);
            command.Parameters.AddWithValue("@ErgonomicFactors", (object?)h.ErgonomicFactors ?? DBNull.Value);
            command.Parameters.AddWithValue("@SourceUrl", (object?)h.SourceUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProfileHash", hash);

            var resultId = (Guid?)command.ExecuteScalar() ?? newId;
            hazardIds[key] = resultId;

            if (changeType is not null)
            {
                changedHazardKeys.Add(key);
                if (changeType == "New") newCount++; else revisedCount++;

                using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
                logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
                logCmd.Parameters.AddWithValue("@HazardProfileId", resultId);
                logCmd.Parameters.AddWithValue("@ChemicalName", h.ChemicalName);
                logCmd.Parameters.AddWithValue("@ChangeType", changeType);
                logCmd.Parameters.AddWithValue("@PreviousHash", (object?)prior?.ProfileHash ?? DBNull.Value);
                logCmd.Parameters.AddWithValue("@NewHash", hash);
                logCmd.Parameters.AddWithValue("@Summary", $"{h.ChemicalName}: {changeType} (seed sync)");
                logCmd.ExecuteNonQuery();
            }
            else
            {
                unchangedCount++;
            }
        }

        // Anything previously imported but absent from this run's seed set: soft-handle it.
        // NIOSHHazardProfile has no Status column (unlike OSHAStandard's Rescinded flow), so we
        // do NOT mutate the row -- we only log the removal for visibility/audit.
        foreach (var (key, prior) in existing)
        {
            if (seenKeys.Contains(key)) continue;

            using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
            logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
            logCmd.Parameters.AddWithValue("@HazardProfileId", prior.HazardProfileId);
            logCmd.Parameters.AddWithValue("@ChemicalName", prior.ChemicalName);
            logCmd.Parameters.AddWithValue("@ChangeType", "Removed");
            logCmd.Parameters.AddWithValue("@PreviousHash", (object?)prior.ProfileHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@NewHash", (object?)prior.ProfileHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@Summary", $"{prior.ChemicalName}: no longer present in seed source (Removed, row retained)");
            logCmd.ExecuteNonQuery();

            removedCount++;
        }

        transaction.Commit();
        return new NioshDiffResult(hazardIds, changedHazardKeys, newCount, revisedCount, unchangedCount, removedCount);
    }

    // ------------------------------------------------------------------------------------------
    // Phase 4: child-row replace (respirator matrix / PPE guidance) for changed hazards only.
    // ------------------------------------------------------------------------------------------

    private static void ReplaceChildRows(string connectionString, List<ParsedHazardProfile> hazards, NioshDiffResult diff)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string deleteRespiratorSql = "DELETE FROM [Compliance].[NIOSHRespiratorMatrix] WHERE HazardId = @HazardProfileId;";
        const string deletePpeSql = "DELETE FROM [Compliance].[NIOSHPPEGuidance] WHERE HazardId = @HazardProfileId;";
        const string insertRespiratorSql = """
            INSERT INTO [Compliance].[NIOSHRespiratorMatrix]
                (MatrixId, HazardId, HazardType, ConcentrationRange, RecommendedRespirator, Notes)
            VALUES
                (@RespiratorMatrixId, @HazardProfileId, @HazardType, @ConcentrationRange, @RecommendedRespirator, @Notes);
            """;
        const string insertPpeSql = """
            INSERT INTO [Compliance].[NIOSHPPEGuidance]
                (GuidanceId, HazardId, PPEType, PerformanceCriteria, Limitations)
            VALUES
                (@PPEGuidanceId, @HazardProfileId, @PPEType, @PerformanceCriteria, @Limitations);
            """;

        var regenerated = 0;
        var respiratorRows = 0;
        var ppeRows = 0;
        foreach (var h in hazards)
        {
            var key = KeyFor(h);
            if (!diff.HazardIds.TryGetValue(key, out var hazardId)) continue;
            if (!diff.ChangedHazardKeys.Contains(key)) continue;
            regenerated++;

            using (var del = new SqlCommand(deleteRespiratorSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@HazardProfileId", hazardId);
                del.ExecuteNonQuery();
            }
            using (var del = new SqlCommand(deletePpeSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@HazardProfileId", hazardId);
                del.ExecuteNonQuery();
            }

            foreach (var m in h.RespiratorMatrix)
            {
                using var insert = new SqlCommand(insertRespiratorSql, connection, transaction);
                insert.Parameters.AddWithValue("@RespiratorMatrixId", Guid.NewGuid());
                insert.Parameters.AddWithValue("@HazardProfileId", hazardId);
                insert.Parameters.AddWithValue("@HazardType", m.HazardType);
                insert.Parameters.AddWithValue("@ConcentrationRange", m.ConcentrationRange);
                insert.Parameters.AddWithValue("@RecommendedRespirator", m.RecommendedRespirator);
                insert.Parameters.AddWithValue("@Notes", (object?)m.Notes ?? DBNull.Value);
                insert.ExecuteNonQuery();
                respiratorRows++;
            }

            foreach (var g in h.PPEGuidance)
            {
                using var insert = new SqlCommand(insertPpeSql, connection, transaction);
                insert.Parameters.AddWithValue("@PPEGuidanceId", Guid.NewGuid());
                insert.Parameters.AddWithValue("@HazardProfileId", hazardId);
                insert.Parameters.AddWithValue("@PPEType", g.PPEType);
                insert.Parameters.AddWithValue("@PerformanceCriteria", g.PerformanceCriteria);
                insert.Parameters.AddWithValue("@Limitations", (object?)g.Limitations ?? DBNull.Value);
                insert.ExecuteNonQuery();
                ppeRows++;
            }
        }

        transaction.Commit();
        Console.WriteLine($"Regenerated child rows for {regenerated} changed hazards ({respiratorRows} respirator-matrix rows, {ppeRows} PPE-guidance rows).");
    }

    // ------------------------------------------------------------------------------------------
    // Phase 5: NIOSH hazard -> PlantSafety module mapping.
    //
    // Deterministic rule table, keyed primarily by the hazard's HazardCategory (an explicit
    // seed-input signal), with a keyword fallback over HealthEffects text for any hazard whose
    // HazardCategory doesn't match a known rule -- so the mapping degrades gracefully for a
    // future real data source that may not carry a clean category field. Every mapping row
    // written carries the MappingRuleCode/Rationale of the exact rule that produced it.
    //
    // Rules (per the user-specified mapping):
    //   Respiratory hazards            -> primary PPE, secondary OperationalSafety, EnvironmentalHealth
    //   Chemical hazards               -> primary HazardousWasteManagement, secondary EnvironmentalHealth
    //   Ergonomic hazards              -> primary Ergonomics, secondary OperationalSafety
    //   Confined space / atmospheric   -> primary PermitToWork, secondary EnvironmentalHealth
    //   Biological hazards             -> primary EmployeeSafety, secondary PPE
    // ------------------------------------------------------------------------------------------

    private sealed record ModuleRule(string PrimaryModule, string RuleCode, string Rationale, string[] SecondaryModules, string Severity);

    private static readonly Dictionary<string, ModuleRule> CategoryRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Respiratory"] = new(
            "PPE", "HazardCategory.Respiratory",
            "Respiratory hazard: inhalation exposure control is primarily a personal-protective-equipment concern, with operational and environmental-health overlays.",
            ["OperationalSafety", "EnvironmentalHealth"], "High"),
        ["Chemical"] = new(
            "HazardousWasteManagement", "HazardCategory.Chemical",
            "Chemical hazard: handling, storage, and disposal of the hazardous substance is HazardousWasteManagement's domain, with an environmental-health exposure overlay.",
            ["EnvironmentalHealth"], "High"),
        ["Ergonomic"] = new(
            "Ergonomics", "HazardCategory.Ergonomic",
            "Ergonomic hazard: musculoskeletal risk from posture/repetition/force is Ergonomics' domain, with an operational-safety overlay for task/process design.",
            ["OperationalSafety"], "Medium"),
        ["ConfinedSpaceAtmospheric"] = new(
            "PermitToWork", "HazardCategory.ConfinedSpaceAtmospheric",
            "Confined space / atmospheric hazard: entry requires permit-controlled work procedures, with an environmental-health atmospheric-monitoring overlay.",
            ["EnvironmentalHealth"], "Critical"),
        ["Biological"] = new(
            "EmployeeSafety", "HazardCategory.Biological",
            "Biological hazard: pathogen exposure is a direct employee-health-protection concern, with a PPE overlay for barrier protection equipment.",
            ["PPE"], "High"),
    };

    // Fallback keyword signals over HealthEffects text, used only when HazardCategory doesn't
    // match a rule above -- keeps the mapping working for future non-seed data sources that may
    // not carry a clean category field.
    private static readonly (string Keyword, string Category)[] HealthEffectsKeywordFallback =
    [
        ("respir", "Respiratory"),
        ("inhal", "Respiratory"),
        ("lung", "Respiratory"),
        ("musculoskeletal", "Ergonomic"),
        ("repetitive", "Ergonomic"),
        ("posture", "Ergonomic"),
        ("pathogen", "Biological"),
        ("infect", "Biological"),
        ("bloodborne", "Biological"),
        ("confined space", "ConfinedSpaceAtmospheric"),
        ("atmospher", "ConfinedSpaceAtmospheric"),
        ("asphyx", "ConfinedSpaceAtmospheric"),
    ];

    private static readonly ModuleRule UnmappedCategoryFallback = new(
        "HazardousWasteManagement", "HazardCategory.Unmapped.Fallback",
        "No HazardCategory/keyword rule matched this hazard; routed to hazardous-substance handling pending an explicit rule.",
        [], "Medium");

    private static ModuleRule ResolveModuleRule(string hazardCategory, string? healthEffects)
    {
        if (CategoryRules.TryGetValue(hazardCategory, out var rule)) return rule;

        if (!string.IsNullOrEmpty(healthEffects))
        {
            var lower = healthEffects.ToLowerInvariant();
            foreach (var (keyword, category) in HealthEffectsKeywordFallback)
            {
                if (lower.Contains(keyword) && CategoryRules.TryGetValue(category, out var fallbackRule))
                    return fallbackRule;
            }
        }

        return UnmappedCategoryFallback;
    }

    private static void ApplyModuleMapping(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Re-derive the mapping for every current hazard profile (not just unmapped ones): the
        // rule table is small and deterministic, and this keeps the demo/seed tool simple. Delete
        // existing mapping rows for a hazard before reinserting -- same delete-then-reinsert
        // pattern as the respirator/PPE child rows, and mapping rows are deleted before nothing
        // else depends on them (no further FK-dependent child table beneath the mapping table).
        const string selectHazardsSql = """
            SELECT HazardId, ChemicalName, HealthEffects
            FROM [Compliance].[NIOSHHazardProfile];
            """;

        var hazardCategoryByChemical = LoadHazardCategoriesFromSeed();

        var targets = new List<(Guid HazardProfileId, string ChemicalName, string? HealthEffects)>();
        using (var selectCmd = new SqlCommand(selectHazardsSql, connection))
        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                targets.Add((
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)));
            }
        }

        if (targets.Count == 0)
        {
            Console.WriteLine("No hazard profiles found; nothing to map.");
            return;
        }

        const string deleteMappingSql = "DELETE FROM [Compliance].[NIOSHHazardModuleMapping] WHERE HazardId = @HazardProfileId;";
        const string insertMappingSql = """
            INSERT INTO [Compliance].[NIOSHHazardModuleMapping]
                (MappingId, HazardId, Module, IsPrimary, Severity, MappingRuleCode, Rationale)
            VALUES
                (@MappingId, @HazardProfileId, @Module, @IsPrimary, @Severity, @MappingRuleCode, @Rationale);
            """;

        using var transaction = connection.BeginTransaction();
        var mappingRowCount = 0;
        var hazardCount = 0;
        foreach (var target in targets)
        {
            var category = hazardCategoryByChemical.GetValueOrDefault(target.ChemicalName, "General");
            var rule = ResolveModuleRule(category, target.HealthEffects);

            using (var del = new SqlCommand(deleteMappingSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@HazardProfileId", target.HazardProfileId);
                del.ExecuteNonQuery();
            }

            using (var insertCmd = new SqlCommand(insertMappingSql, connection, transaction))
            {
                insertCmd.Parameters.AddWithValue("@MappingId", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@HazardProfileId", target.HazardProfileId);
                insertCmd.Parameters.AddWithValue("@Module", rule.PrimaryModule);
                insertCmd.Parameters.AddWithValue("@IsPrimary", true);
                insertCmd.Parameters.AddWithValue("@Severity", rule.Severity);
                insertCmd.Parameters.AddWithValue("@MappingRuleCode", rule.RuleCode);
                insertCmd.Parameters.AddWithValue("@Rationale", rule.Rationale);
                insertCmd.ExecuteNonQuery();
                mappingRowCount++;
            }

            foreach (var secondaryModule in rule.SecondaryModules)
            {
                using var insertCmd = new SqlCommand(insertMappingSql, connection, transaction);
                insertCmd.Parameters.AddWithValue("@MappingId", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@HazardProfileId", target.HazardProfileId);
                insertCmd.Parameters.AddWithValue("@Module", secondaryModule);
                insertCmd.Parameters.AddWithValue("@IsPrimary", false);
                insertCmd.Parameters.AddWithValue("@Severity", rule.Severity);
                insertCmd.Parameters.AddWithValue("@MappingRuleCode", rule.RuleCode);
                insertCmd.Parameters.AddWithValue("@Rationale", rule.Rationale);
                insertCmd.ExecuteNonQuery();
                mappingRowCount++;
            }

            hazardCount++;
        }

        transaction.Commit();
        Console.WriteLine($"Mapped {hazardCount} hazard profiles ({mappingRowCount} mapping rows, {mappingRowCount - hazardCount} secondary).");
    }

    // Module mapping needs each hazard's HazardCategory, which lives in the seed input, not in
    // NIOSHHazardProfile itself (the table mirrors only the columns the module reads; HazardCategory
    // is a mapping-time signal, not stored reference data). Re-reading the seed here keeps Phase 5
    // able to run standalone against whatever hazard profiles are currently in the table.
    private static Dictionary<string, string> LoadHazardCategoriesFromSeed()
    {
        var baseDir = AppContext.BaseDirectory;
        var defaultSeedFile = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "data", "niosh-hazards.seed.json"));
        if (!File.Exists(defaultSeedFile)) return new Dictionary<string, string>();

        var json = File.ReadAllText(defaultSeedFile);
        var records = JsonSerializer.Deserialize<List<SeedHazardRecord>>(json, JsonOptions) ?? [];
        return records
            .Where(r => !string.IsNullOrWhiteSpace(r.ChemicalName))
            .ToDictionary(r => r.ChemicalName!.Trim(), r => r.HazardCategory?.Trim() ?? "General");
    }

    // ------------------------------------------------------------------------------------------
    // Seed JSON DTOs (Phase 1 only) and internal parsed model (Phases 2-5).
    // ------------------------------------------------------------------------------------------

    private sealed class SeedHazardRecord
    {
        public string? ChemicalName { get; set; }
        public string? CasNumber { get; set; }
        public string? HazardCategory { get; set; }
        public string? RelTwa { get; set; }
        public string? RelStel { get; set; }
        public string? HealthEffects { get; set; }
        public string? RecommendedControls { get; set; }
        public string? PpeRecommendations { get; set; }
        public string? AtmosphericMonitoring { get; set; }
        public string? ErgonomicFactors { get; set; }
        public string? SourceUrl { get; set; }
        public List<SeedRespiratorMatrixRow>? RespiratorMatrix { get; set; }
        public List<SeedPpeGuidanceRow>? PpeGuidance { get; set; }
    }

    private sealed class SeedRespiratorMatrixRow
    {
        public string HazardType { get; set; } = string.Empty;
        public string ConcentrationRange { get; set; } = string.Empty;
        public string RecommendedRespirator { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    private sealed class SeedPpeGuidanceRow
    {
        public string PPEType { get; set; } = string.Empty;
        public string PerformanceCriteria { get; set; } = string.Empty;
        public string? Limitations { get; set; }
    }

    private sealed record ParsedHazardProfile(
        string ChemicalName,
        string? CASNumber,
        string HazardCategory,
        double? REL_TWA,
        double? REL_STEL,
        string? HealthEffects,
        string? RecommendedControls,
        string? PPERecommendations,
        string? AtmosphericMonitoring,
        string? ErgonomicFactors,
        string? SourceUrl,
        List<ParsedRespiratorMatrixRow> RespiratorMatrix,
        List<ParsedPpeGuidanceRow> PPEGuidance);

    private sealed record ParsedRespiratorMatrixRow(string HazardType, string ConcentrationRange, string RecommendedRespirator, string? Notes);

    private sealed record ParsedPpeGuidanceRow(string PPEType, string PerformanceCriteria, string? Limitations);

    private sealed record ExistingHazard(Guid HazardProfileId, string ChemicalName, string? CASNumber, string? ProfileHash);

    private sealed record NioshDiffResult(
        Dictionary<string, Guid> HazardIds,
        HashSet<string> ChangedHazardKeys,
        int NewCount,
        int RevisedCount,
        int UnchangedCount,
        int RemovedCount);
}
