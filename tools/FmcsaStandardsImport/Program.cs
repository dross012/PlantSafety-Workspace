// Standalone, one-off import tool. Not part of PlantSafety.sln and not referenced by any
// PlantSafety module -- it only writes to Compliance.FMCSAStandard / Compliance.FMCSARequirement /
// Compliance.FMCSARequirementModuleMapping in PlantSafety.CoreDB via the same schema the module
// code will read from (schema owned by PlantSafety.Infrastructure/Compliance/Sql/Tables and
// PlantSafety.Infrastructure/Auditing/Sql/Tables, built by a parallel effort -- see the actual
// column names in those .sql files; this tool never inlines duplicate DDL).
//
// Pipeline: download govinfo.gov CFR Title 49 bulk XML -> parse the 8 target FMCSA parts
// (383/390/391/392/393/395/396/397) -> run the Compliance/Sql/Tables + Auditing/Sql/Tables DDL ->
// upsert Standards -> derive Requirements -> map Requirements to PlantSafety modules (Phase 6).
//
// 49 CFR is public federal law published by the Federal Motor Carrier Safety Administration, the
// same legal character as OSHA's 29 CFR -- unlike a prior NFPA effort (NFPA text is copyrighted,
// not government-published, so that tool used mock data), this tool downloads and parses the real
// govinfo.gov XML, the same way OshaStandardsImport does for Title 29.
//
// Phase 6's mapping ruleset (Part -> primary module, Part -> forced secondary module for Parts
// 393/397, RequirementType -> secondary module overlay, RequirementType -> Severity) is
// deliberately explicit, inspectable C# data, not a black box -- every mapping written carries the
// RuleCode/Rationale of the exact rule that produced it. It only names PlantSafety modules that
// actually exist as top-level PlantSafety.Infrastructure folders in this repository (verified
// 2026-08-07: LogisticsTransportSafety, OperationalSafety, MachineSafety, EnvironmentalHealth,
// Auditing, Operational, PermitToWork, PPE).
//
// Usage: dotnet run -- [--skip-download] [--connection-string "..."]

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace FmcsaStandardsImport;

internal static class Program
{
    private const string SourceUrl = "https://www.govinfo.gov/bulkdata/ECFR/title-49/ECFR-title49.xml";
    private const int TitleNumber = 49;

    // Extensible allow-list of 49 CFR parts in scope -- not a hardcoded single-part/range check.
    // Adding a part to this array is the only change needed to bring it into scope for parsing,
    // diffing, requirement derivation, and module mapping.
    private static readonly int[] TargetParts =
    [
        383, // Commercial Driver's License (CDL) Standards
        390, // FMCSA General Applicability and Definitions
        391, // Driver Qualification
        392, // Driving of Commercial Motor Vehicles
        393, // Parts and Accessories Necessary for Safe Operation (vehicle safety standards)
        395, // Hours of Service of Drivers
        396, // Inspection, Repair, and Maintenance
        397, // Transportation of Hazardous Materials, Driving and Parking Rules (shared with PHMSA)
    ];

    private static readonly HashSet<int> TargetPartSet = [.. TargetParts];

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
        var xmlPath = Path.Combine(dataDir, "ECFR-title49.xml");

        var skipDownload = args.Contains("--skip-download");
        var connectionString = GetArg(args, "--connection-string")
            ?? "Server=localhost\\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

        Console.WriteLine("=== PHASE 1: Download CFR Title 49 XML ===");
        await DownloadXmlAsync(SourceUrl, xmlPath, skipDownload);
        ValidateXml(xmlPath);

        Console.WriteLine($"=== PHASE 2: Parse FMCSA Parts ({string.Join(", ", TargetParts)}) ===");
        var standards = ParseFmcsaStandards(xmlPath, TargetPartSet);
        Console.WriteLine($"Parsed {standards.Count} sections across {standards.Select(s => s.PartNumber).Distinct().Count()} parts.");

        Console.WriteLine("=== PHASE 3: Build/verify FMCSA schema (Compliance.FMCSAStandard / Compliance.FMCSARequirement / Compliance.FMCSAStandardChangeLog / Compliance.FMCSARequirementModuleMapping / Auditing.AuditFindingFmcsaCoverage) ===");
        var complianceDdlDir = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Compliance", "Sql", "Tables");
        var auditingDdlDir = Path.Combine(repoRoot, "PlantSafety", "PlantSafety.Infrastructure", "Auditing", "Sql", "Tables");
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "FMCSAStandard.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "FMCSARequirement.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "FMCSAStandardChangeLog.sql"));
        RunDdlScript(connectionString, Path.Combine(complianceDdlDir, "FMCSARequirementModuleMapping.sql"));
        RunDdlScript(connectionString, Path.Combine(auditingDdlDir, "AuditFindingFmcsaCoverage.sql"));

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
        if (titleNode is null || titleNode.Value.Trim() != "49")
            throw new InvalidDataException("XML does not declare Title 49.");

        var hasTargetPart = doc.Descendants("DIV5")
            .Any(d => (string?)d.Attribute("TYPE") == "PART" && int.TryParse((string?)d.Attribute("N"), out var n) && TargetPartSet.Contains(n));
        if (!hasTargetPart)
            throw new InvalidDataException($"XML does not contain any of the target FMCSA parts ({string.Join(", ", TargetParts)}).");

        Console.WriteLine("XML validated: well-formed, Title 49, contains target FMCSA parts.");
    }

    private static List<ParsedStandard> ParseFmcsaStandards(string xmlPath, HashSet<int> targetParts)
    {
        var doc = XDocument.Load(xmlPath);
        var results = new List<ParsedStandard>();

        var partDivs = doc.Descendants("DIV5")
            .Where(d => (string?)d.Attribute("TYPE") == "PART"
                        && int.TryParse((string?)d.Attribute("N"), out var n)
                        && targetParts.Contains(n));

        foreach (var partDiv in partDivs)
        {
            var partNumber = int.Parse((string?)partDiv.Attribute("N") ?? "0", CultureInfo.InvariantCulture);
            var partTitle = CleanText(partDiv.Element("HEAD")?.Value);
            var authority = CleanText(partDiv.Element("AUTH")?.Value);
            WalkPart(partDiv, partNumber, partTitle, authority, subpart: null, results);
        }

        return results;
    }

    private static void WalkPart(XElement element, int partNumber, string? partTitle, string? authority, string? subpart, List<ParsedStandard> results)
    {
        foreach (var child in element.Elements())
        {
            var type = (string?)child.Attribute("TYPE");

            if (child.Name.LocalName == "DIV6" && type == "SUBPART")
            {
                var childSubpart = (string?)child.Attribute("N");
                WalkPart(child, partNumber, partTitle, authority, childSubpart, results);
            }
            else if (type == "SECTION")
            {
                results.Add(ExtractSection(child, partNumber, partTitle, authority, subpart));
            }
            else
            {
                WalkPart(child, partNumber, partTitle, authority, subpart, results);
            }
        }
    }

    private static ParsedStandard ExtractSection(XElement sectionDiv, int partNumber, string? partTitle, string? authority, string? subpart)
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

        if (sectionText.Length == 0)
            sectionText = head;

        return new ParsedStandard(partNumber, partTitle, subpart, sectionNumber, sectionTitle, sectionText, authority, effectiveDate);
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

    // Compliance.FMCSAStandard has no Status column (see FMCSAStandard.sql) -- unlike
    // Compliance.OSHAStandard, "current state" of a standard is just whatever row exists;
    // "Rescinded" is tracked only as an immutable Compliance.FMCSAStandardChangeLog entry, never
    // as a mutable flag on the standard row itself.
    private static Dictionary<(int PartNumber, string SectionNumber), ExistingStandard> LoadExistingStandards(SqlConnection connection, SqlTransaction transaction, HashSet<int> targetParts)
    {
        var existing = new Dictionary<(int, string), ExistingStandard>();
        const string sql = """
            SELECT StandardId, PartNumber, SectionNumber, SectionTextHash
            FROM [Compliance].[FMCSAStandard];
            """;
        using var command = new SqlCommand(sql, connection, transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (!int.TryParse(reader.GetString(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var partNumber)) continue;
            if (!targetParts.Contains(partNumber)) continue;
            var sectionNumber = reader.GetString(2);
            existing[(partNumber, sectionNumber)] = new ExistingStandard(
                reader.GetGuid(0),
                reader.IsDBNull(3) ? null : reader.GetString(3));
        }
        return existing;
    }

    private static HashSet<Guid> LoadAlreadyRescindedStandardIds(SqlConnection connection, SqlTransaction transaction)
    {
        var result = new HashSet<Guid>();
        const string sql = "SELECT DISTINCT StandardId FROM [Compliance].[FMCSAStandardChangeLog] WHERE ChangeType = 'Rescinded';";
        using var command = new SqlCommand(sql, connection, transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(reader.GetGuid(0));
        return result;
    }

    private static DiffResult UpsertStandards(string connectionString, List<ParsedStandard> standards)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existing = LoadExistingStandards(connection, transaction, TargetPartSet);
        var alreadyRescinded = LoadAlreadyRescindedStandardIds(connection, transaction);
        var standardIds = new Dictionary<(int, string), Guid>();
        var changedStandardIds = new HashSet<Guid>();
        var seenKeys = new HashSet<(int, string)>();
        int newCount = 0, revisedCount = 0, unchangedCount = 0, rescindedCount = 0;

        const string upsertSql = """
            MERGE [Compliance].[FMCSAStandard] AS target
            USING (SELECT @SectionNumber AS SectionNumber, @PartNumber AS PartNumber) AS source
            ON target.SectionNumber = source.SectionNumber AND target.PartNumber = source.PartNumber
            WHEN MATCHED THEN UPDATE SET
                SubpartLetter = @SubpartLetter, SectionTitle = @SectionTitle, SectionText = @SectionText,
                SectionTextHash = @SectionTextHash, SourceUrl = @SourceUrl
            WHEN NOT MATCHED THEN INSERT
                (StandardId, PartNumber, SubpartLetter, SectionNumber, SectionTitle, SectionText, SectionTextHash, SourceUrl)
                VALUES (@StandardId, @PartNumber, @SubpartLetter, @SectionNumber, @SectionTitle, @SectionText, @SectionTextHash, @SourceUrl)
            OUTPUT inserted.StandardId;
            """;
        const string changeLogSql = """
            INSERT INTO [Compliance].[FMCSAStandardChangeLog]
                (ChangeLogId, StandardId, PartNumber, SectionNumber, ChangeType, PreviousTextHash, NewTextHash, Summary)
            VALUES
                (@ChangeLogId, @StandardId, @PartNumber, @SectionNumber, @ChangeType, @PreviousTextHash, @NewTextHash, @Summary);
            """;

        var count = 0;
        foreach (var s in standards)
        {
            var key = (s.PartNumber, s.SectionNumber);
            seenKeys.Add(key);
            var hash = ComputeHash(s.SectionText);
            existing.TryGetValue(key, out var prior);

            var changeType = prior is null ? "New" : prior.SectionTextHash != hash ? "Revised" : null;

            using var command = new SqlCommand(upsertSql, connection, transaction);
            var newId = Guid.NewGuid();
            command.Parameters.AddWithValue("@StandardId", newId);
            command.Parameters.AddWithValue("@PartNumber", s.PartNumber.ToString(CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@SubpartLetter", (object?)s.Subpart ?? DBNull.Value);
            command.Parameters.AddWithValue("@SectionNumber", s.SectionNumber);
            command.Parameters.AddWithValue("@SectionTitle", (object?)s.SectionTitle ?? DBNull.Value);
            command.Parameters.AddWithValue("@SectionText", s.SectionText);
            command.Parameters.AddWithValue("@SectionTextHash", hash);
            command.Parameters.AddWithValue("@SourceUrl", SourceUrl);

            var resultId = (Guid?)command.ExecuteScalar() ?? newId;
            standardIds[key] = resultId;

            if (changeType is not null)
            {
                changedStandardIds.Add(resultId);
                if (changeType == "New") newCount++; else revisedCount++;

                using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
                logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
                logCmd.Parameters.AddWithValue("@StandardId", resultId);
                logCmd.Parameters.AddWithValue("@PartNumber", s.PartNumber.ToString(CultureInfo.InvariantCulture));
                logCmd.Parameters.AddWithValue("@SectionNumber", s.SectionNumber);
                logCmd.Parameters.AddWithValue("@ChangeType", changeType);
                logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior?.SectionTextHash ?? DBNull.Value);
                logCmd.Parameters.AddWithValue("@NewTextHash", hash);
                logCmd.Parameters.AddWithValue("@Summary", $"{s.SectionNumber}: {changeType} (govinfo.gov CFR Title 49 sync)");
                logCmd.ExecuteNonQuery();
            }
            else
            {
                unchangedCount++;
            }

            count++;
            if (count % 200 == 0) Console.WriteLine($"  processed {count}/{standards.Count} standards...");
        }

        // No Status column to flip on Compliance.FMCSAStandard (see LoadExistingStandards comment)
        // -- a rescinded section's row is simply left as last-synced; only the ChangeLog gets an
        // immutable "Rescinded" entry, and only once per StandardId (alreadyRescinded guards
        // idempotency across repeated runs).
        foreach (var (key, prior) in existing)
        {
            if (seenKeys.Contains(key) || alreadyRescinded.Contains(prior.StandardId)) continue;

            using var logCmd = new SqlCommand(changeLogSql, connection, transaction);
            logCmd.Parameters.AddWithValue("@ChangeLogId", Guid.NewGuid());
            logCmd.Parameters.AddWithValue("@StandardId", prior.StandardId);
            logCmd.Parameters.AddWithValue("@PartNumber", key.Item1.ToString(CultureInfo.InvariantCulture));
            logCmd.Parameters.AddWithValue("@SectionNumber", key.Item2);
            logCmd.Parameters.AddWithValue("@ChangeType", "Rescinded");
            logCmd.Parameters.AddWithValue("@PreviousTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@NewTextHash", (object?)prior.SectionTextHash ?? DBNull.Value);
            logCmd.Parameters.AddWithValue("@Summary", $"{key.Item2}: no longer present in govinfo.gov CFR Title 49 source (Rescinded)");
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
            DELETE m FROM [Compliance].[FMCSARequirementModuleMapping] m
            JOIN [Compliance].[FMCSARequirement] r ON r.RequirementId = m.RequirementId
            WHERE r.StandardId = @StandardId;
            """;
        const string deleteSql = "DELETE FROM [Compliance].[FMCSARequirement] WHERE StandardId = @StandardId;";
        const string insertSql = """
            INSERT INTO [Compliance].[FMCSARequirement] (RequirementId, StandardId, RequirementNumber, RequirementText, Severity, AppliesToModule, Rationale)
            VALUES (@RequirementId, @StandardId, @RequirementNumber, @RequirementText, NULL, NULL, @Rationale);
            """;

        using var transaction = connection.BeginTransaction();
        var totalRequirements = 0;
        var regenerated = 0;
        foreach (var s in standards)
        {
            if (!diff.StandardIds.TryGetValue((s.PartNumber, s.SectionNumber), out var standardId)) continue;
            if (!diff.ChangedStandardIds.Contains(standardId)) continue;
            regenerated++;

            // Module mappings reference RequirementId by FK -- must be cleared before the
            // requirement rows they point to are deleted below, or the delete violates the FK.
            using (var delMappings = new SqlCommand(deleteMappingsSql, connection, transaction))
            {
                delMappings.Parameters.AddWithValue("@StandardId", standardId);
                delMappings.ExecuteNonQuery();
            }

            using (var del = new SqlCommand(deleteSql, connection, transaction))
            {
                del.Parameters.AddWithValue("@StandardId", standardId);
                del.ExecuteNonQuery();
            }

            var sequence = 0;
            foreach (var sentence in SentenceSplitRegex.Split(s.SectionText))
            {
                var trimmed = sentence.Trim();
                if (trimmed.Length == 0 || !RequirementSentenceRegex.IsMatch(trimmed)) continue;

                sequence++;
                var requirementNumber = $"{s.SectionNumber}-{sequence}";
                var requirementText = trimmed.Length > 4000 ? trimmed[..4000] : trimmed;

                using var insert = new SqlCommand(insertSql, connection, transaction);
                insert.Parameters.AddWithValue("@RequirementId", Guid.NewGuid());
                insert.Parameters.AddWithValue("@StandardId", standardId);
                insert.Parameters.AddWithValue("@RequirementNumber", requirementNumber);
                insert.Parameters.AddWithValue("@RequirementText", requirementText);
                insert.Parameters.AddWithValue("@Rationale", $"Derived from 49 CFR {s.SectionNumber} ({s.PartTitle ?? "Part " + s.PartNumber}).");
                insert.ExecuteNonQuery();
                totalRequirements++;
            }
        }
        transaction.Commit();
        Console.WriteLine($"Regenerated requirements for {regenerated} changed standards ({totalRequirements} requirement rows).");
    }

    // ------------------------------------------------------------------------------------------
    // Phase 6: FMCSA requirement -> PlantSafety module mapping.
    //
    // Two-tier deterministic ruleset:
    //   1) Primary module, from the requirement's CFR Part (fixed ruleset below). Parts 393 and
    //      397 also carry a forced Part-level secondary module -- their subject matter (vehicle
    //      equipment condition; hazmat routing) is legitimately dual-homed between a domain module
    //      and LogisticsTransportSafety, but only one module can be mirrored onto the single
    //      FMCSARequirement.AppliesToModule column, so the domain module (MachineSafety /
    //      EnvironmentalHealth) is coded as Primary and LogisticsTransportSafety as a forced
    //      Secondary for those two parts specifically.
    //   2) Secondary module(s), from the requirement's already-derived RequirementType keyword
    //      (PPE/Training/Inspection/Permit only) -- added only when the RequirementType's module
    //      differs from modules already present, mirroring OshaStandardsImport's
    //      RequirementTypeSecondaryRules mechanism exactly.
    //
    // Severity is assigned per RequirementType (same value on every mapping row for a given
    // requirement, since it describes the requirement's own text, not the module relationship) --
    // same SeverityByRequirementType style/values as OshaStandardsImport.
    //
    // Compliance.FMCSARequirementModuleMapping has no IsPrimary column (unlike OSHA's mapping
    // table) -- "primary" only matters for which single module gets mirrored onto
    // Compliance.FMCSARequirement.AppliesToModule, which this method still does explicitly.
    //
    // Every module name below is a real top-level PlantSafety.Infrastructure folder in this
    // repository (verified 2026-08-07): LogisticsTransportSafety, OperationalSafety, MachineSafety,
    // EnvironmentalHealth, Auditing, Operational, PermitToWork, PPE.
    // ------------------------------------------------------------------------------------------

    private sealed record ModuleRule(string Module, string RuleCode, string Rationale);

    // Part-level primary rule -- fixed ruleset per the FMCSA ingestion spec, one entry per target part.
    private static readonly Dictionary<int, ModuleRule> PartPrimaryRules = new()
    {
        [383] = new("LogisticsTransportSafety", "Part.383.Primary", "Commercial Driver's License (CDL) Standards: driver licensing/qualification for commercial motor vehicle operation is core logistics/transport safety."),
        [390] = new("LogisticsTransportSafety", "Part.390.Primary", "FMCSA General Applicability, Definitions and General Rules: foundational motor-carrier safety program administration."),
        [391] = new("LogisticsTransportSafety", "Part.391.Primary", "Driver Qualification: physical qualification, testing, and driver-file requirements are direct commercial-driver fitness-for-duty controls."),
        [392] = new("OperationalSafety", "Part.392.Primary", "Driving of Commercial Motor Vehicles: rules governing conduct while operating a commercial motor vehicle are direct operational hazard controls."),
        [393] = new("MachineSafety", "Part.393.Primary", "Parts and Accessories Necessary for Safe Operation: vehicle equipment condition/mechanical safety standards are machine-safety controls."),
        [395] = new("LogisticsTransportSafety", "Part.395.Primary", "Hours of Service of Drivers: driver fatigue/duty-time limits for commercial motor vehicle operation."),
        [396] = new("LogisticsTransportSafety", "Part.396.Primary", "Inspection, Repair, and Maintenance: vehicle maintenance program requirements for motor carriers."),
        [397] = new("EnvironmentalHealth", "Part.397.Primary", "Transportation of Hazardous Materials, Driving and Parking Rules: hazmat routing/handling (shared with PHMSA) is a chemical/environmental-hazard exposure control."),
    };

    // Forced Part-level secondary rule -- only Parts 393 and 397 are dual-homed per the spec.
    private static readonly Dictionary<int, ModuleRule> PartForcedSecondaryRules = new()
    {
        [393] = new("LogisticsTransportSafety", "Part.393.Secondary", "Vehicle safety standards directly govern the equipment used in commercial transport operations; also tracked under logistics/transport safety."),
        [397] = new("LogisticsTransportSafety", "Part.397.Secondary", "Hazmat routing directly governs commercial transport operations; also tracked under logistics/transport safety."),
    };

    // Secondary module overlay, keyed by the RequirementType already derived in Phase 5 -- added
    // alongside the primary (and any forced secondary) Part mapping when it names a module not
    // already present. Only PPE/Training/Inspection/Permit carry a secondary-module rule, mirroring
    // OshaStandardsImport's RequirementTypeSecondaryRules mechanism.
    private static readonly Dictionary<string, ModuleRule> RequirementTypeSecondaryRules = new()
    {
        ["PPE"] = new("PPE", "RequirementType.PPE", "PPE-flavoured requirement text; direct personal protective equipment requirement."),
        ["Training"] = new("Operational", "RequirementType.Training", "Training-flavoured requirement text; real training records live in Operational.Training (see WorkerTrainingFactsProvider)."),
        ["Inspection"] = new("Auditing", "RequirementType.Inspection", "Inspection-flavoured requirement text; inspection activity maps to internal audit posture."),
        ["Permit"] = new("PermitToWork", "RequirementType.Permit", "Permit-flavoured requirement text; permit-controlled work is PermitToWork's domain regardless of CFR location."),
    };

    // Severity per RequirementType (ComplianceSeverity codes: Advisory/Low/Medium/High/Critical --
    // see PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity). Same style/values as
    // OshaStandardsImport: Permit-controlled and PPE-flavoured requirements are High (failure is a
    // direct, immediate injury/violation risk); Inspection/Training/Reporting/Procedure are Medium
    // (process controls); Recordkeeping/General are Low (administrative). Deterministic business
    // rule, not a fact extracted from CFR text -- the CFR does not itself grade section severity.
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

    private static string DeriveRequirementType(string requirementText)
    {
        var lower = " " + requirementText.ToLowerInvariant() + " ";
        return TypeKeywords.FirstOrDefault(k => lower.Contains(k.Keyword)).RequirementType ?? "General";
    }

    private static void ApplyModuleMapping(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string selectUnmappedSql = """
            SELECT r.RequirementId, r.RequirementText, s.PartNumber
            FROM [Compliance].[FMCSARequirement] r
            JOIN [Compliance].[FMCSAStandard] s ON s.StandardId = r.StandardId
            WHERE NOT EXISTS (SELECT 1 FROM [Compliance].[FMCSARequirementModuleMapping] m WHERE m.RequirementId = r.RequirementId);
            """;

        var targets = new List<(Guid RequirementId, string RequirementText, int PartNumber)>();
        using (var selectCmd = new SqlCommand(selectUnmappedSql, connection))
        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                if (!int.TryParse(reader.GetString(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var partNumber)) continue;
                targets.Add((
                    reader.GetGuid(0),
                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    partNumber));
            }
        }

        if (targets.Count == 0)
        {
            Console.WriteLine("No unmapped requirements found; nothing to map.");
            return;
        }

        const string insertMappingSql = """
            INSERT INTO [Compliance].[FMCSARequirementModuleMapping]
                (MappingId, RequirementId, ModuleName, MappingRuleCode, Rationale, Severity)
            VALUES
                (@MappingId, @RequirementId, @ModuleName, @MappingRuleCode, @Rationale, @Severity);
            """;
        const string updateRequirementSql = """
            UPDATE [Compliance].[FMCSARequirement] SET AppliesToModule = @Module, Severity = @Severity
            WHERE RequirementId = @RequirementId;
            """;

        using var transaction = connection.BeginTransaction();
        var mappingRowCount = 0;
        var requirementCount = 0;
        foreach (var target in targets)
        {
            var requirementType = DeriveRequirementType(target.RequirementText);
            var severity = SeverityByRequirementType.TryGetValue(requirementType, out var sev) ? sev : "Low";

            var primary = PartPrimaryRules.TryGetValue(target.PartNumber, out var partRule)
                ? partRule
                : new ModuleRule("OperationalSafety", "Part.Unmapped.Fallback", "No Part rule defined for this CFR part; routed to general operational safety pending an explicit rule.");

            var mappings = new List<ModuleRule> { primary };

            if (PartForcedSecondaryRules.TryGetValue(target.PartNumber, out var forcedSecondary)
                && mappings.All(m => m.Module != forcedSecondary.Module))
            {
                mappings.Add(forcedSecondary);
            }

            if (RequirementTypeSecondaryRules.TryGetValue(requirementType, out var typeSecondary)
                && mappings.All(m => m.Module != typeSecondary.Module))
            {
                mappings.Add(typeSecondary);
            }

            foreach (var rule in mappings)
            {
                using var insertCmd = new SqlCommand(insertMappingSql, connection, transaction);
                insertCmd.Parameters.AddWithValue("@MappingId", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@RequirementId", target.RequirementId);
                insertCmd.Parameters.AddWithValue("@ModuleName", rule.Module);
                insertCmd.Parameters.AddWithValue("@MappingRuleCode", rule.RuleCode);
                insertCmd.Parameters.AddWithValue("@Rationale", rule.Rationale);
                insertCmd.Parameters.AddWithValue("@Severity", severity);
                insertCmd.ExecuteNonQuery();
                mappingRowCount++;
            }

            using (var updateCmd = new SqlCommand(updateRequirementSql, connection, transaction))
            {
                updateCmd.Parameters.AddWithValue("@Module", primary.Module);
                updateCmd.Parameters.AddWithValue("@Severity", severity);
                updateCmd.Parameters.AddWithValue("@RequirementId", target.RequirementId);
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
        string SectionNumber,
        string? SectionTitle,
        string SectionText,
        string? Authority,
        DateOnly? EffectiveDate);

    private sealed record ExistingStandard(Guid StandardId, string? SectionTextHash);

    private sealed record DiffResult(
        Dictionary<(int PartNumber, string SectionNumber), Guid> StandardIds,
        HashSet<Guid> ChangedStandardIds,
        int NewCount,
        int RevisedCount,
        int UnchangedCount,
        int RescindedCount);
}
