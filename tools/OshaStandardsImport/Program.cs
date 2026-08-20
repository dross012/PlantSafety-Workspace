// Standalone, one-off import tool. Not part of PlantSafety.sln and not referenced by any
// PlantSafety module -- it only writes to Compliance.OSHAStandard / Compliance.OSHARequirement /
// Compliance.OSHARequirementModuleMapping in PlantSafety.CoreDB via the same schema the module
// code will read from.
//
// Pipeline: download eCFR Title 29 bulk XML -> parse OSHA Parts 1900-1999 -> run the
// Compliance/Sql/Tables DDL -> upsert Standards -> derive Requirements -> map Requirements to
// PlantSafety modules (Phase 6).
//
// Phase 6's mapping ruleset (Part/Subpart -> primary module, RequirementType -> secondary
// module overlay, RequirementType -> Severity) is deliberately explicit, inspectable C# data,
// not a black box -- every mapping written carries the RuleCode/Rationale of the exact rule
// that produced it. It only names PlantSafety modules that actually exist as top-level
// PlantSafety.Infrastructure folders in this repository (verified 2026-08-07); it never invents
// a module name. Where a genuine "Training" module folder exists but is a stub
// (PlantSafety.Infrastructure/Training/TrainingStub.cs) with the real, end-to-end training data
// living in PlantSafety.Infrastructure/Operational/Training instead, Training-type requirements
// are mapped to "Operational" -- the same choice WorkerTrainingFactsProvider already made, for
// the same reason: map to where the real data lives, not to the name that merely sounds right.
//
// Usage: dotnet run -- [--skip-download] [--connection-string "..."]

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace OshaStandardsImport;

internal static class Program
{
    private const string SourceUrl = "https://www.govinfo.gov/bulkdata/ECFR/title-29/ECFR-title29.xml";
    private const int TitleNumber = 29;
    private const int OshaPartMin = 1900;
    private const int OshaPartMax = 1999;

    private static readonly Regex DateRegex = new(
        @"(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}",
        RegexOptions.Compiled);

    private static readonly Regex HeadNumberRegex = new(@"^\S+\s+[\d.\-a-zA-Z]+\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex GoBatchSplitRegex = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
    private static readonly Regex RequirementSentenceRegex = new(@"\b(must|shall|required|prohibited)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SentenceSplitRegex = new(@"(?<=[.!?])\s+(?=[A-Z(§])", RegexOptions.Compiled);

    private static readonly (string Keyword, string RequirementType)[] TypeKeywords =
    [
        ("personal protective equipment", "PPE"),
        (" ppe ", "PPE"),
        ("train", "Training"),
        ("inspect", "Inspection"),
        ("record", "Recordkeeping"),
        ("report", "Reporting"),
        ("procedure", "Procedure"),
        ("permit", "Permit"),
    ];

    private static async Task Main(string[] args)
    {
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", ".."));
        var dataDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "data"));
        Directory.CreateDirectory(dataDir);
        var xmlPath = Path.Combine(dataDir, "ECFR-title29.xml");

        var skipDownload = args.Contains("--skip-download");
        var connectionString = GetArg(args, "--connection-string")
            ?? "Server=localhost\\PLANTSAFETY;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        Console.WriteLine("=== PHASE 1: Download CFR Title 29 XML ===");
        await DownloadXmlAsync(SourceUrl, xmlPath, skipDownload);
        ValidateXml(xmlPath);

        Console.WriteLine("=== PHASE 2: Parse OSHA Parts (1900-1999) ===");
        var standards = ParseOshaStandards(xmlPath, OshaPartMin, OshaPartMax);
        Console.WriteLine($"Parsed {standards.Count} sections across {standards.Select(s => s.PartNumber).Distinct().Count()} parts.");

        Console.WriteLine("=== PHASE 3: Build/verify OSHA schema (Compliance.OSHAStandard / Compliance.OSHARequirement / Compliance.OSHAStandardChangeLog / Compliance.OSHARequirementModuleMapping) ===");
        var ddlDir = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Compliance", "Sql", "Tables");
        RunDdlScript(connectionString, Path.Combine(ddlDir, "OSHAStandard.sql"));
        RunDdlScript(connectionString, Path.Combine(ddlDir, "OSHARequirement.sql"));
        RunDdlScript(connectionString, Path.Combine(ddlDir, "OSHAStandardChangeLog.sql"));
        RunDdlScript(connectionString, Path.Combine(ddlDir, "OSHARequirementModuleMapping.sql"));

        Console.WriteLine("=== PHASE 4: Diff against existing data and upsert changed Standards ===");
        var diff = UpsertStandards(connectionString, standards);
        Console.WriteLine($"New: {diff.NewCount}, Revised: {diff.RevisedCount}, Unchanged: {diff.UnchangedCount}, Rescinded: {diff.RescindedCount}");

        Console.WriteLine("=== PHASE 5: Regenerate derived Requirements for changed Standards only ===");
        InsertRequirements(connectionString, standards, diff);

        Console.WriteLine("=== PHASE 6: Map Requirements to PlantSafety modules ===");
        ApplyModuleMapping(connectionString);

        Console.WriteLine("=== DONE ===");
    }

    private static string? GetArg(string[] args, string name)
    {
        var idx = Array.IndexOf(args, name);
        return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
    }

    private static async Task DownloadXmlAsync(string url, string path, bool skipDownload)
    {
        if (skipDownload && File.Exists(path))
        {
            Console.WriteLine($"Skipping download; using existing file at {path}");
            return;
        }

        Console.WriteLine($"Downloading {url} ...");
        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var fileStream = File.Create(path);
        await response.Content.CopyToAsync(fileStream);
        Console.WriteLine($"Saved {new FileInfo(path).Length:N0} bytes to {path}");
    }

    private static void ValidateXml(string path)
    {
        var doc = XDocument.Load(path);
        if (doc.Root?.Name.LocalName != "DLPSTEXTCLASS")
            throw new InvalidDataException($"Unexpected root element '{doc.Root?.Name}'; expected DLPSTEXTCLASS.");

        var titleNode = doc.Descendants("IDNO").FirstOrDefault(e => (string?)e.Attribute("TYPE") == "title");
        if (titleNode is null || titleNode.Value.Trim() != "29")
            throw new InvalidDataException("XML does not declare Title 29.");

        var hasOshaPart = doc.Descendants("DIV5")
            .Any(d => (string?)d.Attribute("TYPE") == "PART" && int.TryParse((string?)d.Attribute("N"), out var n) && n is >= OshaPartMin and <= OshaPartMax);
        if (!hasOshaPart)
            throw new InvalidDataException("XML does not contain any OSHA parts (1900-1999).");

        Console.WriteLine("XML validated: well-formed, Title 29, contains OSHA parts.");
    }

    private static List<ParsedStandard> ParseOshaStandards(string xmlPath, int partMin, int partMax)
    {
        var doc = XDocument.Load(xmlPath);
        var results = new List<ParsedStandard>();

        var partDivs = doc.Descendants("DIV5")
            .Where(d => (string?)d.Attribute("TYPE") == "PART"
                        && int.TryParse((string?)d.Attribute("N"), out var n)
                        && n >= partMin && n <= partMax);

        foreach (var partDiv in partDivs)
        {
            var partNumber = int.Parse((string?)partDiv.Attribute("N") ?? "0", CultureInfo.InvariantCulture);
            var partTitle = CleanText(partDiv.Element("HEAD")?.Value);
            WalkPart(partDiv, partNumber, partTitle, subpart: null, subpartTitle: null, results);
        }

        return results;
    }

    private static void WalkPart(XElement element, int partNumber, string? partTitle, string? subpart, string? subpartTitle, List<ParsedStandard> results)
    {
        foreach (var child in element.Elements())
        {
            var type = (string?)child.Attribute("TYPE");

            if (child.Name.LocalName == "DIV6" && type == "SUBPART")
            {
                var childSubpart = (string?)child.Attribute("N");
                var childSubpartTitle = CleanText(child.Element("HEAD")?.Value);
                WalkPart(child, partNumber, partTitle, childSubpart, childSubpartTitle, results);
            }
            else if (type == "SECTION")
            {
                results.Add(ExtractSection(child, partNumber, partTitle, subpart, subpartTitle));
            }
            else
            {
                WalkPart(child, partNumber, partTitle, subpart, subpartTitle, results);
            }
        }
    }

    private static ParsedStandard ExtractSection(XElement sectionDiv, int partNumber, string? partTitle, string? subpart, string? subpartTitle)
    {
        var head = CleanText(sectionDiv.Element("HEAD")?.Value) ?? string.Empty;
        var sectionNumber = ((string?)sectionDiv.Attribute("N") ?? head).Trim();

        string? sectionTitle = null;
        var headMatch = HeadNumberRegex.Match(head);
        if (headMatch.Success)
            sectionTitle = headMatch.Groups[1].Value.Trim().TrimEnd('.');

        var paragraphs = sectionDiv.Descendants("P").Select(p => CleanText(p.Value) ?? string.Empty).Where(t => t.Length > 0);
        var sectionText = string.Join("\n", paragraphs).Trim();

        var citation = sectionDiv.Descendants("CITA").Select(c => c.Value).FirstOrDefault();
        DateOnly? effectiveDate = null;
        if (!string.IsNullOrEmpty(citation))
        {
            var matches = DateRegex.Matches(citation);
            if (matches.Count > 0 && DateOnly.TryParse(matches[^1].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                effectiveDate = parsed;
        }

        var status = sectionText.Contains("[Reserved]", StringComparison.OrdinalIgnoreCase) || head.Contains("[Reserved]", StringComparison.OrdinalIgnoreCase)
            ? "Reserved"
            : "Active";

        if (sectionText.Length == 0)
            sectionText = head;

        return new ParsedStandard(partNumber, partTitle, subpart, subpartTitle, sectionNumber, sectionTitle, sectionText, effectiveDate, status);
    }

    private static string? CleanText(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return WhitespaceRegex.Replace(raw, " ").Trim();
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

    private static Dictionary<(int PartNumber, string Section), ExistingStandard> LoadExistingStandards(SqlConnection connection, SqlTransaction transaction, int partMin, int partMax)
    {
        var existing = new Dictionary<(int, string), ExistingStandard>();
        const string sql = """
            SELECT OSHAStandardId, PartNumber, Section, Status, SectionTextHash
            FROM [Compliance].[OSHAStandard]
            WHERE PartNumber BETWEEN @PartMin AND @PartMax;
            """;
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@PartMin", partMin);
        command.Parameters.AddWithValue("@PartMax", partMax);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var partNumber = reader.GetInt32(1);
            var section = reader.GetString(2);
            existing[(partNumber, section)] = new ExistingStandard(
                reader.GetGuid(0),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4));
        }
        return existing;
    }

    private static DiffResult UpsertStandards(string connectionString, List<ParsedStandard> standards)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = LoadExistingStandards(connection, transaction, OshaPartMin, OshaPartMax);
        var standardIds = new Dictionary<(int, string), Guid>();
        var changedStandardIds = new HashSet<Guid>();
        var seenKeys = new HashSet<(int, string)>();
        int newCount = 0, revisedCount = 0, unchangedCount = 0, rescindedCount = 0;

        const string upsertSql = """
            MERGE [Compliance].[OSHAStandard] AS target
            USING (SELECT @Section AS Section, @PartNumber AS PartNumber) AS source
            ON target.Section = source.Section AND target.PartNumber = source.PartNumber
            WHEN MATCHED THEN UPDATE SET
                TitleNumber = @TitleNumber, PartTitle = @PartTitle, Subpart = @Subpart, SubpartTitle = @SubpartTitle,
                SectionTitle = @SectionTitle, SectionText = @SectionText, EffectiveDate = @EffectiveDate,
                Status = @Status, SourceUrl = @SourceUrl, SectionTextHash = @SectionTextHash, UpdatedAtUtc = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                (OSHAStandardId, TitleNumber, PartNumber, PartTitle, Subpart, SubpartTitle, Section, SectionTitle, SectionText, EffectiveDate, Status, SourceUrl, SectionTextHash)
                VALUES (@OSHAStandardId, @TitleNumber, @PartNumber, @PartTitle, @Subpart, @SubpartTitle, @Section, @SectionTitle, @SectionText, @EffectiveDate, @Status, @SourceUrl, @SectionTextHash)
            OUTPUT inserted.OSHAStandardId;
            """;
        const string changeLogSql = """
            INSERT INTO [Compliance].[OSHAStandardChangeLog]
                (ChangeLogId, OSHAStandardId, PartNumber, Section, ChangeType, PreviousStatus, NewStatus, PreviousTextHash, NewTextHash, Summary)
            VALUES
                (@ChangeLogId, @OSHAStandardId, @PartNumber, @Section, @ChangeType, @PreviousStatus, @NewStatus, @PreviousTextHash, @NewTextHash, @Summary);
            """;

        var count = 0;
        foreach (var s in standards)
        {
            var key = (s.PartNumber, s.Section);
            seenKeys.Add(key);
            var hash = ComputeHash(s.SectionText);
            existing.TryGetValue(key, out var prior);

            var changeType = prior is null ? "New" : prior.SectionTextHash != hash ? "Revised" : null;

            using var command = new SqlCommand(upsertSql, connection, transaction);
            var newId = Guid.NewGuid();
            command.Parameters.AddWithValue("@OSHAStandardId", newId);
            command.Parameters.AddWithValue("@TitleNumber", TitleNumber);
            command.Parameters.AddWithValue("@PartNumber", s.PartNumber);
            command.Parameters.AddWithValue("@PartTitle", (object?)s.PartTitle ?? DBNull.Value);
            command.Parameters.AddWithValue("@Subpart", (object?)s.Subpart ?? DBNull.Value);
            command.Parameters.AddWithValue("@SubpartTitle", (object?)s.SubpartTitle ?? DBNull.Value);
            command.Parameters.AddWithValue("@Section", s.Section);
            command.Parameters.AddWithValue("@SectionTitle", (object?)s.SectionTitle ?? DBNull.Value);
            command.Parameters.AddWithValue("@SectionText", s.SectionText);
            command.Parameters.AddWithValue("@EffectiveDate", (object?)s.EffectiveDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", s.Status);
            command.Parameters.AddWithValue("@SourceUrl", SourceUrl);
            command.Parameters.AddWithValue("@SectionTextHash", hash);

            var resultId = (Guid?)command.ExecuteScalar() ?? newId;
            standardIds[key] = resultId;

            if (changeType is not null)
            {
                changedStandardIds.Add(resultId);
                if (changeType == "New") newCount++; else revisedCount++;

                using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
                logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
                logCmd.Parameters.AddWithValue("@OSHAStandardId", resultId);
                logCmd.Parameters.AddWithValue("@PartNumber", s.PartNumber);
                logCmd.Parameters.AddWithValue("@Section", s.Section);
                logCmd.Parameters.AddWithValue("@ChangeType", changeType);
                logCmd.Parameters.AddWithValue("@PreviousStatus", (object?)prior?.Status ?? DBNull.Value);
                logCmd.Parameters.AddWithValue("@NewStatus", s.Status);
                logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior?.SectionTextHash ?? DBNull.Value);
                logCmd.Parameters.AddWithValue("@NewTextHash", hash);
                logCmd.Parameters.AddWithValue("@Summary", $"{s.Section}: {changeType} (eCFR sync)");
                logCmd.ExecuteNonQuery();
            }
            else
            {
                unchangedCount++;
            }

            count++;
            if (count % 200 == 0) Console.WriteLine($"  processed {count}/{standards.Count} standards...");
        }

        const string rescindSql = """
            UPDATE [Compliance].[OSHAStandard] SET Status = 'Rescinded', UpdatedAtUtc = SYSUTCDATETIME()
            WHERE OSHAStandardId = @OSHAStandardId;
            """;
        foreach (var (key, prior) in existing)
        {
            if (seenKeys.Contains(key) || prior.Status == "Rescinded") continue;

            using (var rescind = new SqlCommand(rescindSql, connection, transaction))
            {
                rescind.Parameters.AddWithValue("@OSHAStandardId", prior.OSHAStandardId);
                rescind.ExecuteNonQuery();
            }

            using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
            logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
            logCmd.Parameters.AddWithValue("@OSHAStandardId", prior.OSHAStandardId);
            logCmd.Parameters.AddWithValue("@PartNumber", key.Item1);
            logCmd.Parameters.AddWithValue("@Section", key.Item2);
            logCmd.Parameters.AddWithValue("@ChangeType", "Rescinded");
            logCmd.Parameters.AddWithValue("@PreviousStatus", prior.Status);
            logCmd.Parameters.AddWithValue("@NewStatus", "Rescinded");
            logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@NewTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@Summary", $"{key.Item2}: no longer present in eCFR source (Rescinded)");
            logCmd.ExecuteNonQuery();

            rescindedCount++;
        }

        transaction.Commit();
        return new DiffResult(standardIds, changedStandardIds, newCount, revisedCount, unchangedCount, rescindedCount);
    }

    private static void InsertRequirements(string connectionString, List<ParsedStandard> standards, DiffResult diff)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string deleteMappingsSql = """
            DELETE m FROM [Compliance].[OSHARequirementModuleMapping] m
            JOIN [Compliance].[OSHARequirement] r ON r.OSHARequirementId = m.OSHARequirementId
            WHERE r.OSHAStandardId = @OSHAStandardId;
            """;
        const string deleteSql = "DELETE FROM [Compliance].[OSHARequirement] WHERE OSHAStandardId = @OSHAStandardId;";
        const string insertSql = """
            INSERT INTO [Compliance].[OSHARequirement] (OSHARequirementId, OSHAStandardId, RequirementType, AppliesToModule, Severity, Notes)
            VALUES (@OSHARequirementId, @OSHAStandardId, @RequirementType, NULL, NULL, @Notes);
            """;

        using var transaction = connection.BeginTransaction();
        var totalRequirements = 0;
        var regenerated = 0;
        foreach (var s in standards)
        {
            if (!diff.StandardIds.TryGetValue((s.PartNumber, s.Section), out var standardId)) continue;
            if (!diff.ChangedStandardIds.Contains(standardId)) continue;
            regenerated++;

            // Module mappings reference OSHARequirementId by FK -- must be cleared before the
            // requirement rows they point to are deleted below, or the delete violates the FK.
            using (var delMappings = new SqlCommand(deleteMappingsSql, connection, transaction))
            {
                delMappings.Parameters.AddWithValue("@OSHAStandardId", standardId);
                delMappings.ExecuteNonQuery();
            }

            using (var del = new SqlCommand(deleteSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@OSHAStandardId", standardId);
                del.ExecuteNonQuery();
            }

            foreach (var sentence in SentenceSplitRegex.Split(s.SectionText))
            {
                var trimmed = sentence.Trim();
                if (trimmed.Length == 0 || !RequirementSentenceRegex.IsMatch(trimmed)) continue;

                var lower = " " + trimmed.ToLowerInvariant() + " ";
                var requirementType = TypeKeywords.FirstOrDefault(k => lower.Contains(k.Keyword)).RequirementType ?? "General";

                using var insert = new SqlCommand(insertSql, connection, transaction);
                insert.Parameters.AddWithValue("@OSHARequirementId", Guid.NewGuid());
                insert.Parameters.AddWithValue("@OSHAStandardId", standardId);
                insert.Parameters.AddWithValue("@RequirementType", requirementType);
                insert.Parameters.AddWithValue("@Notes", trimmed.Length > 4000 ? trimmed[..4000] : trimmed);
                insert.ExecuteNonQuery();
                totalRequirements++;
            }
        }
        transaction.Commit();
        Console.WriteLine($"Regenerated requirements for {regenerated} changed standards ({totalRequirements} requirement rows).");
    }

    // ------------------------------------------------------------------------------------------
    // Phase 6: OSHA requirement -> PlantSafety module mapping.
    //
    // Two-tier deterministic ruleset:
    //   1) Primary module, from the requirement's CFR location: a Part+Subpart rule if one
    //      exists (only defined for Parts 1910/1926, the two parts with real subpart-level
    //      structure spanning multiple safety domains), else a Part-level default rule.
    //   2) Secondary module(s), from the requirement's already-derived RequirementType
    //      (PPE/Training/Inspection/Recordkeeping/Reporting/Procedure/Permit) -- added only
    //      when the RequirementType's module differs from the primary, so e.g. a hot-work
    //      section's "permit"-flavoured sentence gets tagged PermitToWork even when its
    //      Part/Subpart primary is MachineSafety.
    //
    // Severity is assigned per RequirementType (same value on every mapping row for a given
    // requirement, since it describes the requirement's own text, not the module relationship).
    //
    // Every module name below is a real top-level PlantSafety.Infrastructure folder in this
    // repository (verified 2026-08-07) -- see the file-header note on why Training-type
    // requirements map to "Operational", not "Training".
    // ------------------------------------------------------------------------------------------

    private sealed record ModuleRule(string Module, string RuleCode, string Rationale);

    // Subpart-level overrides, Part 1910 (General Industry).
    private static readonly Dictionary<string, ModuleRule> Subpart1910Rules = new()
    {
        ["A"] = new("SafetyPolicies", "Part.1910.SubpartA", "Subpart A - General: scope/purpose/definitions govern the whole part; treated as policy, not an operational control."),
        ["B"] = new("Governance", "Part.1910.SubpartB", "Subpart B - Adoption and Extension of Established Federal Standards: administrative rulemaking history, not an operational control."),
        ["D"] = new("OperationalSafety", "Part.1910.SubpartD", "Subpart D - Walking-Working Surfaces: general facility operational hazard control."),
        ["E"] = new("SafetyProcedures", "Part.1910.SubpartE", "Subpart E - Exit Routes and Emergency Planning: emergency action/fire prevention plans are documented safety procedures."),
        ["F"] = new("MachineSafety", "Part.1910.SubpartF", "Subpart F - Powered Platforms, Manlifts, and Vehicle-Mounted Work Platforms: powered equipment."),
        ["G"] = new("EnvironmentalHealth", "Part.1910.SubpartG", "Subpart G - Occupational Health and Environmental Control: noise/ventilation/exposure limits."),
        ["H"] = new("HazardousWasteManagement", "Part.1910.SubpartH", "Subpart H - Hazardous Materials: flammable/combustible/explosive materials handling and storage."),
        ["I"] = new("PPE", "Part.1910.SubpartI", "Subpart I - Personal Protective Equipment: direct PPE requirements."),
        ["J"] = new("EnvironmentalHealth", "Part.1910.SubpartJ", "Subpart J - General Environmental Controls: sanitation, permit-required confined spaces (environmental hazard control)."),
        ["K"] = new("EmployeeSafety", "Part.1910.SubpartK", "Subpart K - Medical and First Aid: employee health/medical response."),
        ["L"] = new("OperationalSafety", "Part.1910.SubpartL", "Subpart L - Fire Protection: no dedicated fire-protection module exists; general operational hazard control."),
        ["M"] = new("MachineSafety", "Part.1910.SubpartM", "Subpart M - Compressed Gas and Compressed Air Equipment: pressurized equipment."),
        ["N"] = new("LogisticsTransportSafety", "Part.1910.SubpartN", "Subpart N - Materials Handling and Storage: material handling/storage/transport."),
        ["O"] = new("MachineSafety", "Part.1910.SubpartO", "Subpart O - Machinery and Machine Guarding: direct machine-guarding requirements."),
        ["P"] = new("MachineSafety", "Part.1910.SubpartP", "Subpart P - Hand and Portable Powered Tools: powered/hand tool equipment safety."),
        ["Q"] = new("PermitToWork", "Part.1910.SubpartQ", "Subpart Q - Welding, Cutting and Brazing: hot work is permit-controlled work."),
        ["R"] = new("OperationalSafety", "Part.1910.SubpartR", "Subpart R - Special Industries: industry-specific operational hazard control."),
        ["S"] = new("MachineSafety", "Part.1910.SubpartS", "Subpart S - Electrical: electrical equipment/systems safety."),
        ["T"] = new("OperationalSafety", "Part.1910.SubpartT", "Subpart T - Commercial Diving Operations: specialized operational hazard control."),
        ["U"] = new("EmployeeSafety", "Part.1910.SubpartU", "Subpart U - COVID-19: direct employee health protection."),
    };

    // Subpart-level overrides, Part 1926 (Construction).
    private static readonly Dictionary<string, ModuleRule> Subpart1926Rules = new()
    {
        ["A"] = new("SafetyPolicies", "Part.1926.SubpartA", "Subpart A - General: scope/purpose/applicability governs the whole part."),
        ["AA"] = new("PermitToWork", "Part.1926.SubpartAA", "Subpart AA - Confined Spaces in Construction: confined-space entry is permit-controlled work."),
        ["B"] = new("Governance", "Part.1926.SubpartB", "Subpart B - General Interpretations: interpretive/administrative, not an operational control."),
        ["C"] = new("OperationalSafety", "Part.1926.SubpartC", "Subpart C - General Safety and Health Provisions: general jobsite operational hazard control."),
        ["CC"] = new("MachineSafety", "Part.1926.SubpartCC", "Subpart CC - Cranes and Derricks in Construction: powered lifting equipment."),
        ["D"] = new("EnvironmentalHealth", "Part.1926.SubpartD", "Subpart D - Occupational Health and Environmental Controls: exposure/ventilation/sanitation."),
        ["E"] = new("PPE", "Part.1926.SubpartE", "Subpart E - Personal Protective and Life Saving Equipment: direct PPE requirements."),
        ["F"] = new("OperationalSafety", "Part.1926.SubpartF", "Subpart F - Fire Protection and Prevention: no dedicated fire-protection module exists."),
        ["G"] = new("PhysicalAccessControl", "Part.1926.SubpartG", "Subpart G - Signs, Signals, and Barricades: jobsite access/hazard demarcation."),
        ["H"] = new("LogisticsTransportSafety", "Part.1926.SubpartH", "Subpart H - Materials Handling, Storage, Use, and Disposal: material handling/transport."),
        ["I"] = new("MachineSafety", "Part.1926.SubpartI", "Subpart I - Tools-Hand and Power: powered/hand tool equipment safety."),
        ["J"] = new("PermitToWork", "Part.1926.SubpartJ", "Subpart J - Welding and Cutting: hot work is permit-controlled work."),
        ["K"] = new("MachineSafety", "Part.1926.SubpartK", "Subpart K - Electrical: electrical equipment/systems safety."),
        ["L"] = new("OperationalSafety", "Part.1926.SubpartL", "Subpart L - Scaffolds: elevated-work operational hazard control."),
        ["M"] = new("PPE", "Part.1926.SubpartM", "Subpart M - Fall Protection: fall-arrest/restraint systems are personal protective equipment."),
        ["N"] = new("MachineSafety", "Part.1926.SubpartN", "Subpart N - Helicopters, Hoists, Elevators, and Conveyors: powered lifting/moving equipment."),
        ["O"] = new("LogisticsTransportSafety", "Part.1926.SubpartO", "Subpart O - Motor Vehicles, Mechanized Equipment, and Marine Operations: vehicle/equipment transport."),
        ["P"] = new("PermitToWork", "Part.1926.SubpartP", "Subpart P - Excavations: excavation/trenching is permit-controlled work."),
        ["Q"] = new("OperationalSafety", "Part.1926.SubpartQ", "Subpart Q - Concrete and Masonry Construction: trade-specific operational hazard control."),
        ["R"] = new("OperationalSafety", "Part.1926.SubpartR", "Subpart R - Steel Erection: trade-specific operational hazard control."),
        ["S"] = new("PermitToWork", "Part.1926.SubpartS", "Subpart S - Underground Construction, Caissons, Cofferdams and Compressed Air: confined/hazardous-atmosphere work is permit-controlled."),
        ["T"] = new("PermitToWork", "Part.1926.SubpartT", "Subpart T - Demolition: demolition is permit-controlled work."),
        ["U"] = new("PermitToWork", "Part.1926.SubpartU", "Subpart U - Blasting and the Use of Explosives: blasting is permit-controlled work."),
        ["V"] = new("MachineSafety", "Part.1926.SubpartV", "Subpart V - Electric Power Transmission and Distribution: electrical equipment/systems safety."),
        ["W"] = new("MachineSafety", "Part.1926.SubpartW", "Subpart W - Rollover Protective Structures; Overhead Protection: mobile-equipment protective structures."),
        ["X"] = new("OperationalSafety", "Part.1926.SubpartX", "Subpart X - Stairways and Ladders: general jobsite operational hazard control."),
        ["Y"] = new("OperationalSafety", "Part.1926.SubpartY", "Subpart Y - Diving: specialized operational hazard control."),
        ["Z"] = new("HazardousWasteManagement", "Part.1926.SubpartZ", "Subpart Z - Toxic and Hazardous Substances: hazardous substance exposure control."),
    };

    // Part-level default/fallback rules -- used for every part without subpart-level rules
    // above, and for any 1910/1926 section whose Subpart isn't in the tables above.
    private static readonly Dictionary<int, ModuleRule> PartDefaultRules = new()
    {
        [1902] = new("Governance", "Part.1902.Default", "State Plans for the Development and Enforcement of State Standards: intergovernmental administrative process."),
        [1903] = new("Auditing", "Part.1903.Default", "Inspections, Citations and Proposed Penalties: OSHA's own inspection/enforcement process maps to internal audit posture."),
        [1904] = new("AccidentManagement", "Part.1904.Default", "Recording and Reporting Occupational Injuries and Illnesses: direct incident recordkeeping requirement."),
        [1905] = new("Governance", "Part.1905.Default", "Rules of Practice for Variances, Limitations, Variations, Tolerances, and Exemptions: administrative process."),
        [1908] = new("Governance", "Part.1908.Default", "Consultation Agreements: administrative/programmatic, not an operational control."),
        [1911] = new("Governance", "Part.1911.Default", "Rules of Procedure for Promulgating, Modifying, or Revoking Standards: rulemaking process."),
        [1912] = new("Governance", "Part.1912.Default", "Advisory Committees on Standards: administrative/programmatic."),
        [1910] = new("HazardousWasteManagement", "Part.1910.Default", "Sections outside any 1910 Subpart A-U rule fall in the 1910.1000+ series (Subpart Z - Toxic and Hazardous Substances; not represented as a distinct Subpart in the parsed eCFR bulk XML) -- air contaminants, carcinogens, asbestos, and other substance-specific exposure standards."),
        [1913] = new("EmployeeSafety", "Part.1913.Default", "Rules of Agency Practice Concerning OSHA Access to Employee Medical Records: employee health-record handling."),
        [1915] = new("LogisticsTransportSafety", "Part.1915.Default", "Occupational Safety and Health Standards for Shipyard Employment: maritime industry operational safety."),
        [1917] = new("LogisticsTransportSafety", "Part.1917.Default", "Marine Terminals: maritime cargo/terminal operational safety."),
        [1918] = new("LogisticsTransportSafety", "Part.1918.Default", "Safety and Health Regulations for Longshoring: maritime cargo-handling operational safety."),
        [1919] = new("MachineSafety", "Part.1919.Default", "Gear Certification: rigging/lifting-gear equipment certification."),
        [1920] = new("Governance", "Part.1920.Default", "Procedure for Variations under the Longshoremen's and Harbor Workers' Compensation Act: administrative process."),
        [1921] = new("Governance", "Part.1921.Default", "Rules of Practice in Enforcement Proceedings (LHWCA sec. 41): administrative/enforcement process."),
        [1922] = new("Governance", "Part.1922.Default", "Investigational Hearings (LHWCA sec. 41): administrative process."),
        [1924] = new("EmployeeSafety", "Part.1924.Default", "Safety Standards for Grant-Assisted Workshops and Rehabilitation Facilities: employee health/safety standards."),
        [1925] = new("ContractorSafety", "Part.1925.Default", "Safety and Health Standards for Federal Service Contracts: contractor-performed work safety standards."),
        [1928] = new("OperationalSafety", "Part.1928.Default", "Occupational Safety and Health Standards for Agriculture: industry-specific operational hazard control."),
        [1949] = new("Operational", "Part.1949.Default", "Office of Training and Education: training-program administration (see Operational.Training)."),
        [1952] = new("Governance", "Part.1952.Default", "Approved State Plans for Enforcement of State Standards: intergovernmental administrative process."),
        [1953] = new("Governance", "Part.1953.Default", "Changes to State Plans: intergovernmental administrative process."),
        [1954] = new("Governance", "Part.1954.Default", "Procedures for the Evaluation and Monitoring of Approved State Plans: intergovernmental administrative process."),
        [1955] = new("Governance", "Part.1955.Default", "Procedures for Withdrawal of Approval of State Plans: intergovernmental administrative process."),
        [1956] = new("Governance", "Part.1956.Default", "State Plans for State/Local Government Employees: intergovernmental administrative process."),
        [1960] = new("Governance", "Part.1960.Default", "Basic Program Elements for Federal Employee OSH Programs: federal-employer program administration."),
        [1975] = new("Governance", "Part.1975.Default", "Coverage of Employers under the OSH Act: jurisdictional/administrative determination."),
        [1977] = new("Governance", "Part.1977.Default", "Discrimination Against Employees Exercising OSH Act Rights: whistleblower/retaliation complaint process."),
        [1978] = new("Governance", "Part.1978.Default", "Retaliation Complaints under the Surface Transportation Assistance Act: whistleblower complaint process."),
        [1979] = new("Governance", "Part.1979.Default", "Discrimination Complaints under the AIR21 Act: whistleblower complaint process."),
        [1980] = new("Governance", "Part.1980.Default", "Retaliation Complaints under the Sarbanes-Oxley Act: whistleblower complaint process."),
        [1981] = new("Governance", "Part.1981.Default", "Discrimination Complaints under the Pipeline Safety Improvement Act: whistleblower complaint process."),
        [1982] = new("Governance", "Part.1982.Default", "Retaliation Complaints under the National Transit Systems Security/Federal Railroad Safety Acts: whistleblower complaint process."),
        [1983] = new("Governance", "Part.1983.Default", "Retaliation Complaints under the Consumer Product Safety Improvement Act: whistleblower complaint process."),
        [1984] = new("Governance", "Part.1984.Default", "Retaliation Complaints under the Affordable Care Act: whistleblower complaint process."),
        [1985] = new("Governance", "Part.1985.Default", "Retaliation Complaints under the Consumer Financial Protection Act: whistleblower complaint process."),
        [1986] = new("Governance", "Part.1986.Default", "Retaliation Complaints under the Seaman's Protection Act: whistleblower complaint process."),
        [1987] = new("Governance", "Part.1987.Default", "Retaliation Complaints under the FDA Food Safety Modernization Act: whistleblower complaint process."),
        [1988] = new("Governance", "Part.1988.Default", "Retaliation Complaints under MAP-21: whistleblower complaint process."),
        [1989] = new("Governance", "Part.1989.Default", "Retaliation Complaints under the Taxpayer First Act: whistleblower complaint process."),
        [1990] = new("EnvironmentalHealth", "Part.1990.Default", "Identification, Classification, and Regulation of Potential Occupational Carcinogens: chemical exposure hazard control."),
        [1991] = new("Governance", "Part.1991.Default", "Retaliation Complaints under the Criminal Antitrust Anti-Retaliation Act: whistleblower complaint process."),
        [1992] = new("Governance", "Part.1992.Default", "Retaliation Complaints under the Anti-Money Laundering Act: whistleblower complaint process."),
    };

    // Fallback when neither a subpart nor a part rule matches (should only occur for OSHA parts
    // outside the 44 already covered above, e.g. if the eCFR source adds a new Part 19xx later).
    private static readonly ModuleRule UnmappedPartFallback = new("OperationalSafety", "Part.Unmapped.Fallback", "No Part/Subpart rule defined for this CFR part; routed to general operational safety pending an explicit rule.");

    // Named-section overrides for standards whose regulatory intent the blunt Subpart/Part rules
    // above get wrong -- known exceptions, not guesses. Checked before Subpart/Part rules.
    private static readonly Dictionary<string, ModuleRule> SectionOverrideRules = new()
    {
        ["§ 1910.147"] = new("MachineSafety", "Section.1910.147", "The Control of Hazardous Energy (Lockout/Tagout): isolating machine energy sources before service/maintenance is a machine-safety control first, even though this section sits in Subpart J. Its own RequirementType.Permit overlay still adds PermitToWork as a secondary module."),
        ["§ 1910.146"] = new("PermitToWork", "Section.1910.146", "Permit-Required Confined Spaces: the section's own title names the permit requirement directly."),
        ["§ 1910.1030"] = new("EmployeeSafety", "Section.1910.1030", "Bloodborne Pathogens: direct employee exposure-control/health-protection program, not a materials/waste-handling standard."),
        ["§ 1926.1101"] = new("EmployeeSafety", "Section.1926.1101", "Asbestos (construction): direct employee exposure-control/health-protection program, distinct from bulk hazardous-material handling."),
    };

    // Secondary module overlay, keyed by the RequirementType already derived in Phase 5 --
    // added alongside the primary Part/Subpart mapping when it names a different module.
    private static readonly Dictionary<string, ModuleRule> RequirementTypeSecondaryRules = new()
    {
        ["Training"] = new("Operational", "RequirementType.Training", "Training-flavoured requirement text; real training records live in Operational.Training (see WorkerTrainingFactsProvider)."),
        ["Recordkeeping"] = new("AccidentManagement", "RequirementType.Recordkeeping", "Recordkeeping-flavoured requirement text; incident/injury recordkeeping is AccidentManagement's domain."),
        ["Reporting"] = new("AccidentManagement", "RequirementType.Reporting", "Reporting-flavoured requirement text; incident/injury reporting is AccidentManagement's domain."),
        ["Inspection"] = new("Auditing", "RequirementType.Inspection", "Inspection-flavoured requirement text; inspection activity maps to internal audit posture."),
        ["Permit"] = new("PermitToWork", "RequirementType.Permit", "Permit-flavoured requirement text; permit-controlled work is PermitToWork's domain regardless of CFR location."),
        ["PPE"] = new("PPE", "RequirementType.PPE", "PPE-flavoured requirement text; direct personal protective equipment requirement."),
    };

    // Severity per RequirementType (ComplianceSeverity codes: Advisory/Low/Medium/High/Critical
    // -- see PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity). Permit-
    // controlled and PPE-flavoured requirements are rated High: failure to obtain a required
    // permit or wear required PPE is a direct, immediate injury risk. Inspection/Training/
    // Reporting/Procedure are Medium: process controls whose failure is a leading indicator, not
    // itself an immediate injury event. Recordkeeping/General are Low: paperwork/administrative
    // in nature. This is a deterministic business rule, not a fact extracted from CFR text --
    // the CFR does not itself grade section severity.
    private static readonly Dictionary<string, string> SeverityByRequirementType = new()
    {
        ["Permit"] = "High",
        ["PPE"] = "High",
        ["Inspection"] = "Medium",
        ["Training"] = "Medium",
        ["Reporting"] = "Medium",
        ["Procedure"] = "Medium",
        ["Recordkeeping"] = "Low",
        ["General"] = "Low",
    };

    private static ModuleRule ResolvePrimaryRule(int partNumber, string? subpart, string section)
    {
        if (SectionOverrideRules.TryGetValue(section, out var sectionRule)) return sectionRule;

        if (subpart is not null)
        {
            if (partNumber == 1910 && Subpart1910Rules.TryGetValue(subpart, out var rule1910)) return rule1910;
            if (partNumber == 1926 && Subpart1926Rules.TryGetValue(subpart, out var rule1926)) return rule1926;
        }

        return PartDefaultRules.TryGetValue(partNumber, out var partRule) ? partRule : UnmappedPartFallback;
    }

    private static void ApplyModuleMapping(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string selectUnmappedSql = """
            SELECT r.OSHARequirementId, r.RequirementType, s.PartNumber, s.Subpart, s.Section
            FROM [Compliance].[OSHARequirement] r
            JOIN [Compliance].[OSHAStandard] s ON s.OSHAStandardId = r.OSHAStandardId
            WHERE NOT EXISTS (SELECT 1 FROM [Compliance].[OSHARequirementModuleMapping] m WHERE m.OSHARequirementId = r.OSHARequirementId);
            """;

        var targets = new List<(Guid OSHARequirementId, string? RequirementType, int PartNumber, string? Subpart, string Section)>();
        using (var selectCmd = new SqlCommand(selectUnmappedSql, connection))
        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                targets.Add((
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? null : reader.GetString(1),
                    reader.GetInt32(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.GetString(4)));
            }
        }

        if (targets.Count == 0)
        {
            Console.WriteLine("No unmapped requirements found; nothing to map.");
            return;
        }

        const string insertMappingSql = """
            INSERT INTO [Compliance].[OSHARequirementModuleMapping]
                (MappingId, OSHARequirementId, Module, IsPrimary, Severity, MappingRuleCode, Rationale)
            VALUES
                (@MappingId, @OSHARequirementId, @Module, @IsPrimary, @Severity, @MappingRuleCode, @Rationale);
            """;
        const string updateRequirementSql = """
            UPDATE [Compliance].[OSHARequirement] SET AppliesToModule = @Module, Severity = @Severity
            WHERE OSHARequirementId = @OSHARequirementId;
            """;

        using var transaction = connection.BeginTransaction();
        var mappingRowCount = 0;
        var requirementCount = 0;
        foreach (var target in targets)
        {
            var requirementType = target.RequirementType ?? "General";
            var severity = SeverityByRequirementType.TryGetValue(requirementType, out var sev) ? sev : "Low";
            var primary = ResolvePrimaryRule(target.PartNumber, target.Subpart, target.Section);

            var mappings = new List<ModuleRule> { primary };
            if (RequirementTypeSecondaryRules.TryGetValue(requirementType, out var secondary) && secondary.Module != primary.Module)
            {
                mappings.Add(secondary);
            }

            for (var i = 0; i < mappings.Count; i++)
            {
                var rule = mappings[i];
                using var insertCmd = new SqlCommand(insertMappingSql, connection, transaction);
                insertCmd.Parameters.AddWithValue("@MappingId", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@OSHARequirementId", target.OSHARequirementId);
                insertCmd.Parameters.AddWithValue("@Module", rule.Module);
                insertCmd.Parameters.AddWithValue("@IsPrimary", i == 0);
                insertCmd.Parameters.AddWithValue("@Severity", severity);
                insertCmd.Parameters.AddWithValue("@MappingRuleCode", rule.RuleCode);
                insertCmd.Parameters.AddWithValue("@Rationale", rule.Rationale);
                insertCmd.ExecuteNonQuery();
                mappingRowCount++;
            }

            using (var updateCmd = new SqlCommand(updateRequirementSql, connection, transaction))
            {
                updateCmd.Parameters.AddWithValue("@Module", primary.Module);
                updateCmd.Parameters.AddWithValue("@Severity", severity);
                updateCmd.Parameters.AddWithValue("@OSHARequirementId", target.OSHARequirementId);
                updateCmd.ExecuteNonQuery();
            }

            requirementCount++;
            if (requirementCount % 2000 == 0) Console.WriteLine($"  mapped {requirementCount}/{targets.Count} requirements...");
        }
        transaction.Commit();
        Console.WriteLine($"Mapped {requirementCount} requirements ({mappingRowCount} mapping rows, {mappingRowCount - requirementCount} secondary).");
    }

    private sealed record ParsedStandard(
        int PartNumber,
        string? PartTitle,
        string? Subpart,
        string? SubpartTitle,
        string Section,
        string? SectionTitle,
        string SectionText,
        DateOnly? EffectiveDate,
        string Status);

    private sealed record ExistingStandard(Guid OSHAStandardId, string Status, string? SectionTextHash);

    private sealed record DiffResult(
        Dictionary<(int PartNumber, string Section), Guid> StandardIds,
        HashSet<Guid> ChangedStandardIds,
        int NewCount,
        int RevisedCount,
        int UnchangedCount,
        int RescindedCount);
}
