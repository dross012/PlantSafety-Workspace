# FMCSA Standards Import / Update Tool

Standalone tool (not part of `PlantSafety.sln`) that keeps `Compliance.FMCSAStandard` and
`Compliance.FMCSARequirement` in `PlantSafety.CoreDB` in sync with CFR Title 49 (FMCSA -- Federal
Motor Carrier Safety Administration), scoped to 8 target parts, sourced from the govinfo.gov bulk
XML at `https://www.govinfo.gov/bulkdata/ECFR/title-49/ECFR-title49.xml`.

49 CFR is public federal law, the same legal character as OSHA's 29 CFR (which
`tools/OshaStandardsImport` already downloads and parses from eCFR/govinfo.gov). Unlike a prior
NFPA effort -- NFPA text is copyrighted, not government-published, so that tool ran on mock data --
this tool downloads and parses real, publicly downloadable federal regulation text, the same way
`OshaStandardsImport` does for Title 29. This project mirrors `OshaStandardsImport`'s architecture
and coding conventions exactly: same phased `Program.cs` pipeline, same `HttpClient` download, same
`XDocument`/regex parse, same SHA-256 diff/upsert/change-log pattern, same raw-`SqlClient` DDL
runner, same command-line flags.

The same run does both jobs: a first run on an empty table is a full import (everything comes back
"New"); every later run is an incremental update (diffs by SHA-256 hash of each section's text and
only touches what changed).

## Target parts (49 CFR)

Scoped by an explicit, extensible allow-list (`TargetParts` in `Program.cs`) -- not a single
part/range hack:

| Part | Subject                                                              |
|------|-----------------------------------------------------------------------|
| 383  | Commercial Driver's License (CDL) Standards                          |
| 390  | FMCSA General Applicability and Definitions / General Rules          |
| 391  | Driver Qualification                                                  |
| 392  | Driving of Commercial Motor Vehicles                                  |
| 393  | Parts and Accessories Necessary for Safe Operation (vehicle safety)  |
| 395  | Hours of Service of Drivers                                           |
| 396  | Inspection, Repair, and Maintenance                                   |
| 397  | Transportation of Hazardous Materials, Driving and Parking Rules (shared with PHMSA) |

## Run manually

```bash
dotnet run --project tools/FmcsaStandardsImport -c Release
```

Flags:
- `--skip-download` -- reuse the XML already in `tools/FmcsaStandardsImport/data/`, useful for
  iterating on parsing/diff logic without refetching the bulk XML.
- `--connection-string "..."` -- override the default
  `Server=localhost\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;`.

## Local parsing test fixture

`testdata/sample-title49.xml` is a small, hand-built XML file that mimics the real govinfo.gov CFR
bulk XML shape (`DLPSTEXTCLASS`/`DIV5 PART`/`DIV6 SUBPART`/`SECTION`/`HEAD`/`P`/`CITA`/`AUTH`)
closely enough to exercise Phase 2's parser end to end without a live network call. It is a
standalone local test aid only -- it is never read by the tool's real Phase 1 download path and is
not the primary data source. To use it: copy it to `tools/FmcsaStandardsImport/data/ECFR-title49.xml`
(the `data/` directory is gitignored and is also where Phase 1 saves the real download) and run
with `--skip-download`.

## What it does each run

1. Downloads the current govinfo.gov CFR Title 49 bulk XML via `HttpClient` and validates it
   (well-formed, declares Title 49, contains at least one of the 8 target parts).
2. Parses the 8 target parts into Part/Subpart/Section rows (Section text, Authority citation,
   effective date where present).
3. Applies the DDL under `PlantSafety.Infrastructure/Compliance/Sql/Tables`
   (`FMCSAStandard.sql`, `FMCSARequirement.sql`, `FMCSAStandardChangeLog.sql`,
   `FMCSARequirementModuleMapping.sql`) and `PlantSafety.Infrastructure/Auditing/Sql/Tables`
   (`AuditFindingFmcsaCoverage.sql`) in-process, same `RunDdlScript`/`GO`-batch-split pattern as
   `OshaStandardsImport` -- this tool never inlines duplicate DDL.
4. For each parsed section, compares its SHA-256 content hash against
   `Compliance.FMCSAStandard.SectionTextHash`, keyed on `(PartNumber, SectionNumber)`:
   - not present -> **New**
   - hash differs -> **Revised**
   - hash matches -> **Unchanged** (no write)
   Every New/Revised standard is upserted via `MERGE ... OUTPUT inserted.StandardId` inside a
   `SqlTransaction` and logged to `Compliance.FMCSAStandardChangeLog` (append-only -- rows are
   never updated or deleted after being written).
5. Any section that exists in the DB (within the 8 target parts) but is missing from the freshly
   parsed set is logged once as `Rescinded` in `Compliance.FMCSAStandardChangeLog`.
   `Compliance.FMCSAStandard` itself has no mutable status column, so a rescinded section's row is
   simply left as last-synced -- the changelog is the single source of truth for "this section is
   gone," and it is only ever logged once per standard (idempotent across repeated runs).
6. `Compliance.FMCSARequirement` rows are regenerated only for standards that were New or Revised
   this run (deleted and re-derived from the section text via the same
   `must`/`shall`/`required`/`prohibited` sentence-split regex `OshaStandardsImport` uses), leaving
   Unchanged standards' requirements untouched. `RequirementNumber`/`RequirementText`/`Rationale`
   are persisted; `AppliesToModule`/`Severity` are left `NULL` until Phase 6.
7. Phase 6 (`ApplyModuleMapping`) deterministically maps every unmapped requirement to one or more
   PlantSafety modules and writes `Compliance.FMCSARequirementModuleMapping` rows, mirroring the
   primary module/severity back onto `FMCSARequirement.AppliesToModule`/`Severity`:
   - Part 383/390/391/395/396 -> `LogisticsTransportSafety`.
   - Part 392 -> `OperationalSafety`.
   - Part 393 -> `MachineSafety` (primary) + `LogisticsTransportSafety` (forced secondary) --
     vehicle equipment condition is a machine-safety control that is also directly part of
     transport operations.
   - Part 397 -> `EnvironmentalHealth` (primary) + `LogisticsTransportSafety` (forced secondary) --
     hazmat routing is a chemical/environmental exposure control that is also directly part of
     transport operations.
   - Secondary modules are also derived from the requirement's keyword-detected type: `PPE`,
     `Training` -> `Operational`, `Inspection` -> `Auditing`, `Permit` -> `PermitToWork` -- the
     same `RequirementTypeSecondaryRules` mechanism `OshaStandardsImport` uses.
   - Severity is assigned per requirement-type via a fixed `SeverityByRequirementType` lookup
     (Permit/PPE = High, Inspection/Training/Reporting/Procedure = Medium,
     Recordkeeping/General = Low).
   - Every mapping row carries a dotted `MappingRuleCode` (e.g. `Part.383.Primary`,
     `Part.393.Secondary`, `RequirementType.PPE`) and a free-text `Rationale` -- the ruleset is
     explicit, inspectable C# data, never a black box.

Query recent changes (what a "regulation changed" alert would read):

```sql
SELECT * FROM [Compliance].[FMCSAStandardChangeLog]
WHERE DetectedAtUtc > DATEADD(day, -30, SYSUTCDATETIME())
ORDER BY DetectedAtUtc DESC;
```

## Scheduling

Nothing is registered automatically. To run it on a recurring schedule:

- **Windows**: `schedule/Register-FmcsaUpdateTask.ps1` registers a Windows Task Scheduler job
  (weekly by default, `-Daily` for daily) that runs `schedule/run-update.ps1`. Run it yourself,
  elevated: `.\schedule\Register-FmcsaUpdateTask.ps1`. `-Unregister` removes it.
- **Linux/cron**: `schedule/run-update.sh`, wired up via crontab (example command in the file
  header).

Both wrappers publish the tool once to `tools/FmcsaStandardsImport/publish/` and reuse the
compiled output on subsequent runs.

## Why this stays external to PlantSafety.Api

FMCSA data is global regulatory reference data, not tenant-scoped runtime data, and the sync is a
heavy, external-network-dependent, infrequent operation (bulk XML download, multi-part parse,
thousands of requirement rows). Running it as a hosted `BackgroundService` inside `PlantSafety.Api`
would mean every API start/restart/deploy could trigger a CFR pull. Keeping it as an external,
independently-scheduled tool keeps regulatory ETL out of the runtime's failure and performance
surface -- `PlantSafety.Api` only ever reads `Compliance.FMCSAStandard`/`FMCSARequirement`/
`FMCSAStandardChangeLog`/`FMCSARequirementModuleMapping`, never ingests them. This project is not
referenced by, and does not reference, `PlantSafety.Api`/`.Application`/`.Infrastructure` -- it
talks to the database with raw `Microsoft.Data.SqlClient` only, same as `OshaStandardsImport`.
