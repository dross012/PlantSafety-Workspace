// Standalone, one-off import tool. Not part of PlantSafety.sln and not referenced by any
// PlantSafety module -- it only writes to Compliance.NFPA_HazardProfile /
// Compliance.NFPA_HazardChangeLog / Compliance.NFPA_ControlMatrix /
// Compliance.NFPA_HazardModuleMapping in PlantSafety.CoreDB via the same schema the module
// code will read from.
//
// CRITICAL: NFPA (National Fire Protection Association) is not a government agency and does
// not publish machine-readable open regulatory data the way eCFR does for OSHA. NFPA code text
// is copyrighted. This tool NEVER downloads, scrapes, or fabricates real NFPA code text. It
// ingests exclusively from local mock/placeholder JSON files under SampleData/, each record
// clearly labelled "PLACEHOLDER" in every guidance field, representing a hypothetical NFPA
// reference-data feed pending a real licensed NFPA data-source integration. See README.md.
//
// Pipeline: load mock SampleData/*.json -> parse into hazard-profile records -> run the
// Compliance/Sql/Tables + Auditing/Sql/Tables DDL -> upsert HazardProfiles -> replace
// ControlMatrix rows for changed/new hazards -> map hazards to PlantSafety modules (Phase 6).
//
// Phase 6's mapping ruleset (CodeNumber -> primary module(s), FlammableMaterialsGuidance ->
// secondary overlay, fire-protection wording -> secondary overlay, ControlType -> secondary
// module overlay, ControlType -> Severity) is deliberately explicit, inspectable C# data, not a
// black box -- every mapping row written carries the MappingRuleCode/Rationale of the exact rule
// that produced it. It only names PlantSafety modules that actually exist as top-level
// PlantSafety.Infrastructure folders in this repository (verified 2026-08-07); it never invents
// a module name. Two rule-requested module names do not exist as real folders and are
// substituted -- see the file-header note in ApplyModuleMapping's rule tables below for the
// substitution and its rationale (same convention OSHA's tool already uses for CFR subparts with
// no dedicated fire-protection module).
//
// Usage: dotnet run -- [--skip-download] [--connection-string "..."]

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace NfpaReferenceImport;

internal static class Program
{
    // Extensible codes list -- add another (CodeNumber, FileName) pair here to bring a new NFPA
    // code into scope; nothing else in this file hardcodes "70" or "51B" logic.
    private static readonly (string CodeNumber, string FileName)[] SourceFiles =
    [
        ("70", "nfpa70-hazards.json"),
        ("51B", "nfpa51b-hazards.json"),
    ];

    private static readonly Regex GoBatchSplitRegex = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static readonly Regex FireProtectionRegex = new(@"fire\s+protection", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly HashSet<string> ValidSeverities =
        new(StringComparer.OrdinalIgnoreCase) { "Advisory", "Low", "Medium", "High", "Critical" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static async Task Main(string[] args)
    {
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", ".."));
        var sampleDataDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "SampleData"));

        var skipDownload = args.Contains("--skip-download");
        var connectionString = GetArg(args, "--connection-string")
            ?? "Server=localhost\\PLANTSAFETY;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        Console.WriteLine("=== PHASE 1: Load mock NFPA reference-data input files (SampleData/) ===");
        if (skipDownload)
        {
            // There is no network call for this tool -- NFPA text must never be fetched or
            // scraped. The flag is kept for command-line parity with OshaStandardsImport; here
            // it is a documented no-op.
            Console.WriteLine("--skip-download has no effect for this tool (no network source exists); reading local SampleData/*.json as usual.");
        }
        var rawRecords = LoadMockInput(sampleDataDir, SourceFiles);
        Console.WriteLine($"Loaded {rawRecords.Count} mock hazard-profile records across {SourceFiles.Length} NFPA code file(s).");

        Console.WriteLine("=== PHASE 2: Parse mock input into internal hazard-profile model ===");
        var hazards = ParseHazardRecords(rawRecords);
        Console.WriteLine($"Parsed {hazards.Count} hazard profiles across {hazards.Select(h => h.CodeNumber).Distinct().Count()} NFPA code(s).");

        Console.WriteLine("=== PHASE 3: Build/verify NFPA schema (Compliance.NFPA_HazardProfile / NFPA_HazardChangeLog / NFPA_ControlMatrix / NFPA_HazardModuleMapping / Auditing.AuditFindingNfpaCoverage) ===");
        var complianceDdlDir = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Compliance", "Sql", "Tables");
        var auditingDdlDir = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Auditing", "Sql", "Tables");
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "NFPA_HazardProfile.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "NFPA_HazardChangeLog.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "NFPA_ControlMatrix.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "NFPA_HazardModuleMapping.sql"));
        RunDdlScript(connectionString, Path.Combine(auditingDdlDir, "AuditFindingNfpaCoverage.sql"));

        Console.WriteLine("=== PHASE 4: Diff against existing data and upsert changed HazardProfiles ===");
        var diff = UpsertHazardProfiles(connectionString, hazards);
        Console.WriteLine($"New: {diff.NewCount}, Revised: {diff.RevisedCount}, Unchanged: {diff.UnchangedCount}, Rescinded: {diff.RescindedCount}");

        Console.WriteLine("=== PHASE 5: Replace ControlMatrix rows for changed/new hazards only ===");
        ReplaceControlMatrixRows(connectionString, hazards, diff);

        Console.WriteLine("=== PHASE 6: Map hazards to PlantSafety modules ===");
        ApplyModuleMapping(connectionString);

        Console.WriteLine("=== DONE ===");
    }

    private static string? GetArg(string[] args, string name)
    {
        var idx = Array.IndexOf(args, name);
        return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
    }

    // ------------------------------------------------------------------------------------------
    // Phases 1-2: load + parse mock input
    // ------------------------------------------------------------------------------------------

    private static List<(string CodeNumber, string FileName, MockHazardRecord Record)> LoadMockInput(
        string sampleDataDir, (string CodeNumber, string FileName)[] sourceFiles)
    {
        var results = new List<(string, string, MockHazardRecord)>();
        foreach (var (codeNumber, fileName) in sourceFiles)
        {
            var path = Path.Combine(sampleDataDir, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Mock NFPA input file not found for code {codeNumber}: {path}", path);

            var json = File.ReadAllText(path);
            var records = JsonSerializer.Deserialize<List<MockHazardRecord>>(json, JsonOptions)
                ?? throw new InvalidDataException($"Failed to deserialize {path} into a hazard-profile array.");

            foreach (var record in records)
            {
                results.Add((codeNumber, fileName, record));
            }

            Console.WriteLine($"  {fileName}: {records.Count} record(s) for NFPA {codeNumber}.");
        }
        return results;
    }

    private static List<ParsedHazard> ParseHazardRecords(List<(string CodeNumber, string FileName, MockHazardRecord Record)> rawRecords)
    {
        var results = new List<ParsedHazard>();
        foreach (var (codeNumber, fileName, record) in rawRecords)
        {
            if (string.IsNullOrWhiteSpace(record.CodeNumber) || record.CodeNumber != codeNumber)
                throw new InvalidDataException($"{fileName}: record CodeNumber '{record.CodeNumber}' does not match the expected code '{codeNumber}' for this file.");
            if (string.IsNullOrWhiteSpace(record.SectionNumber))
                throw new InvalidDataException($"{fileName}: record is missing SectionNumber.");
            if (string.IsNullOrWhiteSpace(record.HazardName))
                throw new InvalidDataException($"{fileName}: section {record.SectionNumber} is missing HazardName.");

            var controls = (record.Controls ?? []).Select(c =>
            {
                if (string.IsNullOrWhiteSpace(c.ControlType))
                    throw new InvalidDataException($"{fileName}: section {record.SectionNumber} has a control with no ControlType.");
                if (string.IsNullOrWhiteSpace(c.Severity) || !ValidSeverities.Contains(c.Severity))
                    throw new InvalidDataException($"{fileName}: section {record.SectionNumber} control '{c.ControlType}' has invalid Severity '{c.Severity}' (must be one of Advisory/Low/Medium/High/Critical).");

                return new ParsedControl(c.ControlType.Trim(), c.ControlDescription?.Trim() ?? string.Empty, c.Severity.Trim(), c.Rationale?.Trim() ?? string.Empty);
            }).ToList();

            results.Add(new ParsedHazard(
                record.CodeNumber.Trim(),
                record.SectionNumber.Trim(),
                record.HazardName.Trim(),
                record.HazardDescription?.Trim() ?? string.Empty,
                record.RecommendedControls?.Trim() ?? string.Empty,
                record.PPERecommendations?.Trim() ?? string.Empty,
                record.ElectricalSafetyGuidance?.Trim() ?? string.Empty,
                record.HotWorkGuidance?.Trim() ?? string.Empty,
                record.FlammableMaterialsGuidance?.Trim() ?? string.Empty,
                record.SourceUrl?.Trim() ?? string.Empty,
                controls));
        }
        return results;
    }

    // ------------------------------------------------------------------------------------------
    // Phase 3: DDL runner (identical pattern to OshaStandardsImport.RunDdlScript)
    // ------------------------------------------------------------------------------------------

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

    private static string BuildHashInput(ParsedHazard h) => string.Join("",
        h.HazardDescription, h.RecommendedControls, h.PPERecommendations,
        h.ElectricalSafetyGuidance, h.HotWorkGuidance, h.FlammableMaterialsGuidance);

    // ------------------------------------------------------------------------------------------
    // Phase 4: diff + upsert NFPA_HazardProfile, keyed by (CodeNumber, SectionNumber)
    // ------------------------------------------------------------------------------------------

    private static Dictionary<(string CodeNumber, string SectionNumber), ExistingHazard> LoadExistingHazards(
        SqlConnection connection, SqlTransaction transaction)
    {
        var existing = new Dictionary<(string, string), ExistingHazard>();
        const string sql = """
            SELECT HazardId, CodeNumber, SectionNumber, SectionTextHash
            FROM [Compliance].[NFPA_HazardProfile];
            """;
        using var command = new SqlCommand(sql, connection, transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var codeNumber = reader.GetString(1);
            var sectionNumber = reader.GetString(2);
            existing[(codeNumber, sectionNumber)] = new ExistingHazard(
                reader.GetGuid(0),
                reader.IsDBNull(3) ? null : reader.GetString(3));
        }
        return existing;
    }

    private static DiffResult UpsertHazardProfiles(string connectionString, List<ParsedHazard> hazards)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = LoadExistingHazards(connection, transaction);
        var hazardIds = new Dictionary<(string, string), Guid>();
        var changedHazardIds = new HashSet<Guid>();
        var seenKeys = new HashSet<(string, string)>();
        int newCount = 0, revisedCount = 0, unchangedCount = 0, rescindedCount = 0;

        const string upsertSql = """
            MERGE [Compliance].[NFPA_HazardProfile] AS target
            USING (SELECT @CodeNumber AS CodeNumber, @SectionNumber AS SectionNumber) AS source
            ON target.CodeNumber = source.CodeNumber AND target.SectionNumber = source.SectionNumber
            WHEN MATCHED THEN UPDATE SET
                HazardName = @HazardName, HazardDescription = @HazardDescription,
                RecommendedControls = @RecommendedControls, PPERecommendations = @PPERecommendations,
                ElectricalSafetyGuidance = @ElectricalSafetyGuidance, HotWorkGuidance = @HotWorkGuidance,
                FlammableMaterialsGuidance = @FlammableMaterialsGuidance, SourceUrl = @SourceUrl,
                SectionTextHash = @SectionTextHash
            WHEN NOT MATCHED THEN INSERT
                (HazardId, CodeNumber, SectionNumber, HazardName, HazardDescription, RecommendedControls,
                 PPERecommendations, ElectricalSafetyGuidance, HotWorkGuidance, FlammableMaterialsGuidance,
                 SourceUrl, SectionTextHash)
                VALUES
                (@HazardId, @CodeNumber, @SectionNumber, @HazardName, @HazardDescription, @RecommendedControls,
                 @PPERecommendations, @ElectricalSafetyGuidance, @HotWorkGuidance, @FlammableMaterialsGuidance,
                 @SourceUrl, @SectionTextHash)
            OUTPUT inserted.HazardId;
            """;
        const string changeLogSql = """
            INSERT INTO [Compliance].[NFPA_HazardChangeLog]
                (ChangeLogId, HazardId, CodeNumber, SectionNumber, ChangeType, PreviousTextHash, NewTextHash, Summary)
            VALUES
                (@ChangeLogId, @HazardId, @CodeNumber, @SectionNumber, @ChangeType, @PreviousTextHash, @NewTextHash, @Summary);
            """;

        foreach (var h in hazards)
        {
            var key = (h.CodeNumber, h.SectionNumber);
            seenKeys.Add(key);
            var hash = ComputeHash(BuildHashInput(h));
            existing.TryGetValue(key, out var prior);

            var changeType = prior is null ? "New" : prior.SectionTextHash != hash ? "Revised" : null;

            using var command = new SqlCommand(upsertSql, connection, transaction);
            var newId = Guid.NewGuid();
            command.Parameters.AddWithValue("@HazardId", newId);
            command.Parameters.AddWithValue("@CodeNumber", h.CodeNumber);
            command.Parameters.AddWithValue("@SectionNumber", h.SectionNumber);
            command.Parameters.AddWithValue("@HazardName", h.HazardName);
            command.Parameters.AddWithValue("@HazardDescription", (object?)EmptyToNull(h.HazardDescription) ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecommendedControls", (object?)EmptyToNull(h.RecommendedControls) ?? DBNull.Value);
            command.Parameters.AddWithValue("@PPERecommendations", (object?)EmptyToNull(h.PPERecommendations) ?? DBNull.Value);
            command.Parameters.AddWithValue("@ElectricalSafetyGuidance", (object?)EmptyToNull(h.ElectricalSafetyGuidance) ?? DBNull.Value);
            command.Parameters.AddWithValue("@HotWorkGuidance", (object?)EmptyToNull(h.HotWorkGuidance) ?? DBNull.Value);
            command.Parameters.AddWithValue("@FlammableMaterialsGuidance", (object?)EmptyToNull(h.FlammableMaterialsGuidance) ?? DBNull.Value);
            command.Parameters.AddWithValue("@SourceUrl", (object?)EmptyToNull(h.SourceUrl) ?? DBNull.Value);
            command.Parameters.AddWithValue("@SectionTextHash", hash);

            var resultId = (Guid?)command.ExecuteScalar() ?? newId;
            hazardIds[key] = resultId;

            if (changeType is not null)
            {
                changedHazardIds.Add(resultId);
                if (changeType == "New") newCount++; else revisedCount++;

                using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
                logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
                logCmd.Parameters.AddWithValue("@HazardId", resultId);
                logCmd.Parameters.AddWithValue("@CodeNumber", h.CodeNumber);
                logCmd.Parameters.AddWithValue("@SectionNumber", h.SectionNumber);
                logCmd.Parameters.AddWithValue("@ChangeType", changeType);
                logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior?.SectionTextHash ?? DBNull.Value);
                logCmd.Parameters.AddWithValue("@NewTextHash", hash);
                logCmd.Parameters.AddWithValue("@Summary", $"NFPA {h.CodeNumber} {h.SectionNumber}: {changeType} (mock reference-data sync)");
                logCmd.ExecuteNonQuery();
            }
            else
            {
                unchangedCount++;
            }
        }

        // NFPA_HazardProfile has no Status column (unlike OSHAStandard) -- reference profiles are
        // never soft-retired in place. A hazard absent from the current mock input set is recorded
        // as "Rescinded" purely as a NFPA_HazardChangeLog entry so history/alerting can surface it;
        // the profile row itself is left untouched.
        foreach (var (key, prior) in existing)
        {
            if (seenKeys.Contains(key)) continue;

            using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
            logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
            logCmd.Parameters.AddWithValue("@HazardId", prior.HazardId);
            logCmd.Parameters.AddWithValue("@CodeNumber", key.Item1);
            logCmd.Parameters.AddWithValue("@SectionNumber", key.Item2);
            logCmd.Parameters.AddWithValue("@ChangeType", "Rescinded");
            logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@NewTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@Summary", $"NFPA {key.Item1} {key.Item2}: no longer present in mock input set (Rescinded)");
            logCmd.ExecuteNonQuery();

            rescindedCount++;
        }

        transaction.Commit();
        return new DiffResult(hazardIds, changedHazardIds, newCount, revisedCount, unchangedCount, rescindedCount);
    }

    private static string? EmptyToNull(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    // ------------------------------------------------------------------------------------------
    // Phase 5: NFPA_ControlMatrix -- delete + reinsert for changed/new hazards only
    // ------------------------------------------------------------------------------------------

    private static void ReplaceControlMatrixRows(string connectionString, List<ParsedHazard> hazards, DiffResult diff)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        const string deleteSql = "DELETE FROM [Compliance].[NFPA_ControlMatrix] WHERE HazardId = @HazardId;";
        const string insertSql = """
            INSERT INTO [Compliance].[NFPA_ControlMatrix] (MatrixId, HazardId, ControlType, ControlDescription, Severity, Rationale)
            VALUES (@MatrixId, @HazardId, @ControlType, @ControlDescription, @Severity, @Rationale);
            """;

        var totalControls = 0;
        var regenerated = 0;
        foreach (var h in hazards)
        {
            if (!diff.HazardIds.TryGetValue((h.CodeNumber, h.SectionNumber), out var hazardId)) continue;
            if (!diff.ChangedHazardIds.Contains(hazardId)) continue;
            regenerated++;

            using (var del = new SqlCommand(deleteSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@HazardId", hazardId);
                del.ExecuteNonQuery();
            }

            foreach (var c in h.Controls)
            {
                using var insert = new SqlCommand(insertSql, connection, transaction);
                insert.Parameters.AddWithValue("@MatrixId", Guid.NewGuid());
                insert.Parameters.AddWithValue("@HazardId", hazardId);
                insert.Parameters.AddWithValue("@ControlType", c.ControlType);
                insert.Parameters.AddWithValue("@ControlDescription", (object?)EmptyToNull(c.ControlDescription) ?? DBNull.Value);
                insert.Parameters.AddWithValue("@Severity", c.Severity);
                insert.Parameters.AddWithValue("@Rationale", (object?)EmptyToNull(c.Rationale) ?? DBNull.Value);
                insert.ExecuteNonQuery();
                totalControls++;
            }
        }
        transaction.Commit();
        Console.WriteLine($"Regenerated control-matrix rows for {regenerated} changed/new hazards ({totalControls} control rows).");
    }

    // ------------------------------------------------------------------------------------------
    // Phase 6: NFPA hazard -> PlantSafety module mapping.
    //
    // Deterministic, inspectable ruleset (mirrors OshaStandardsImport.ApplyModuleMapping):
    //   1) Primary module(s), from the hazard's NFPA CodeNumber (PrimaryModulesByCode).
    //   2) Secondary overlay: FlammableMaterialsGuidance non-empty -> EnvironmentalHealth,
    //      HazardousWasteManagement (FlammableSecondaryModules).
    //   3) Secondary overlay: HazardName/HazardDescription mentions "fire protection" ->
    //      FacilitySafety, OperationalSafety (FireProtectionSecondaryModules).
    //   4) Secondary overlay: one row per distinct ControlType present on the hazard's controls
    //      (ControlTypeSecondaryModules) -- PPE->PPE, Training->Training, Inspection->the closest
    //      real inspection-related module, Permit->PermitToWork.
    //   Rows are deduplicated by Module (first rule to claim a module wins; primary rules are
    //   evaluated first) so a module named by more than one rule gets exactly one mapping row,
    //   satisfying the UNIQUE (HazardId, Module) constraint and keeping re-runs idempotent.
    //
    // Severity: every row carries a severity from SeverityByControlType, the same style as
    // OSHA's SeverityByRequirementType -- a deterministic business rule, not a fact extracted
    // from the input text. Primary/Flammable/FireProtection rows use the hazard's own
    // "worst control" severity (the highest-ranked SeverityByControlType value among that
    // hazard's controls); rows produced by the ControlType overlay use that specific
    // ControlType's own severity.
    //
    // Two of the module names named in the user's ruleset -- "HotWorkSafety" (NFPA 51B primary)
    // and "FacilitySafety" (fire-protection secondary) -- were checked against the real
    // top-level folders under PlantSafety.Infrastructure (verified 2026-08-07: AccidentManagement,
    // AiGovernance, AiSafetyIntelligence, Alerts, Analytics, Auditing, AutonomousVehicleSafety,
    // BackgroundJobs, Billing, Caching, Checklists, Common, CommunitySafety, Compliance,
    // ContinuousImprovement, ContractorSafety, ContractorVisitor, CrossDomain,
    // CyberPhysicalSecurity, Dashboards, DependencyInjection, DigitalTwin, EmployeeSafety,
    // Enterprise, EnterpriseFabric, EnvironmentalHealth, ExternalServices, Features, FileStorage,
    // Financials, GlobalSafetyInsights, Governance, HazardousWasteManagement, HealthChecks,
    // HumanFactors, Identity, Integrations, Intelligence, LogisticsTransportSafety, MachineSafety,
    // Messaging, Observability, Operational, OperationalSafety, Operations, PPE, PermitToWork,
    // Persistence, PhysicalAccessControl, PlanningProjects, ProcessManagement,
    // PublicSafetyCommunication, RiskManagement, SafetyCulture, SafetyEvents, SafetyPolicies,
    // SafetyProcedures, SafetyPrograms, Scheduling, Scorecards, Search, Sustainability, Tenancy,
    // Training, WorkerWellBeing, Workflow) and NEITHER EXISTS. Both are substituted with
    // "OperationalSafety" -- the same module OSHA's own tool already routes to for CFR subparts
    // that concern fire protection/hot work but have no dedicated fire-protection module
    // (see Subpart1910Rules["L"] and Subpart1926Rules["F"] in OshaStandardsImport/Program.cs).
    // ------------------------------------------------------------------------------------------

    private sealed record ModuleRule(string Module, string RuleCode, string Rationale);

    // Primary modules by NFPA CodeNumber. Add an entry here to bring a new code into scope for
    // Phase 6 without touching any other logic.
    private static readonly Dictionary<string, ModuleRule[]> PrimaryModulesByCode = new()
    {
        ["70"] =
        [
            new("MachineSafety", "Code.70.Primary", "NFPA 70 (National Electrical Code) hazard: electrical equipment is machinery/equipment in the machine-safety sense (guarding, lockout, safe operation)."),
            new("ElectricalSafety", "Code.70.Primary", "NFPA 70 (National Electrical Code) hazard: direct electrical-safety subject matter (the module's namesake domain)."),
            new("OperationalSafety", "Code.70.Primary", "NFPA 70 (National Electrical Code) hazard: general facility operational hazard control for electrical systems."),
        ],
        ["51B"] =
        [
            new("PermitToWork", "Code.51B.Primary", "NFPA 51B (Fire Prevention During Welding, Cutting, and Other Hot Work) hazard: hot work is permit-controlled work."),
            new("OperationalSafety", "Code.51B.Primary", "NFPA 51B hazard: requested primary module 'HotWorkSafety' does not exist as a PlantSafety.Infrastructure folder (verified 2026-08-07); substituted with OperationalSafety, the same fallback OSHA's tool uses for CFR subparts with no dedicated fire-protection module (see Subpart1926Rules[\"F\"])."),
            new("PPE", "Code.51B.Primary", "NFPA 51B hazard: direct personal protective equipment subject matter (flame-resistant clothing, welding PPE)."),
        ],
    };

    // Secondary overlay: any hazard whose FlammableMaterialsGuidance field is non-empty.
    private static readonly ModuleRule[] FlammableSecondaryModules =
    [
        new("EnvironmentalHealth", "Flammable.Secondary", "Hazard names flammable-materials guidance: flammable/combustible material presence is an environmental exposure and release concern."),
        new("HazardousWasteManagement", "Flammable.Secondary", "Hazard names flammable-materials guidance: flammable/combustible materials fall under hazardous-materials handling and storage."),
    ];

    // Secondary overlay: hazard name/description mentions "fire protection".
    private static readonly ModuleRule[] FireProtectionSecondaryModules =
    [
        new("OperationalSafety", "FireProtection.Secondary", "Hazard name/description mentions fire protection: requested module 'FacilitySafety' does not exist as a PlantSafety.Infrastructure folder (verified 2026-08-07); substituted with OperationalSafety, the same fallback OSHA's tool uses for CFR subparts concerning fire protection with no dedicated module (see Subpart1910Rules[\"L\"])."),
    ];

    // Secondary overlay, keyed by ControlType already present on the hazard's controls -- added
    // for every distinct ControlType found, alongside primary/Flammable/FireProtection rows.
    private static readonly Dictionary<string, ModuleRule> ControlTypeSecondaryModules = new()
    {
        ["PPE"] = new("PPE", "ControlType.PPE", "Control-flavoured requirement text; direct personal protective equipment control."),
        ["Training"] = new("Training", "ControlType.Training", "Control-flavoured requirement text; awareness/competency control maps to the Training module."),
        ["Inspection"] = new("Auditing", "ControlType.Inspection", "Control-flavoured requirement text; requested 'relevant inspection-related module' -- Auditing is the closest real PlantSafety.Infrastructure folder for inspection/verification activity (same choice OshaStandardsImport.RequirementTypeSecondaryRules[\"Inspection\"] already makes)."),
        ["Permit"] = new("PermitToWork", "ControlType.Permit", "Control-flavoured requirement text; permit-controlled work is PermitToWork's domain regardless of NFPA code."),
    };

    // Severity per ControlType (ComplianceSeverity codes: Advisory/Low/Medium/High/Critical --
    // mirrors OshaStandardsImport.SeverityByRequirementType). Permit and PPE controls are High:
    // failure to obtain a required hot-work permit or wear required PPE is a direct, immediate
    // injury/ignition risk. Inspection/Training are Medium: process controls whose failure is a
    // leading indicator, not itself an immediate injury event. This is a deterministic business
    // rule, not a fact extracted from the mock input text.
    private static readonly Dictionary<string, string> SeverityByControlType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Permit"] = "High",
        ["PPE"] = "High",
        ["Inspection"] = "Medium",
        ["Training"] = "Medium",
    };

    private const string DefaultSeverity = "Low";
    private static readonly string[] SeverityRankOrder = ["Advisory", "Low", "Medium", "High", "Critical"];

    private static string HighestSeverity(IEnumerable<string> severities)
    {
        var best = DefaultSeverity;
        var bestRank = Array.IndexOf(SeverityRankOrder, best);
        foreach (var s in severities)
        {
            var rank = Array.IndexOf(SeverityRankOrder, s);
            if (rank > bestRank) { best = s; bestRank = rank; }
        }
        return best;
    }

    private static bool MentionsFireProtection(string hazardName, string hazardDescription) =>
        FireProtectionRegex.IsMatch(hazardName) || FireProtectionRegex.IsMatch(hazardDescription);

    private static void ApplyModuleMapping(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string selectUnmappedSql = """
            SELECT h.HazardId, h.CodeNumber, h.HazardName, h.HazardDescription, h.FlammableMaterialsGuidance
            FROM [Compliance].[NFPA_HazardProfile] h
            WHERE NOT EXISTS (SELECT 1 FROM [Compliance].[NFPA_HazardModuleMapping] m WHERE m.HazardId = h.HazardId);
            """;

        var targets = new List<(Guid HazardId, string CodeNumber, string HazardName, string HazardDescription, string FlammableMaterialsGuidance)>();
        using (var selectCmd = new SqlCommand(selectUnmappedSql, connection))
        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                targets.Add((
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4)));
            }
        }

        if (targets.Count == 0)
        {
            Console.WriteLine("No unmapped hazards found; nothing to map.");
            return;
        }

        const string selectControlsSql = "SELECT ControlType FROM [Compliance].[NFPA_ControlMatrix] WHERE HazardId = @HazardId;";
        const string insertMappingSql = """
            INSERT INTO [Compliance].[NFPA_HazardModuleMapping]
                (MappingId, HazardId, ModuleName, Severity, MappingRuleCode, Rationale)
            VALUES
                (@MappingId, @HazardId, @ModuleName, @Severity, @MappingRuleCode, @Rationale);
            """;

        using var transaction = connection.BeginTransaction();
        var mappingRowCount = 0;
        var hazardCount = 0;
        foreach (var target in targets)
        {
            var controlTypes = new List<string>();
            using (var controlsCmd = new SqlCommand(selectControlsSql, connection, transaction))
            {
                controlsCmd.Parameters.AddWithValue("@HazardId", target.HazardId);
                using var controlsReader = controlsCmd.ExecuteReader();
                while (controlsReader.Read()) controlTypes.Add(controlsReader.GetString(0));
            }

            var hazardSeverity = HighestSeverity(
                controlTypes.Select(ct => SeverityByControlType.TryGetValue(ct, out var sev) ? sev : DefaultSeverity));

            // Build candidate mapping rows in precedence order; first rule to claim a Module wins
            // the dedupe below, which is why primary rules run first.
            var candidates = new List<(ModuleRule Rule, bool IsPrimary, string Severity)>();

            if (PrimaryModulesByCode.TryGetValue(target.CodeNumber, out var primaryRules))
            {
                candidates.AddRange(primaryRules.Select(r => (r, true, hazardSeverity)));
            }

            if (!string.IsNullOrWhiteSpace(target.FlammableMaterialsGuidance))
            {
                candidates.AddRange(FlammableSecondaryModules.Select(r => (r, false, hazardSeverity)));
            }

            if (MentionsFireProtection(target.HazardName, target.HazardDescription))
            {
                candidates.AddRange(FireProtectionSecondaryModules.Select(r => (r, false, hazardSeverity)));
            }

            foreach (var controlType in controlTypes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (ControlTypeSecondaryModules.TryGetValue(controlType, out var rule))
                {
                    var severity = SeverityByControlType.TryGetValue(controlType, out var sev) ? sev : DefaultSeverity;
                    candidates.Add((rule, false, severity));
                }
            }

            var byModule = new Dictionary<string, (ModuleRule Rule, bool IsPrimary, string Severity)>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in candidates)
            {
                if (!byModule.ContainsKey(candidate.Rule.Module))
                {
                    byModule[candidate.Rule.Module] = candidate;
                }
            }

            foreach (var (rule, _, severity) in byModule.Values)
            {
                using var insertCmd = new SqlCommand(insertMappingSql, connection, transaction);
                insertCmd.Parameters.AddWithValue("@MappingId", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@HazardId", target.HazardId);
                insertCmd.Parameters.AddWithValue("@ModuleName", rule.Module);
                insertCmd.Parameters.AddWithValue("@Severity", severity);
                insertCmd.Parameters.AddWithValue("@MappingRuleCode", rule.RuleCode);
                insertCmd.Parameters.AddWithValue("@Rationale", rule.Rationale);
                insertCmd.ExecuteNonQuery();
                mappingRowCount++;
            }

            hazardCount++;
            if (hazardCount % 200 == 0) Console.WriteLine($"  mapped {hazardCount}/{targets.Count} hazards...");
        }
        transaction.Commit();
        Console.WriteLine($"Mapped {hazardCount} hazards ({mappingRowCount} mapping rows).");
    }

    // ------------------------------------------------------------------------------------------
    // Models
    // ------------------------------------------------------------------------------------------

    private sealed class MockHazardRecord
    {
        public string CodeNumber { get; set; } = string.Empty;
        public string SectionNumber { get; set; } = string.Empty;
        public string HazardName { get; set; } = string.Empty;
        public string? HazardDescription { get; set; }
        public string? RecommendedControls { get; set; }
        public string? PPERecommendations { get; set; }
        public string? ElectricalSafetyGuidance { get; set; }
        public string? HotWorkGuidance { get; set; }
        public string? FlammableMaterialsGuidance { get; set; }
        public string? SourceUrl { get; set; }
        public List<MockControlRecord>? Controls { get; set; }
    }

    private sealed class MockControlRecord
    {
        public string ControlType { get; set; } = string.Empty;
        public string? ControlDescription { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Rationale { get; set; }
    }

    private sealed record ParsedControl(string ControlType, string ControlDescription, string Severity, string Rationale);

    private sealed record ParsedHazard(
        string CodeNumber,
        string SectionNumber,
        string HazardName,
        string HazardDescription,
        string RecommendedControls,
        string PPERecommendations,
        string ElectricalSafetyGuidance,
        string HotWorkGuidance,
        string FlammableMaterialsGuidance,
        string SourceUrl,
        List<ParsedControl> Controls);

    private sealed record ExistingHazard(Guid HazardId, string? SectionTextHash);

    private sealed record DiffResult(
        Dictionary<(string CodeNumber, string SectionNumber), Guid> HazardIds,
        HashSet<Guid> ChangedHazardIds,
        int NewCount,
        int RevisedCount,
        int UnchangedCount,
        int RescindedCount);
}
