# Step F — SQL Schema Stability Assessment

**Scope**: whether the current SQL shape (tables + stored procedures under
`PlantSafety.Infrastructure`) is stable enough to build future UI/API
modernization on top of. This is an assessment, not a remediation plan —
no schema changes are proposed here.

## 1. Scale

- **537** stored procedure files (`usp_*.sql`) across all modules.
- **237** table definition files.
- This is a large, long-lived schema with substantial historical
  accretion — stability findings below should be read as "at this scale,
  X% consistency" rather than expecting uniformity from a smaller,
  newer codebase.

## 2. Error-handling consistency — significant gap

Only **166 of 537** stored procedures (31%) use the `BEGIN TRY`/
`BEGIN CATCH` pattern that this codebase's own documentation (e.g.
`usp_Incident_Create.sql`'s header, which explicitly cites
"TRY/CATCH with AuditDB/ErrorDB logging per the pattern already
established") describes as canonical. The remaining 69% have no
structured error handling — a failure inside those procedures propagates
as a raw, unlogged SQL exception rather than being captured into the
centralized `[PlantSafety.ErrorDB].dbo.ErrorLog` table the canonical
pattern writes to.

**Stability implication**: this is not a correctness bug in the 69% (they
may work correctly for their happy path), but it's an **observability and
diagnosability gap** — a UI/API modernization effort that starts
surfacing more edge cases (validation failures, concurrent writes, malformed
input) will get inconsistent error visibility depending on which
procedure happens to be involved, purely based on when it was written
rather than what it does.

## 3. Concurrency pattern split — two coexisting conventions

Two different optimistic-concurrency patterns are in active use:

- **`ROWVERSION`-backed** (native SQL Server `timestamp` column,
  projected as a hex-string ETag): **24** files. This is the pattern that
  had the `CONVERT(NVARCHAR, RowVersion, style)` bug (Steps B and this
  session's earlier work) — a real, previously-undetected defect that
  would fail on every affected procedure against a real SQL Server, now
  fixed for `SafetyEvents.Incident`, `ContinuousImprovement.CorrectiveAction`,
  and `ProcessManagement.Process`.
- **`ConcurrencyToken`-backed** (a `NEWID()`-generated token column,
  regenerated on every write): **87** files — the more common pattern.

**Stability implication**: this split is not itself a bug — both are
valid optimistic-concurrency strategies — but it means any future
API/UI work touching concurrency (e.g. a generic "conflict resolution"
UI component, or a shared client-side ETag-handling library) cannot
assume a single format across modules and must handle both. The
`ROWVERSION` pattern's now-fixed bug is also a caution: it was a
**copy-paste-propagated defect** (the broken conversion pattern was
explicitly cited in comments as copied from `ProcessManagement.usp_Process_GetById`
into `SafetyEvents.Incident`, `ContinuousImprovement.CorrectiveAction`),
meaning the 24 `ROWVERSION`-pattern files should be treated as a single
population that shares risk — if one had this specific bug, a
targeted audit of the remaining ones for other subtle SQL-Server-specific
gotchas (not just this one) would be reasonable before relying on the
pattern further, though none were found in this session beyond the
already-fixed instances.

## 4. Confirmed gaps found this session (not resolved by this document)

- **`Analytics.Culture`**: the C# repository layer calls
  `usp_Culture_Insert`/`usp_Culture_GetById`/`usp_Culture_GetAll` — **none
  of which exist anywhere in the SQL tree** (Step A). This is a complete
  gap, not a partial/inconsistent one: the module has zero working
  persistence today, independent of the DI registration bug that was the
  original trigger for finding this.
- **`Compliance`** (per the Scheduling/Maintenance/Compliance extraction
  blueprint, `scheduling-maintenance-compliance-extraction-blueprint.md`):
  8 sub-aggregates, 124 SQL scripts, but only 3 of ~124 API actions are
  non-`GET`, and none of those 3 touch the Domain aggregate layer at all
  — the SQL surface is broad, but almost entirely read-only in practice,
  with write logic bypassing the domain model the same anemic way
  pre-migration `IncidentManagement` did.
- **`Scheduling`**: `SchedulingInfrastructureServiceRegistration.cs` is,
  per its own header comment, explicitly not yet wired into the shared
  DI root — a real but narrower gap than Culture's, since the SQL and
  aggregate layers exist, it's specifically the registration that's
  incomplete.

None of these are addressed by this document — they're cited here because
they materially affect the answer to "is the SQL shape stable," and a
stability assessment that omitted known, session-confirmed gaps would be
misleading.

## 5. Soft-delete convention — partial, and that's likely fine

95 of 237 tables (40%) have an `IsDeleted BIT` column; 142 don't. This is
**not inherently a stability problem** — audit/log tables, read-model
projection tables, and pure join/lookup tables legitimately don't need
soft-delete semantics. No attempt was made in this assessment to classify
which of the 142 *should* have it but don't, versus which correctly don't
need it — that classification would require per-table domain knowledge
this document doesn't have. **Flagged as a question, not a finding**: if
UI modernization work needs a generic "list with trash/restore" pattern,
confirm on a per-module basis whether the target tables actually support
it before assuming they all do.

## 6. What appears genuinely stable

- The `[TenantId]`-scoped, `[Module]`-schema-qualified naming convention
  (`[SafetyEvents].[Incident]`, `[Analytics].[Culture]`, etc.) is
  consistent across every file inspected during this session — no
  naming-convention violations were found.
- The canonical header-parameter contract (`@TenantId`, `@CorrelationId`,
  `@ActorId`, `@NowUtc`) is consistently present in every procedure this
  session directly inspected across `SafetyEvents`, `ContinuousImprovement`,
  and `ProcessManagement`.
- No `EF`/ORM coupling was found anywhere — the "No EF, No Drift" rule
  appears genuinely, consistently honored; every data access path
  inspected this session used Dapper + raw stored procedure calls.
- The `[PlantSafety.AuditDB]`/`[PlantSafety.ErrorDB]` cross-database
  logging pattern, where used (the 31% with TRY/CATCH), is applied
  consistently in shape — same table names, same column sets, same
  `TRY`-wrapped best-effort semantics — not reinvented per-module.

## 7. Recommended next steps (assessment-level, not a remediation plan)

1. Treat the Culture, Compliance, and Scheduling gaps (section 4) as
   pre-existing blockers to any UI/API work that would depend on them
   specifically — not blockers to modernization work in modules that
   don't touch them.
2. Before broad UI modernization begins, decide whether the TRY/CATCH
   gap (section 2) needs closing project-wide, or only in modules the UI
   work will actually touch — closing it for 537 procedures is a large
   undertaking not justified purely by this assessment.
3. Document the two-concurrency-pattern split (section 3) somewhere a
   frontend/API-client engineer would find it before they build a
   generic optimistic-concurrency UI component that assumes one shape.
