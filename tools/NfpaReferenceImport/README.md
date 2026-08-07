# NFPA Reference-Data Import / Update Tool

Standalone tool (not part of `PlantSafety.sln`) that keeps `Compliance.NFPA_HazardProfile`,
`Compliance.NFPA_ControlMatrix`, and `Compliance.NFPA_HazardModuleMapping` in
`PlantSafety.CoreDB` in sync with a hazard-profile feed for NFPA (National Fire Protection
Association) codes — currently NFPA 70 (National Electrical Code) and NFPA 51B (Fire
Prevention During Welding, Cutting, and Other Hot Work).

## This tool contains no real NFPA content — read this before touching it

NFPA is not a government standards body. Unlike OSHA (`tools/OshaStandardsImport`), whose
regulatory text is public-domain federal law published as open eCFR bulk XML, NFPA code text
is **copyrighted, licensed content**. There is no open, machine-readable NFPA feed this tool
could legally download, and it must never scrape, fabricate, or otherwise reproduce real NFPA
code language.

Every record this tool ingests comes from local, hand-written mock/placeholder JSON files under
`SampleData/` (`nfpa70-hazards.json`, `nfpa51b-hazards.json`). Every guidance field in every
record is prefixed `PLACEHOLDER:` and paraphrased in the tool's own words (e.g. *"PLACEHOLDER:
Ensure adequate clearance around electrical panels per NFPA 70 guidance"*) — never actual NFPA
code text. This tool performs **no network calls of any kind**. It exists to exercise and prove
out the full ingestion pipeline (diff/upsert, change-log, control matrix, module mapping) against
a schema shaped like the real thing, pending a real licensed NFPA data-source integration
(e.g. an NFPA LiNK API agreement) that would replace `SampleData/*.json` with a genuine feed
without changing anything else about this tool's structure.

## Run manually

```bash
dotnet run --project tools/NfpaReferenceImport -c Release
```

Flags:
- `--skip-download` — kept for command-line parity with `OshaStandardsImport`; this tool has no
  network source, so the flag is a documented no-op (it always reads `SampleData/*.json`).
- `--connection-string "..."` — override the default
  `Server=localhost\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;`.

## What it does each run

1. Loads every configured mock input file under `SampleData/` (currently
   `nfpa70-hazards.json` for NFPA 70, `nfpa51b-hazards.json` for NFPA 51B — add another
   `(CodeNumber, FileName)` pair to `Program.SourceFiles` to bring a new code into scope).
2. Parses each record into an internal hazard-profile model (CodeNumber, SectionNumber,
   HazardName, HazardDescription, RecommendedControls, PPERecommendations,
   ElectricalSafetyGuidance, HotWorkGuidance, FlammableMaterialsGuidance, SourceUrl, and a list
   of controls: ControlType, ControlDescription, Severity, Rationale).
3. Applies `PlantSafety.Infrastructure/Compliance/Sql/Tables/NFPA_HazardProfile.sql`,
   `NFPA_HazardChangeLog.sql`, `NFPA_ControlMatrix.sql`, `NFPA_HazardModuleMapping.sql`, and
   `PlantSafety.Infrastructure/Auditing/Sql/Tables/AuditFindingNfpaCoverage.sql` (idempotent).
4. For each parsed hazard, compares a SHA-256 hash of its concatenated description/guidance
   fields against `Compliance.NFPA_HazardProfile.ProfileHash`, keyed by
   `(CodeNumber, SectionNumber)`:
   - not present → **New**
   - hash differs → **Revised**
   - hash matches → **Unchanged** (no write)
   Every New/Revised hazard is upserted (`MERGE ... OUTPUT inserted.HazardId`, inside a
   `SqlTransaction`) and logged to `Compliance.NFPA_HazardChangeLog`. Any hazard that exists in
   the DB but is missing from the freshly loaded set is marked `Status = 'Rescinded'` and
   logged — the row is kept for history, not deleted.
5. `Compliance.NFPA_ControlMatrix` rows are deleted and reinserted only for hazards that were
   New or Revised this run, from that hazard's own control list, leaving Unchanged hazards'
   control rows untouched.
6. `Compliance.NFPA_HazardModuleMapping` rows are computed for any hazard with no existing
   mapping, via a deterministic, inspectable rule set (NFPA code → primary module(s);
   FlammableMaterialsGuidance presence → EnvironmentalHealth/HazardousWasteManagement overlay;
   "fire protection" wording → OperationalSafety overlay; each distinct ControlType present on
   the hazard's controls → its own secondary module). Every mapping row carries the
   `MappingRuleCode`/`Rationale` of the exact rule that produced it — see the comment block
   above `ApplyModuleMapping` in `Program.cs` for the full ruleset and the two module-name
   substitutions it had to make.

Re-running with unchanged `SampleData/*.json` content is a no-op past Phase 3: zero new
`NFPA_HazardChangeLog` rows, and Phase 6 finds no unmapped hazards (mapping rows are already
present from the prior run), so no duplicate `NFPA_HazardModuleMapping` rows are ever created.

Query recent changes (what a "reference data changed" alert would read):

```sql
SELECT * FROM [Compliance].[NFPA_HazardChangeLog]
WHERE DetectedAtUtc > DATEADD(day, -30, SYSUTCDATETIME())
ORDER BY DetectedAtUtc DESC;
```

## Scheduling

Nothing is registered automatically. To run it on a recurring schedule:

- **Windows**: `schedule/Register-NfpaUpdateTask.ps1` registers a Windows Task Scheduler job
  (weekly by default, `-Daily` for daily) that runs `schedule/run-update.ps1`. Run it yourself,
  elevated: `.\schedule\Register-NfpaUpdateTask.ps1`. `-Unregister` removes it.
- **Linux/cron**: `schedule/run-update.sh`, wired up via crontab (example command in the file
  header).

Both wrappers publish the tool once to `tools/NfpaReferenceImport/publish/` and reuse the
compiled output on subsequent runs.

## Why this stays external to PlantSafety.Api

Like the OSHA tool, NFPA reference data is global, tenant-independent reference data, not
runtime data, and re-syncing it is an infrequent, batch-style operation, not something that
belongs on the API's request path. Keeping it as an external, independently-scheduled tool keeps
regulatory reference-data ETL out of the runtime's failure and performance surface —
`PlantSafety.Api` only ever reads `Compliance.NFPA_HazardProfile`/`NFPA_ControlMatrix`/
`NFPA_HazardModuleMapping`/`NFPA_HazardChangeLog`, never ingests them. This project has no
project reference to `PlantSafety.Api`/`.Application`/`.Infrastructure` and is not referenced by
any of them — it talks to `PlantSafety.CoreDB` directly via raw `Microsoft.Data.SqlClient`, the
same as `OshaStandardsImport`.

## When real NFPA data becomes available

Swap `SampleData/*.json` (or extend `Program.LoadMockInput`/`Program.SourceFiles`) for a real,
licensed NFPA data source once one is contracted (e.g. NFPA LiNK). The diff/upsert, change-log,
control-matrix, and module-mapping logic in Phases 3-6 does not need to change — it operates on
the same internal `ParsedHazard`/`ParsedControl` model regardless of where the records came from.
