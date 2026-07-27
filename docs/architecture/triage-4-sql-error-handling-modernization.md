# Triage 4 — SQL Error-Handling Modernization Plan

## 1. Per-module breakdown (corrects Step F's single aggregate 31% figure)

Step F reported an aggregate 166/537 (31%) `BEGIN TRY` adoption rate
across the whole codebase. That number obscures enormous per-module
variance — some modules are fully covered, others have essentially none:

| Module | TRY/CATCH adoption | Procedures |
|---|---|---|
| `Persistence` | 100% | 7/7 |
| `Alerts` | 75% | 3/4 |
| `SafetyEvents` | 55% | 10/18 |
| `Features` | 52% | 11/21 |
| `ContinuousImprovement` | 50% | 3/6 |
| `Dashboards` | 50% | 2/4 |
| `PermitToWork` | 42% | 9/21 |
| `AccidentManagement`, `RiskManagement`, `Scorecards` | 42% | 3/7 each |
| `Observability`, `PPE`, `Scheduling` | 42% | 11/26 each |
| `Financials` | 40% | 2/5 |
| `SafetyProcedures` | 40% | 16/40 |
| `EmployeeSafety` | 40% | 14/35 |
| `MachineSafety` | 40% | 12/30 |
| `ContractorSafety` | 40% | 10/25 |
| `SafetyPolicies` | 37% | 3/8 |
| `AiGovernance`, `DigitalTwin`, `EnterpriseFabric`, `ProcessManagement`, `Sustainability` | 33% | 1/3 each |
| `Checklists` | 30% | 6/20 |
| `Auditing`, `SafetyCulture` | 25% | 4/16 each |
| `Integrations` | 7% | 1/14 |
| `Operational` | 3% | 1/26 |
| **`Compliance`** | **1%** | **1/62** |
| **`Analytics`** | **0%** | **0/22** |
| `CrossDomain` | 0% | 0/12 |
| `Intelligence` | 0% | 0/4 |
| `EnvironmentalHealth`, `PlanningProjects`, `WorkerWellBeing` | 0% | 0/2 each |
| `HealthChecks` | 0% | 0/1 |

(Modules with zero `.sql` procedure files are omitted — this table covers
every module that has at least one stored procedure.)

## 2. Worst offenders, weighted by actual surface area

Raw percentage alone understates risk for large modules. Ranked by
**absolute count of procedures with no error handling**:

1. **`Compliance` — 61 of 62 procedures with no TRY/CATCH.** The single
   largest procedure count of any module in the codebase, combined with
   the near-total absence of structured error handling. This compounds
   with an already-known finding (the Scheduling/Maintenance/Compliance
   extraction blueprint): `Compliance`'s write path is already anemic
   (only 3 of ~124 API actions write, none touch the domain layer) — a
   module that's simultaneously large, under-tested (Triage 3: not
   covered by any embedded integration-test schema), architecturally
   thin on the write side, and now confirmed to have almost no
   structured SQL error handling. This is the single highest-risk module
   surfaced across this entire triage series.
2. **`Analytics` — 22 of 22 procedures with no TRY/CATCH**, including
   whichever Culture procedures get written per Triage 1's plan — if
   those are added without TRY/CATCH, they'd merely be following the
   module's own (poor) existing convention. **This is a live decision
   point for whoever implements Triage 1**: don't let "matching the
   existing Analytics convention" become the justification for skipping
   error handling on brand-new procedures.
3. **`Operational` — 25 of 26**, **`SafetyProcedures` — 24 of 40**,
   **`EmployeeSafety` — 21 of 35**, **`MachineSafety` — 18 of 30** — all
   large modules with the majority of their procedures uncovered.

## 3. What "no TRY/CATCH" actually costs

Per the canonical pattern this codebase already uses where it exists
(e.g. `usp_Incident_Create.sql`): TRY/CATCH wraps the real work, and the
CATCH block writes a structured record to
`[PlantSafety.ErrorDB].dbo.ErrorLog` (error number, severity, state,
procedure name, line, message, and a module-specific context string)
before re-`THROW`-ing. A procedure without this pattern doesn't fail
differently — the underlying error still happens and still propagates —
but it fails **invisibly** from an operations standpoint: nothing gets
written to the centralized error log, so there's no queryable record
that the failure happened at all, and it becomes purely a Dapper-side
`SqlException` the caller has to know to log itself. For the 40 modules
below 50% adoption, roughly two-thirds of a typical failure's operational
visibility is missing by default.

## 4. Safe rollout plan

**Do not attempt a single sweep across all ~370 uncovered procedures** —
that's a large, mechanical-looking change that's actually high-risk to
do in bulk, since each procedure's existing body has to be correctly
wrapped without altering its logic, return values, or transaction
semantics, across dozens of authors' worth of stylistic variance.

### Phase 1 — stop the bleeding on new/actively-changing code

- Any procedure touched by Triage 1 (Culture), Triage 2 (concurrency
  unification), or any future module work should get TRY/CATCH as part
  of that change, not as separate follow-up debt — it's a small marginal
  cost when a procedure is already being written or edited, and a much
  larger one to retrofit later.
- Establish this as an explicit review criterion (not proposed as a
  Roslyn/tooling enforcement mechanism here — that's a separate,
  larger decision) for any new stored procedure PR.

### Phase 2 — close the gap in `Compliance`, prioritized by write-path relevance

Given `Compliance`'s write path is already known to be thin (3 real write
actions per the extraction blueprint), prioritize TRY/CATCH coverage for
**those specific write procedures first** — the ones an actual user
action depends on — rather than all 62 procedures uniformly. Read-only
procedures are lower-risk (a failed `SELECT` is generally easier to
diagnose from the client side than a failed write with no audit trail).

### Phase 3 — close the gap in `Analytics`, coordinated with Triage 1

Since `Analytics` currently has zero coverage and Triage 1's Culture work
will be adding new procedures to this module regardless, use that work as
the natural point to establish the pattern for the module, then extend it
to the other 22 existing `Analytics` procedures as a follow-on, scoped
piece of work — not blocking Triage 1 itself.

### Phase 4 — the remaining modules, by descending absolute-count risk

`Operational` (25 uncovered), `SafetyProcedures` (24),
`EmployeeSafety` (21), `MachineSafety` (18), `ContractorSafety` (15),
`Checklists` (14), `Auditing`/`SafetyCulture` (12 each), `Integrations`
(13), `CrossDomain` (12) — roughly in that order, each as its own scoped
pass so any regression introduced by the mechanical wrapping is isolated
to one module's review, not a single sprawling cross-cutting PR.

## 5. Explicitly out of scope for this plan

Whether the canonical TRY/CATCH + `AuditDB`/`ErrorDB` pattern itself is
the right long-term error-handling strategy (vs., say, a different
centralized logging approach) is not re-litigated here — this plan
assumes the existing, already-adopted-in-31%-of-the-codebase pattern is
correct and simply under-applied, since no evidence was found during this
triage series suggesting the pattern itself is flawed, only that its
adoption is inconsistent.
