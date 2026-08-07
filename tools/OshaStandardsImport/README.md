# OSHA Standards Import / Update Tool

Standalone tool (not part of `PlantSafety.sln`) that keeps `Compliance.OSHAStandard` and
`Compliance.OSHARequirement` in `PlantSafety.CoreDB` in sync with CFR Title 29 (OSHA),
Parts 1900-1999, sourced from the eCFR bulk XML at
`https://www.govinfo.gov/bulkdata/ECFR/title-29/ECFR-title29.xml`.

The same run does both jobs: a first run on an empty table is a full import (everything
comes back "New"); every later run is an incremental update (diffs by SHA-256 hash of each
section's text and only touches what changed).

## Run manually

```bash
dotnet run --project tools/OshaStandardsImport -c Release
```

Flags:
- `--skip-download` — reuse the XML already in `tools/OshaStandardsImport/data/`, useful for
  iterating on parsing/diff logic without refetching 29MB.
- `--connection-string "..."` — override the default
  `Server=localhost\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;`.

## What it does each run

1. Downloads the current eCFR Title 29 XML and validates it (well-formed, Title 29, contains
   OSHA parts).
2. Parses Parts 1900-1999 into Part/Subpart/Section rows.
3. Applies `PlantSafety.Infrastructure/Compliance/Sql/Tables/OSHAStandard.sql`,
   `OSHARequirement.sql`, and `OSHAStandardChangeLog.sql` (idempotent).
4. For each parsed section, compares its SHA-256 content hash against
   `Compliance.OSHAStandard.SectionTextHash`:
   - not present → **New**
   - hash differs → **Revised**
   - hash matches → **Unchanged** (no write)
   Every New/Revised standard is upserted and logged to `Compliance.OSHAStandardChangeLog`.
5. Any section that exists in the DB (within 1900-1999, not already Rescinded) but is missing
   from the freshly parsed set is marked `Status = 'Rescinded'` and logged — the row is kept
   for history, not deleted.
6. `Compliance.OSHARequirement` rows are regenerated only for standards that were New or
   Revised this run (deleted and re-derived from the section text), leaving Unchanged
   standards' requirements untouched.

Query recent changes (what a "regulation changed" alert would read):

```sql
SELECT * FROM [Compliance].[OSHAStandardChangeLog]
WHERE DetectedAtUtc > DATEADD(day, -30, SYSUTCDATETIME())
ORDER BY DetectedAtUtc DESC;
```

## Scheduling

Nothing is registered automatically. To run it on a recurring schedule:

- **Windows**: `schedule/Register-OshaUpdateTask.ps1` registers a Windows Task Scheduler job
  (weekly by default, `-Daily` for daily) that runs `schedule/run-update.ps1`. Run it yourself,
  elevated: `.\schedule\Register-OshaUpdateTask.ps1`. `-Unregister` removes it.
- **Linux/cron**: `schedule/run-update.sh`, wired up via crontab (example command in the file
  header).

Both wrappers publish the tool once to `tools/OshaStandardsImport/publish/` and reuse the
compiled output on subsequent runs.

## Why this stays external to PlantSafety.Api

OSHA data is global regulatory reference data, not tenant-scoped runtime data, and the sync
is a heavy, external-network-dependent, infrequent operation (large XML download, full-title
parse, tens of thousands of requirement rows). Running it as a hosted `BackgroundService`
inside `PlantSafety.Api` would mean every API start/restart/deploy could trigger a CFR pull.
Keeping it as an external, independently-scheduled tool keeps regulatory ETL out of the
runtime's failure and performance surface — `PlantSafety.Api` only ever reads
`Compliance.OSHAStandard`/`OSHARequirement`/`OSHAStandardChangeLog`, never ingests them.
