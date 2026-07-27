# SQL Error-Handling Modernization Plan

Documentation only — no SQL, code, or UI changes are made by this
document. It supersedes and sharpens Triage 4 (error handling) and
Triage 2 (concurrency) with module-level, cross-verified data gathered
specifically for this plan, and adds one significant finding neither
prior document surfaced.

## 1. TRY/CATCH Modernization Plan

### 1.1 The canonical pattern, precisely defined

The canonical pattern used where this codebase applies it fully (see
`usp_Incident_Create.sql` as the clearest reference) has **three
layers**, not one:

1. `BEGIN TRY` / `BEGIN CATCH` wrapping the procedure's real work.
2. On success, a best-effort `INSERT` into
   `[PlantSafety.AuditDB].dbo.AuditLog` (itself wrapped in its own inner
   `TRY`/`CATCH` so an audit-write failure can't break the main
   operation).
3. In the `CATCH` block, a structured `INSERT` into
   `[PlantSafety.ErrorDB].dbo.ErrorLog` (error number, severity, state,
   procedure, line, message, context), followed by `THROW` to preserve
   the original failure for the caller.

### 1.2 Adoption is far lower than the headline 31% figure suggests

Measuring each layer separately, across all 537 stored procedures:

| Layer | Count | % of 537 |
|---|---|---|
| `BEGIN TRY` present at all | 166 | 31% |
| ...and also writes to `AuditDB.AuditLog` | ~45–56 (two measurement methods, see note) | ~8–10% |
| ...and also writes to `ErrorDB.ErrorLog` in the `CATCH` block | 64 (of the 166 with TRY) | 12% of all procedures |

**Note on the 45 vs. 56 discrepancy**: two different greps were run for
this plan — one counting distinct `.sql` files matching
`AuditDB].dbo.[AuditLog]` (56) and one counting files matching the
broader `AuditDB].dbo` (which could include a handful of files that
merely *reference* the audit schema without inserting into `AuditLog`
specifically, or query it read-only). Both numbers tell the same story:
**the fully canonical three-layer pattern is present in roughly 8–12% of
all stored procedures, not 31%.** The 31% figure (raw `BEGIN TRY`
presence) substantially overstates how much of the codebase actually has
the complete, intended error-handling contract.

### 1.3 Categorized by module and risk level

Risk level here means: procedure count (larger = more surface area) ×
inverse of TRY/CATCH adoption (lower adoption = more exposure), the same
method Triage 4 used, cross-checked against whether each module has any
audit-write coverage at all.

**Tier 1 — Critical (large module, near-zero coverage of any layer)**

| Module | Procedures | TRY/CATCH % | Any audit-write procs |
|---|---|---|---|
| `Compliance` | 62 | 1% | **0** |
| `Analytics` | 22 | 0% | **0** |
| `Operational` | 26 | 3% | **0** |

**Tier 2 — High (large-to-medium module, low-to-moderate coverage, zero audit-write layer)**

| Module | Procedures | TRY/CATCH % | Any audit-write procs |
|---|---|---|---|
| `SafetyProcedures` | 40 | 40% | 0 |
| `EmployeeSafety` | 35 | 40% | 0 |
| `MachineSafety` | 30 | 40% | 0 |
| `ContractorSafety` | 25 | 40% | 0 |
| `CrossDomain` | 12 | 0% | 0 |
| `Integrations` | 14 | 7% | 0 |
| `Checklists` | 20 | 30% | 0 |
| `Auditing` | 16 | 25% | 0 (notably — a module named "Auditing" has zero procedures that write to the audit log) |
| `SafetyCulture` | 16 | 25% | 0 |

**Tier 3 — Moderate (has partial coverage across all three layers, meaningfully better than Tier 1/2)**

| Module | Procedures | TRY/CATCH % | Audit-write procs |
|---|---|---|---|
| `Scheduling` | 26 | 42% | 11 |
| `PPE` | 26 | 42% | 11 |
| `Observability` | 26 | 42% | 11 |
| `PermitToWork` | 21 | 42% | 9 |
| `SafetyEvents` | 18 | 55% | 3 |
| `AccidentManagement` | 7 | 42% | 3 |
| `RiskManagement` | 7 | 42% | 3 |
| `Scorecards` | 7 | 42% | 2 |
| `SafetyPolicies` | 8 | 37% | 2 |
| `ContinuousImprovement` | 6 | 50% | 1 |

**Tier 4 — Lower priority (small module footprint; still worth fixing but bounded blast radius)**

Every remaining module with fewer than ~10 procedures and 0% coverage:
`AiGovernance`, `Alerts`, `Dashboards`, `DigitalTwin`, `EnterpriseFabric`,
`EnvironmentalHealth`, `Features`, `Financials`, `HealthChecks`,
`Intelligence`, `PlanningProjects`, `ProcessManagement` (note: despite
being fixed for the `RowVersion` conversion bug and having 33%
TRY/CATCH, it has zero audit-write coverage), `Sustainability`,
`WorkerWellBeing`.

### 1.4 Safe, incremental rollout plan to 100%

**Reject a single-pass, all-537-procedures sweep.** Each procedure has to
be individually confirmed not to change behavior when wrapped — at this
scale, in one pass, that review burden is itself a major risk.

1. **Phase 0 (prerequisite, not this plan's job)**: restore integration
   test coverage per Section 4 below for whichever module is about to be
   touched — modernizing error handling in a module with zero test
   coverage means the "safe" rollout has no way to prove it didn't change
   behavior.
2. **Phase 1 — Tier 1 modules, write-path procedures only.** Within
   `Compliance`, `Analytics`, `Operational`, prioritize the procedures
   that back actual write operations (per the extraction blueprint,
   `Compliance` has only 3 real write actions — start there, not with
   its 59 read-only procedures) — a failed write with no audit trail is
   materially worse than a failed read.
3. **Phase 2 — Tier 2 modules**, same write-path-first approach.
4. **Phase 3 — bring Tier 3 modules from partial to full coverage** —
   these already have the pattern proven to work for a subset of their
   procedures; extending it to the rest of the same module is the lowest
   -risk category of this entire rollout, since the pattern is already
   locally established and reviewed.
5. **Phase 4 — Tier 4**, opportunistically, as those modules are touched
   for other reasons rather than as a dedicated pass (low absolute risk
   given small procedure counts).
6. **Throughout**: any procedure touched by Triage 1 (Culture) or Triage
   2/Section 2 below (concurrency work) should get the full three-layer
   pattern as part of that change, not deferred — this was already
   recommended in Triage 4 and is restated here as a hard rule, not a
   suggestion, given how low actual full-pattern adoption turned out to
   be (1.2 above).

## 2. Concurrency Unification Plan

### 2.1 The two named schemes — as directed

Per this request, the canonical scheme going forward is **ROWVERSION**.
This plan documents that decision's implications; it does not
re-litigate it, though for completeness: Triage 2, done before this
plan was requested, had independently recommended the opposite
(`UNIQUEIDENTIFIER`/`NEWID()`), based on the one confirmed production
defect being specific to ROWVERSION's string-conversion requirement and
that scheme's narrower existing footprint (24 vs. 87 files, on the raw
text-match count). **This plan proceeds with ROWVERSION as directed** —
flagged here only so the tradeoff is visible to whoever approves the
rollout, not to argue against the decision.

**Scheme A — `ROWVERSION`** (the now-canonical target): native SQL
Server type, requires the `CAST(... AS BINARY(8))` intermediate step
before any string projection (Triage 2/Step B's fixed defect) — this
requirement should be treated as a **mandatory, documented step** in any
new procedure using this scheme, not left to be independently
rediscovered.

**Scheme B — `ConcurrencyToken` / `UNIQUEIDENTIFIER` / `NEWID()`**: the
scheme being retired under this plan's direction.

### 2.2 Where each is actually used — corrected, table-level count

Triage 2's file-count numbers (24 / 87) mixed table declarations and
procedure references. This plan re-measured at the **table** level
specifically (237 total table files, matching Step F's count):

| Concurrency state | Table count | % of 237 |
|---|---|---|
| `RowVersion`-named `ROWVERSION` column (Scheme A, hand-rolled) | 4 tables (`Incident`, `CorrectiveAction`, `Process`, and one more matched by the broad pattern) | 1.7% |
| `ConcurrencyToken`-named `UNIQUEIDENTIFIER` column (Scheme B) | ~29 tables across `AccidentManagement`, `Observability`, `PPE`, `PermitToWork`, `RiskManagement`, `SafetyPolicies`, `Scheduling`, `Scorecards` | ~12% |
| `ConcurrencyToken`-named `ROWVERSION` column (the Triage-2-found, apparently-unused platform migration column) | subset of the remaining tables (not separately re-measured for this plan; Triage 2's finding that it's unreferenced by any procedure stands) | — |
| **No concurrency column of any kind** | **202 tables** | **85%** |

### 2.3 The actual headline finding: this is not primarily a two-scheme conflict

**The overwhelming majority of this schema — 202 of 237 tables, 85% —
has no optimistic concurrency control at all**, under either scheme.
Confirmed directly for `Compliance` (all 62 of its tables) and
`Analytics` (all 22 of its tables) — the two largest modules in the
codebase by table count have zero concurrency protection on any table.
Every module in Section 1's Tier 1/Tier 2 lists above also appears in
this 202-table population.

**This reframes the unification plan's actual priority**: standardizing
the *name and type* of concurrency column for the ~35 tables that already
have one is a much smaller undertaking than the real gap — most tables
in this schema can silently lose concurrent updates today (last write
wins, no conflict detection), independent of which scheme name wins.

### 2.4 Migration plan

**Phase 1 — do not touch the 35 tables that already work.** Neither
scheme has a *known* defect once Scheme A's conversion bug is fixed
(done, for the 4 affected tables). Migrating already-functioning
`ConcurrencyToken`/`UNIQUEIDENTIFIER` tables to `ROWVERSION` is pure churn
with a real regression window (every consumer of that table's ETag —
API, UI, any cached client state — has to change its assumption about
the token's format simultaneously) for a module that isn't broken today.
**Recommendation: only migrate a Scheme B table to Scheme A when that
table is being substantially modified for an unrelated reason anyway**
(the same "touch it, then fix it" principle as Section 1.4's rollout
rule) — not as a dedicated migration sprint.

**Phase 2 — close the 85% gap, using Scheme A (the now-canonical
choice) for every table that gets a concurrency column added.** This is
the actually valuable, large-scope work implied by "unification" — not
converting 35 working tables, but adding protection to 202 unprotected
ones. Prioritize by the same Tier 1/2/3/4 risk ordering as Section 1.4,
since it's largely the same set of large, low-coverage modules
(`Compliance`, `Analytics`, `Operational` first).

**Phase 3 — resolve the unused Mechanism-3 `ConcurrencyToken ROWVERSION`
scaffold** (Triage 2, section 1, Mechanism 3): for any table that already
has this dormant column, Phase 2's "add a Scheme A column" work may be
able to **reuse it** (rename `ConcurrencyToken` to `RowVersion` and wire
it into a real check) rather than adding a second, redundant column —
this needs per-table confirmation the column is genuinely unused
(Triage 2 already found no procedure reads it, but a rename should still
be verified against a live `ValidateOnBuild`/integration-test pass per
Section 4, not assumed safe from static analysis alone).

**Phase 4 — API/UI/client impact.** Every table that goes from "no
concurrency column" to "has one" changes its update contract — an
`UPDATE` that previously always succeeded (last-write-wins) can now fail
with a conflict. This is a **behavioral change for API/UI consumers**,
not a purely internal SQL change — see Section 5 for the regression
analysis.

## 3. Module-by-Module SQL Modernization Matrix

Sorted by risk (Tier 1 → Tier 4, matching Section 1.3), then by
procedure count descending within tier. "Concurrency" column uses A =
Scheme A (`ROWVERSION`), B = Scheme B (`UNIQUEIDENTIFIER`), — = none, ~
= partial/mixed across the module's tables.

| Module | Procs | TRY/CATCH | Audit-write | Concurrency | Integration test coverage | Priority |
|---|---|---|---|---|---|---|
| `Compliance` | 62 | 1% | 0% | — (none, 62/62 tables) | **None** | **1 — Critical** |
| `Analytics` | 22 | 0% | 0% | — (none, 22/22 tables) | **None** | **1 — Critical** |
| `Operational` | 26 | 3% | 0% | — (none, 26/26 tables) | **None** | **1 — Critical** |
| `SafetyProcedures` | 40 | 40% | 0% | — (none found) | None | 2 — High |
| `EmployeeSafety` | 35 | 40% | 0% | — (none found) | None | 2 — High |
| `MachineSafety` | 30 | 40% | 0% | — (none found) | None | 2 — High |
| `ContractorSafety` | 25 | 40% | 0% | — (none found) | None | 2 — High |
| `Scheduling` | 26 | 42% | 42% (11/26) | B (5/5 tables) | None (DI gap also reported, per extraction blueprint) | 2 — High |
| `PPE` | 26 | 42% | 42% (11/26) | B (5/5 tables) | None | 2 — High |
| `Observability` | 26 | 42% | 42% (11/26) | B (5/5 tables) | None | 2 — High |
| `Checklists` | 20 | 30% | 0% | ~ (2/5 tables have some column) | None | 2 — High |
| `PermitToWork` | 21 | 42% | 43% (9/21) | B (4/4 tables) | None | 3 — Moderate |
| `Features` | 21 | 52% | 0% | — (none found) | None | 3 — Moderate |
| `Integrations` | 14 | 7% | 0% | — (none found) | None | 2 — High (low coverage despite moderate size) |
| `CrossDomain` | 12 | 0% | 0% | — (none, 12/12 tables) | None | 2 — High |
| `Auditing` | 16 | 25% | 0% | ~ (2/3 tables) | None | 2 — High |
| `SafetyCulture` | 16 | 25% | 0% | ~ (2/3 tables) | None | 2 — High |
| `SafetyEvents` | 18 | 55% | 17% (3/18) | ~ (1 `RowVersion` table of 10; 9 with nothing) | **Partial** (Incident only, via `ApplyIncidentManagementSchemaAsync`) | 3 — Moderate (best-covered large module) |
| `AccidentManagement` | 7 | 42% | 43% (3/7) | B (1/2 tables) | **Yes** (`ApplyAccidentManagementSchemaAsync`) | 4 — Lower (already covered) |
| `RiskManagement` | 7 | 42% | 43% (3/7) | B (1/2 tables) | **Yes** (`ApplyRiskManagementSchemaAsync`) | 4 — Lower (already covered) |
| `Scorecards` | 7 | 42% | 29% (2/7) | B (1/2 tables) | None | 3 — Moderate |
| `SafetyPolicies` | 8 | 37% | 25% (2/8) | B (1/2 tables) | None | 3 — Moderate |
| `ContinuousImprovement` | 6 | 50% | 17% (1/6) | A (1/3 tables) | None (despite RowVersion bug fix landing here) | 3 — Moderate |
| `ProcessManagement` | 3 | 33% | 0% | A (1/1 table) | None (despite RowVersion bug fix landing here) | 3 — Moderate |
| `Financials`, `AiGovernance`, `Alerts`, `Dashboards`, `DigitalTwin`, `EnterpriseFabric`, `EnvironmentalHealth`, `HealthChecks`, `Intelligence`, `PlanningProjects`, `Sustainability`, `WorkerWellBeing` | ≤5 each | 0–75% (varies) | 0% each | mostly — | None | 4 — Lower (bounded blast radius) |
| `Culture` *(within `Analytics`, called out separately per Triage 1)* | 0 real procs (all missing) | N/A | N/A | N/A | None | **0 — Blocking** (not a modernization priority until it exists at all) |

## 4. Integration Test Coverage Plan

Restated and sharpened from Triage 3, specifically as a **prerequisite
gate** for the SQL modernization work above, not a parallel independent
effort.

### 4.1 Modules missing embedded schema

44 of 47 module folders (94%) — everything except `SafetyEvents`
(partial, Incident only), `RiskManagement`, `AccidentManagement`. This
includes **every single Tier 1 and Tier 2 module** from Section 1/3
above — the modules most in need of error-handling and concurrency
modernization are also the modules with zero ability to verify that
modernization didn't break anything.

### 4.2 Modules with broken repository registrations

Per Triage 3's exhaustive, twice-verified audit: **one** confirmed case
(`ICultureRepository`, Culture/`Analytics`), already being fixed. No
other DI-registration gaps were found across all 272 repository
interfaces checked. This is a narrower problem than the schema-coverage
gap (4.1) — it blocks the *entire* test host from booting at all
(Step C), which is a precondition even more fundamental than per-module
schema coverage: **until this lands, zero integration tests can run for
any module**, coverage gap or not.

### 4.3 Sequencing: coverage must precede modernization, module by module

1. **Global prerequisite**: land the `ICultureRepository` fix (already
   in progress) so the shared test host builds at all.
2. **Per-Tier-1-module prerequisite**: before any error-handling or
   concurrency change lands in `Compliance`, `Analytics`, or
   `Operational`, that module needs embedded schema + at least one
   Create/Update/Read integration test proving the *current* behavior,
   so the modernization change has something to be diffed against.
   Building that test is itself valuable independent of the
   modernization work — it's the only way to know today's actual
   behavior in modules with 0–3% error-handling coverage and no
   concurrency control.
3. **This ordering is a hard dependency, not a preference**: modifying a
   62-procedure, 0%-tested module's error handling and adding
   concurrency columns to it in the same pass, with no test proving
   pre-change behavior, is exactly the scenario most likely to introduce
   a silent regression — the two riskiest changes in this entire plan,
   stacked on the single least-verified module in the codebase.

## 5. Risk & Regression Assessment

### 5.1 Risk of modernizing SQL before integration tests are restored

- **No regression detection.** For the 44 modules with zero embedded
  schema, a behavior change (a procedure that now throws where it
  silently continued before, or an `UPDATE` that now fails on a
  concurrency conflict where it previously always succeeded) has no
  automated way to be caught before it reaches a real environment.
- **Compounding with Tier 1's near-zero existing coverage** — `Compliance`
  and `Analytics` are simultaneously the largest, least-tested, and
  least-protected modules. Any error in sequencing (modernizing before
  testing) has its largest blast radius exactly where the plan's own
  data says risk is highest.

### 5.2 Risk of concurrency unification specifically

- **Behavioral change, not just internal refactor, for the 202
  currently-unprotected tables.** Adding a concurrency column changes an
  `UPDATE` from "always succeeds" to "can fail on conflict" — any API
  handler or UI flow that doesn't already handle a concurrency-conflict
  response will need to. This is the same class of change Step 3
  (Incident aggregate migration) made deliberately and carefully for one
  module; doing it across 202 tables without equivalent care risks
  introducing unhandled-exception paths in production API code that
  currently has no reason to expect an `UPDATE` to ever fail this way.
- **ETag/token format changes ripple to every consumer.** If a
  Scheme-B-to-Scheme-A migration (Section 2.4, Phase 1) is ever done for
  a table with live UI/API consumers, every client holding a
  `UNIQUEIDENTIFIER`-shaped token has to be updated simultaneously to
  expect a `ROWVERSION`-derived hex string instead — a coordinated,
  breaking change, not a silent internal one. This is exactly why
  Section 2.4 recommends against migrating already-working tables absent
  another reason to touch them.
- **The Mechanism-3 column reuse idea (Section 2.4, Phase 3)** carries a
  narrower but real risk: static analysis found it unreferenced, but
  "unreferenced by any stored procedure in this repo" is not the same
  guarantee as "safe to rename" — Section 4's integration-test
  prerequisite applies here specifically before any such rename.

### 5.3 Regression risk for Reporting, UI, and event bus consumers

- **Reporting**: per Step D/Phase 3 findings, Reporting's read model is
  fed by domain events from `IncidentManagement`/`CorrectiveAction`
  specifically — modules in this plan's Tier 3/4 (already
  moderately covered), not Tier 1. Direct regression risk to Reporting
  from *this* plan's highest-priority work (Tier 1: `Compliance`,
  `Analytics`, `Operational`) is currently low, **because those modules
  don't yet publish domain events Reporting consumes** — but that also
  means if/when they do get event-bus integration in the future, doing
  so on top of unmodernized error handling and concurrency would import
  the same risk into the event pipeline at that point. Sequencing
  message: modernize before extending event-bus coverage to Tier 1
  modules, not after.
- **UI**: per the UI readiness matrix (Triage 5), the modules with
  confirmed or inferred backend gaps (`Culture`, `Compliance`, and by
  extension anything mapped to `Analytics`/`Operational`) already have
  existing UI pages built against them. Any SQL modernization to these
  modules needs to be checked against the specific UI flows in
  `PlantSafety.UI/Modules/Culture`, `Modules/Compliance`, and any module
  this triage mapped (with "Inferred" confidence) to `Analytics` or
  `Operational` — a concurrency column added to a table a UI write-flow
  targets could surface as a new, previously-impossible error condition
  the UI has no handling for.
- **Event bus consumers generally**: `Shared.EventBus`'s own retry/
  dead-letter handling (Phase 3 work) is agnostic to which module
  publishes an event — it doesn't need to change for this plan's work.
  The risk is entirely on the publishing side (a modernized procedure
  that starts throwing where it didn't before could, if that procedure
  sits inside a transaction that also enqueues an outbox event, cause an
  event that previously always fired to now sometimes not fire) — this
  is the same category of risk Step D flagged as unresolved for
  `IncidentManagement`'s outbox-write atomicity, and applies equally to
  any Tier 1/2 module that later gains event-bus integration on top of
  this plan's changes.
