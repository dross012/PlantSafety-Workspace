# NIOSH Hazard Reference Import Tool

Standalone tool (not part of `PlantSafety.sln`) that keeps `Compliance.NIOSHHazardProfile`,
`Compliance.NIOSHRespiratorMatrix`, and `Compliance.NIOSHPPEGuidance` in `PlantSafety.CoreDB`
in sync with a normalized set of NIOSH hazard reference records (REL/STEL exposure limits,
health effects, recommended controls, PPE guidance, and respirator selection logic drawn from
sources like the NIOSH Pocket Guide to Chemical Hazards).

## Why this is different from `tools/OshaStandardsImport`

OSHA's data has one canonical bulk source: the eCFR Title 29 XML feed. NIOSH does not publish
an equivalent single bulk API for its hazard reference content — the Pocket Guide, respirator
selection logic, and topic-specific guidance (e.g. ergonomics, bloodborne pathogens) live across
several separate CDC/NIOSH resources, not one machine-readable dataset. Rather than build a
one-off scraper against a fragile HTML source, this tool reads from a local, normalized
**seed file** — `data/niosh-hazards.seed.json` — that represents the hazard data in the same
shape the database expects.

This keeps the tool useful today (it seeds/updates real reference data from curated JSON) while
leaving a clean seam for a future real data source: **only Phase 1 (`LoadSeedHazards` in
`Program.cs`) knows about the JSON file.** Everything after it — DDL, SHA-256 hashing, MERGE
upsert, change classification, change-log writes, child-row replacement, and module mapping —
operates purely on the in-memory `List<ParsedHazardProfile>` and has no idea where that list
came from. To wire up a real scraper or NIOSH-adjacent API later:

1. Write a new loader that produces `List<ParsedHazardProfile>` (same shape: chemical identity,
   REL/STEL text, health effects, controls, PPE recommendations, atmospheric monitoring guidance,
   ergonomic factors, source URL, plus nested respirator-matrix and PPE-guidance rows).
2. Swap the `LoadSeedHazards(seedFile)` call in Phase 1 of `Main` for the new loader.
3. Nothing in Phases 2-5 needs to change.

## Run manually

```bash
dotnet run --project tools/NioshStandardsImport -c Release
```

Flags:
- `--seed-file "..."` — point at a different seed JSON file (defaults to the bundled
  `tools/NioshStandardsImport/data/niosh-hazards.seed.json`).
- `--connection-string "..."` — override the default
  `Server=localhost\SQLEXPRESS;Database=PlantSafety.CoreDB;Trusted_Connection=True;TrustServerCertificate=True;`.

There is no `--skip-download` flag — this tool has no download phase to skip.

## What it does each run

1. **Load input** — reads and parses `data/niosh-hazards.seed.json` into an internal
   `ParsedHazardProfile` list (one entry per hazard/chemical, each carrying nested respirator-
   matrix and PPE-guidance rows).
2. **Build/verify schema** — applies every `NIOSH*.sql` script found under
   `PlantSafety.Infrastructure/Compliance/Sql/Tables/` (globbed by filename prefix rather than
   hardcoded, since those scripts are authored independently of this tool; idempotent, safe to
   re-run). `NIOSHHazardProfile.sql` always runs first regardless of alphabetical order, since
   every other NIOSH table has an FK to it.
3. **Deploy stored procedures** — applies every `usp_NIOSH*.sql` script found under
   `PlantSafety.Infrastructure/Compliance/Sql/Procedures/`. Each is `CREATE OR ALTER PROCEDURE`,
   so this is idempotent and order-independent — safe to re-run every time, which is how the
   Application/Infrastructure layers' Dapper repository stays in sync with the procedures it
   calls without a separate manual deployment step.
4. **Diff and upsert hazard profiles** — computes a SHA-256 hash (`ProfileHash`) over a
   normalized concatenation of each hazard's substantive fields, keyed by `CASNumber` when
   present (falling back to `ChemicalName` for hazards with no CAS number, e.g. ergonomic or
   biological hazards). Compared against `Compliance.NIOSHHazardProfile.ProfileHash`:
   - not present → **New**
   - hash differs → **Revised**
   - hash matches → **Unchanged** (no write)
   Every New/Revised hazard is upserted (`MERGE`, in a transaction) and logged to
   `Compliance.NIOSHHazardChangeLog`.
5. **Handle removals without hard-deleting** — a hazard that exists in the database but is no
   longer present in the current seed set is **not** deleted and its row is **not** mutated
   (`NIOSHHazardProfile` has no `Status` column, unlike `OSHAStandard`'s `Rescinded` flow). A
   `ChangeType = 'Removed'` row is written to `Compliance.NIOSHHazardChangeLog` so the removal is
   visible/auditable without discarding the reference data.
6. **Replace child rows** — for hazards that were New or Revised this run, existing
   `Compliance.NIOSHRespiratorMatrix` and `Compliance.NIOSHPPEGuidance` rows for that hazard are
   deleted and re-inserted from the current seed data. Unchanged hazards' child rows are left
   untouched.
7. **Map hazards to PlantSafety modules** — a deterministic C# rule table
   (`CategoryRules` in `Program.cs`) maps each hazard's category to a primary module (exactly one
   `IsPrimary = 1` row) and zero or more secondary modules, writing rows to
   `Compliance.NIOSHHazardModuleMapping`:

   | Hazard category                  | Primary module              | Secondary module(s)                        |
   |-----------------------------------|------------------------------|---------------------------------------------|
   | Respiratory                       | `PPE`                        | `OperationalSafety`, `EnvironmentalHealth`   |
   | Chemical                          | `HazardousWasteManagement`   | `EnvironmentalHealth`                        |
   | Ergonomic                         | `Ergonomics`                 | `OperationalSafety`                          |
   | Confined space / atmospheric      | `PermitToWork`               | `EnvironmentalHealth`                        |
   | Biological                        | `EmployeeSafety`             | `PPE`                                        |

   Every mapping row carries a `MappingRuleCode` identifying the exact rule that fired and a
   short human-readable `Rationale`. Category resolution prefers the seed record's explicit
   `hazardCategory` field; if a hazard doesn't match a known category (e.g. a future data source
   without a clean category field), a keyword fallback scans `HealthEffects` text before falling
   back to a last-resort default. Mapping rows are deleted and reinserted for every current
   hazard profile each run (the rule table is small and deterministic, so a full re-derive is
   simpler than tracking "only changed hazards" for this phase specifically).

Query recent changes:

```sql
SELECT * FROM [Compliance].[NIOSHHazardChangeLog]
WHERE DetectedAtUtc > DATEADD(day, -30, SYSUTCDATETIME())
ORDER BY DetectedAtUtc DESC;
```

## Why this stays external to PlantSafety.Api

Like OSHA data, NIOSH hazard guidance is global reference data, not tenant-scoped runtime data —
it doesn't belong to any one plant or organization. Keeping ingestion as an external,
independently-run tool keeps this ETL out of `PlantSafety.Api`'s startup/runtime failure surface;
the API only ever reads `Compliance.NIOSHHazardProfile` / `NIOSHRespiratorMatrix` /
`NIOSHPPEGuidance` / `NIOSHHazardChangeLog` / `NIOSHHazardModuleMapping`, never ingests them.

Unlike the OSHA tool, there's no natural "recurring source changed" trigger to schedule against
yet (no bulk feed to poll) — this tool is meant to be run whenever the seed file is curated/edited
by hand, or after swapping in a real scraper per the section above. No Task Scheduler
registration script is included for that reason; add one (mirroring
`tools/OshaStandardsImport/schedule/`) once there's an actual recurring source to schedule.
