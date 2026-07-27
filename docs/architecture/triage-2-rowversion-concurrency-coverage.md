# Triage 2 — Cross-Module Concurrency Scheme Coverage Plan

## 1. Correction to the Step F assessment

Step F described a two-way split ("ROWVERSION-backed" vs
"ConcurrencyToken-backed"). Deeper investigation for this triage found
the actual picture is **three distinct mechanisms**, and one of them was
completely missed by Step F's search (which only matched the literal
column name `RowVersion`, not `ConcurrencyToken`):

### Mechanism 1 — hand-rolled `RowVersion`-named ROWVERSION column

Used by: `SafetyEvents.Incident`, `ContinuousImprovement.CorrectiveAction`,
`ProcessManagement.Process` (the three modules fixed in Step B/this
session's earlier work). Native SQL Server `ROWVERSION` type, column
literally named `RowVersion`, projected to a string ETag via
`CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)`. **This is the
mechanism that had the real, confirmed conversion bug** (now fixed in all
three modules).

### Mechanism 2 — hand-rolled `ConcurrencyToken`-named UNIQUEIDENTIFIER column

Used by (confirmed via direct inspection of table DDL): `AccidentManagement`,
`PPE`, `PermitToWork`, `Scheduling`, `RiskManagement`, `SafetyPolicies`,
`Scorecards`, `Observability`, and others (23 table files total matched
`ConcurrencyToken UNIQUEIDENTIFIER`). Pattern, confirmed via
`AccidentManagement/Sql/Tables/Accident.sql` and
`usp_Accident_Update.sql`:

```sql
-- Table:
ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Accident_Concurrency DEFAULT (NEWID())

-- On update:
ConcurrencyToken = NEWID()
```

This is a self-contained, GUID-to-GUID comparison — **not** subject to
the `ROWVERSION`-to-string conversion bug, since no `CONVERT`/`CAST` of a
`timestamp` type is involved anywhere in this mechanism.

### Mechanism 3 — platform-wide bolted-on `ConcurrencyToken`-named ROWVERSION column (newly found, appears unused)

A single migration script,
`PlantSafety.Infrastructure/Persistence/Migrations/Phase2/20-CoreDB-rowversion.sql`
(plus sibling scripts for `ArchiveDB`, `CloudIdentityDB`,
`DiagnosticsDB`, `ExportImportDB`, `IntegrationDB`, `PredictiveDB`,
`ReportingDB`), adds:

```sql
IF COL_LENGTH('[<Schema>].[<Table>]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [<Schema>].[<Table>] ADD ConcurrencyToken ROWVERSION NOT NULL;
END
```

...to **35 tables across 16 schemas** in `20-CoreDB-rowversion.sql`
alone (`AiGovernance`, `AiSafetyIntelligence`, `Analytics`, `Compliance`,
`ContractorVisitor`, `DigitalTwin`, `EnterpriseFabric`,
`EnvironmentalHealth`, `Governance`, `HumanFactors`, `Operational`,
`OperationalSafety`, `ProcessManagement`, `SafetyEvents`, `Sustainability`,
`WorkerWellBeing` — plus the other seven database-scoped migration
scripts, not individually itemized here). The `COL_LENGTH ... IS NULL`
guard means it **skips** any table that already has its own
`ConcurrencyToken` column (i.e. skips Mechanism 2 tables) — so this is
specifically targeting tables that had **no** concurrency column at all
before this migration ran.

**Critical finding**: a repo-wide search for any stored procedure that
reads, compares, or projects this `ConcurrencyToken` column (`SELECT`,
`WHERE`, `AS ETag`, or any reference outside the migration scripts
themselves) returns **zero matches**. This column appears to have been
added platform-wide as scaffolding and **never wired into any actual
optimistic-concurrency check or API contract** — every table it touches
currently has no working concurrency protection despite having a column
that implies it does.

**Also flagged, not resolved by this document**: the migration targets
`[SafetyEvents].[IncidentManagement]` — a table name distinct from the
real, working `[SafetyEvents].[Incident]` table this session's other work
centered on. Given Phase 2's aggregate redesign already found and deleted
a dead `IncidentManagementItems` read-model scaffold in the same
namespace, `[SafetyEvents].[IncidentManagement]` may be a similar dead
remnant rather than a real table — **this needs direct verification
(does the table exist? is it referenced by any procedure?) before
assuming it's live**, which was outside this triage's scope to confirm.

## 2. Inconsistency summary

| Mechanism | Column name | SQL type | Confirmed wired into a real check | Known defect |
|---|---|---|---|---|
| 1 | `RowVersion` | `ROWVERSION` | Yes (3 modules) | Had the conversion bug — now fixed |
| 2 | `ConcurrencyToken` | `UNIQUEIDENTIFIER` (NEWID) | Yes (8+ modules) | None found |
| 3 | `ConcurrencyToken` | `ROWVERSION` | **No — appears universally unused** | N/A (nothing to break, since nothing reads it) |

The same column name (`ConcurrencyToken`) means two different things
depending on which mechanism a given table happens to use — a real
consistency hazard for anyone writing new code against this schema
without knowing which mechanism a specific table uses.

## 3. Unification plan

**Do not attempt a single big-bang migration** — three years of
accumulated, module-specific concurrency logic across ~500 procedures
makes that disproportionately risky relative to the actual problem (a
naming collision and one dead scaffold column, not a correctness
emergency across all three mechanisms simultaneously).

### Phase 1 — resolve the immediate ambiguity (low risk, high value)

1. **Determine whether Mechanism 3's `ConcurrencyToken ROWVERSION`
   columns are truly dead** across all 16+ schemas, or whether some
   newer/future code was written expecting to use them and just hasn't
   landed yet. If confirmed dead, either wire them into real checks
   (Phase 2 below) or remove them — leaving unused optimistic-concurrency
   scaffolding in 35+ tables is a footgun for any future developer who
   assumes a column's presence means it's enforced.
2. **Resolve the `[SafetyEvents].[IncidentManagement]` vs
   `[SafetyEvents].[Incident]` naming question** definitively — confirm
   which table is real, and if `IncidentManagement` is dead, remove it
   from the migration script's target list (or the table itself, if it
   exists and nothing uses it) to stop it from silently coexisting with
   the real `Incident` table under a confusingly similar name.
3. **Document the two genuinely-live mechanisms (1 and 2) with an
   explicit decision table** — "if a table has a `RowVersion` column, use
   pattern X; if it has a `ConcurrencyToken` UNIQUEIDENTIFIER column, use
   pattern Y" — so future module authors don't have to reverse-engineer
   which one applies by reading table DDL each time.

### Phase 2 — pick a canonical pattern for new modules going forward

Recommend **Mechanism 2 (`ConcurrencyToken` / `UNIQUEIDENTIFIER` /
`NEWID()`)** as the canonical pattern for any new module, not Mechanism
1, for two concrete reasons:

- It has a wider proven track record in this codebase (8+ modules vs. 3)
  with zero known defects, versus Mechanism 1's one confirmed,
  previously-shipped-broken bug.
- It requires no `CONVERT`/`CAST` gymnastics to expose as a string
  token — a `UNIQUEIDENTIFIER` serializes to a string trivially and
  correctly in every context, removing an entire class of
  SQL-Server-conversion-subtlety risk that Mechanism 1 demonstrated is
  real, not theoretical.

This is **not** a recommendation to migrate the three already-working
Mechanism-1 modules (`Incident`, `CorrectiveAction`, `Process`) —
they're fixed, tested, and working; migrating them to a different
mechanism now would be pure churn with no correctness benefit.

### Phase 3 — retire or repurpose Mechanism 3

Once Phase 1's investigation confirms Mechanism 3 is genuinely unused,
either:

- **Remove it** from the 35+ tables it was added to, if no near-term
  plan exists to use it (simplest, lowest-risk option — an unused column
  can always be re-added later if needed), or
- **Wire it into real checks** for whichever modules it was intended for,
  following Mechanism 2's pattern rather than Mechanism 1's, per the
  Phase 2 recommendation above — but only for modules that don't already
  have a working mechanism of their own.

## 4. What this triage does not claim

This document does not assert that Mechanism 3's columns are safe to
drop without further investigation — "appears unused" based on a
stored-procedure-text search is strong evidence, not proof (application
code, ORMs, or ad-hoc tooling outside this repo could theoretically
reference it, though this codebase's own "no EF, Dapper + stored
procedures only" convention makes that unlikely). Phase 1's
investigation step exists specifically to convert this from "appears
unused" to a confirmed fact before Phase 3 acts on it.
