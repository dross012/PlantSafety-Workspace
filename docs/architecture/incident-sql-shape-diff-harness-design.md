# Incident SQL-Shape Diff Harness — Design (Not Implemented)

## Status

**Design only.** This document describes how a fixture-based diff harness
*could* be built to compare the pre-aggregate and post-aggregate Incident
Create/Update handlers. It is **not implemented and has not been run**.

## Why this exists, and why it's a partial substitute

The original 7-step hardening plan for the `IncidentManagement` aggregate
called for a step 6: "run both versions against a production-like dataset
and diff the results" before merging. That requires access to a production
or production-shaped dataset, which this environment does not have — there
is no live PlantSafety database, no anonymized production export, and no
sanctioned way to fabricate one convincingly.

What follows is the next-best thing: a **synthetic fixture diff**, using
hand-written representative and edge-case inputs instead of real data. It
can catch a behavioral divergence between old and new code paths on the
scenarios it's given, but it **cannot** catch skew from real-world data
shapes this design didn't anticipate (legacy rows with unexpected `NULL`s
from an earlier schema version, unusual Unicode in `Title`/`Description`,
timestamps far outside any test's expected range, etc.). Treat a clean run
of this harness as evidence of "no *known* regression," not as equivalent
to a real production-shape audit.

## What actually changed, scoped correctly

Per `docs/architecture/plantsafety-architecture.md` §5 and the commit
history (`cab2b017` = last pre-aggregate commit touching the Incident
handlers, `f9fc782c` = the aggregate migration), the change is narrow:

- Both versions call the **same stored procedures**
  (`usp_Incident_Create`, `usp_Incident_Update`) with, for the most part,
  the **same parameter set**.
- The new version adds **validation that didn't exist before** — required-
  field checks and the archived-state update guard, enforced by the
  aggregate's `Create`/`Rehydrate`/`UpdateDetails` methods before any SQL
  runs.
- The new version's `UpdateAsync` still uses the original
  null-means-unchanged diff parameters against the same
  `ISNULL`/`COALESCE` partial-update procedure — it does not switch to a
  full-column overwrite.

This means the realistic failure mode isn't "the new code writes different
column values for the same accepted input" (the parameter mapping is
largely unchanged) — it's **"the new code now rejects an input the old
code would have silently accepted and written."** The harness design below
is built around detecting that specific class of divergence, since it's
the one this migration could plausibly introduce.

## Harness design

### 1. Two code paths, two isolated databases

The old and new handler code cannot coexist in one build — both define
`CreateIncidentCommandHandler`/`UpdateIncidentCommandHandler` in the same
namespace. The harness would need:

- A **temporary git worktree** checked out at commit `cab2b017` (`git
  worktree add ../plantsafety-pre-aggregate cab2b017`), built in isolation,
  exposing just enough of `PlantSafety.Application` +
  `PlantSafety.Infrastructure` to construct and invoke the old handlers
  directly (bypassing MediatR/the API layer — a thin `Program.cs`-style
  console entry point, not a full host).
- The **current working tree**, built and invoked the same way, calling
  the new aggregate-based handlers.
- **Two separate Testcontainers SQL Server instances** (or one instance,
  two schemas) so each path writes to its own copy of
  `[SafetyEvents].[Incident]` with no cross-contamination — reusing
  `TestDatabaseFactory`'s schema-application logic (now fixed — see the
  `RowVersion`→`NVARCHAR` conversion bug fixed alongside this design pass)
  against each.

### 2. Fixture set

A fixed list of `CreateIncidentCommand`/`UpdateIncidentCommand` inputs,
covering:

- **Golden path**: minimal valid Create, minimal valid Update.
- **Boundary lengths**: `Title` at 200 chars (the column max), `Description`
  at 4000 chars, `Location` at 500 chars — one at the limit, one one
  character over (expected to be rejected by both paths equally, since
  the stored procedure's column definition enforces this regardless of
  which handler called it).
- **Whitespace/normalization edge cases**: leading/trailing whitespace in
  `Severity`. Confirmed via `git show cab2b017:.../CreateIncidentCommandHandler.cs`:
  the old handler passed `request.Severity` straight through to the SQL
  parameters with no trimming, while the new aggregate's `Create`
  explicitly trims it (`normalizedSeverity = (severity ??
  string.Empty).Trim()`). This is a **confirmed, real behavioral
  difference** for whitespace-padded input — a fixture with
  `Severity = "  Minor  "` will persist `"Minor"` in the new path and
  `"  Minor  "` in the old path. This is almost certainly a desirable
  fix, not a regression, but it's exactly the kind of silent value change
  this harness exists to surface rather than assume.
- **Unicode**: non-ASCII characters in `Title`/`Description`/`Location`.
- **Update partial-diff combinations**: all-null (no-op update except
  metadata), single-field-set, all-fields-set.
- **Update against an archived incident**: expected to succeed silently
  in the old path (no invariant existed) and be rejected with
  `IncidentManagementInvalidStatusTransitionException` in the new path —
  this is the **expected, intentional** divergence the migration exists
  to introduce. The harness should assert this divergence occurs, not
  flag it as a bug.
- **Stale/invalid ETag on Update**: both paths should reject identically
  (unchanged `ConcurrencyConflictException` behavior at the SQL layer).
- **Required-field omission** (empty `Title`, empty `ReportedByWorkerId`):
  expected to succeed in the old path (no validation) and be rejected in
  the new path — another intentional, expected divergence.

### 3. Execution and comparison

For each fixture:

1. Run it through the old path against database A; capture success/failure
   and, on success, the full persisted row (reusing the raw
   `SELECT * FROM [SafetyEvents].[Incident] WHERE IncidentId = @id`
   pattern from `IncidentSqlPayloadSnapshotTests.ReadIncidentRowAsync`).
2. Run the equivalent fixture through the new path against database B;
   capture the same.
3. Classify the result:
   - **Same accept/reject outcome, same persisted values** → no
     divergence.
   - **Same accept/reject outcome, different persisted values** → real
     regression, needs investigation (the `Severity.Trim()` case above is
     the most likely candidate).
   - **New path rejects, old path accepted** → check against the "expected
     divergences" list (archived-update guard, required-field
     enforcement). If it's on the list, that's correct new behavior. If
     not, it's an unintended new restriction — a regression.
   - **New path accepts, old path rejected** → should not happen (the new
     path is strictly more restrictive than the old one); if it does,
     that's a correctness bug in the new validation logic.
4. Emit a report (fixture name → classification) rather than hard
   pass/fail assertions, since some divergences are intended.

### 4. Where this would live, if built

As a disposable console tool (e.g. under a `tools/incident-shape-diff/`
directory), **not** inside `PlantSafety.Tests` or
`PlantSafety.IntegrationTests` — it depends on checking out a second git
worktree and running two database instances simultaneously, which doesn't
fit the fast, hermetic unit/integration test conventions used elsewhere in
this repo. It should be treated as a one-time audit tool, run once before
this migration is considered fully validated, then archived or deleted —
not maintained as ongoing CI coverage (`IncidentSqlPayloadSnapshotTests.cs`
already provides the ongoing regression coverage for the *new* path's
shape; this harness's job is a one-time comparison against the *old*
path, which is only interesting until confidence in the migration is
established).

## Recommendation

Given the change is narrowly scoped and the two most likely divergence
points (the archived-update guard and required-field enforcement) are
**intentional and already covered** by the handler-level tests added in
the aggregate migration itself, the marginal value of building this full
two-worktree, two-database harness is probably lower than its
implementation cost — the one genuinely new risk it would catch
(the `Severity.Trim()` normalization case, or something similarly subtle
in the field mapping) could likely be checked faster with a couple of
targeted unit tests comparing the aggregate's normalized output against
the exact bytes the old handler would have sent. Building the full harness
is worth it if this migration pattern (anemic handler → aggregate) is
about to be repeated across many more modules and a reusable comparison
tool would pay for itself; for a single aggregate, it's likely overkill.
