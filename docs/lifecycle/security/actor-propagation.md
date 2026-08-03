# Lifecycle Actor/CorrelationId Propagation — Per-Transition Consolidation

Phase 7 of 10, Lifecycle Coverage Audit. Root: `C:\Users\dross\source`. Date: 2026-07-26.
Consolidates `rule-enforcement/_shared-findings.md` G2 (the two distinct actor/correlation bugs)
against `inventory.md`'s ~344-transition census, at the per-transition/per-aggregate level, and
answers directly: which of the two bugs hits which transitions, and is any transition genuinely
unaffected by either.

---

## Bottom line

**Zero of the ~344 lifecycle transitions enumerated by Phase 1 have confirmed, correct,
end-to-end Actor/CorrelationId propagation from the HTTP request to a persisted audit/outbox
record.** Every transition falls into exactly one of three buckets, and none of the three is
"correct":

- **Bucket A — reaches a real repository, and that repository's outbox call hardcodes the actor
  away (the universal C#-layer bug).** This is every transition on every one of the ~81 PlantSafety
  aggregates that have a working repository implementation — both Bug #1 alone, and, for a named
  23-aggregate subset, Bug #1 **and** Bug #2 stacked on the same transitions.
- **Bucket B — never reaches any repository at all**, because the aggregate has zero implementing
  class for its domain repository interface (`sql-shape/_shared-findings.md` G1's 13-aggregate
  finding). These transitions are technically "unaffected" by the outbox hardcoding bug, but only
  because the call fails at DI-container resolution before any actor-carrying code executes — not
  because propagation works.
- **Bucket C — DataLake/Intelligence/Azure**, which don't call the `outboxWriter.Enqueue` signature
  these two bugs live in at all, but each has its own, independently-confirmed actor-loss gap
  (different shape, same net effect: the persisted record does not know who acted).

Reporting has 0 transitions (no aggregates, no persistence — inherited, not re-derived).

---

## 1. The two bugs, restated precisely (inherited from G2, not re-derived)

- **Bug #1 (C#/outbox layer)**: `PlantSafety.Infrastructure\Persistence\UnitOfWork\OutboxWriter.cs`
  is called as `outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system",
  Guid.Empty.ToString())` at all 58 confirmed call sites — the literal strings `"system"` and
  `Guid.Empty` are hardcoded in the C# call, discarding whatever real `ActorId`/`CorrelationId`
  the command handler actually received via `CommandEnvelope`. The correct, already-built
  alternative — `IUnitOfWork.EnqueueEvent(domainEvent, tenantId, actorId, correlationId)`
  (`UnitOfWork.cs:40-41`) — has zero callers anywhere in the solution.
- **Bug #2 (SQL/AuditLog layer)**: a separate, independent defect — the T-SQL `AuditLog` INSERT
  embedded in each `usp_*_Insert`/`_Update` stored procedure for the 23 audit-logged aggregates
  binds `ChangedBy = SYSTEM_USER` (a SQL Server built-in reflecting the service account the
  connection authenticates as, never the application-level actor). Fixing Bug #1 (wiring
  `EnqueueEvent`) does **not** touch this — the SQL procs' `SYSTEM_USER` binding is untouched by
  any C#-layer change, and vice versa.

Both bugs land on the identical 23-aggregate population by coincidence of a shared copy-paste
template, not because they're the same defect.

## 2. Per-aggregate mapping — PlantSafety (309 transitions: 93 Create + 216 non-Create)

### 2a. The 23 aggregates hit by BOTH Bug #1 and Bug #2 (every transition on each)

| Aggregate (repository) | Transitions affected | Bug #1 (C# outbox hardcode) | Bug #2 (SQL `SYSTEM_USER`) |
|---|---|---|---|
| Accident (`AccidentDapperRepository`) | Create + Update/Close/AssignInvestigator/RecordRootCause/FileRegulatory/etc. | Yes | Yes |
| Risk (`RiskDapperRepository`) | Create + non-Create | Yes | Yes |
| SafetyPolicy (`SafetyPolicyDapperRepository`) | Create + Approve/Reject/non-Create | Yes | Yes |
| Scorecard (`ScorecardDapperRepository`) | Create + non-Create | Yes | Yes |
| PermitWorkflow, PermitTemplate, PermitExpiration, PermitScheduling (4 repos) | Create + non-Create, each | Yes | Yes |
| PPEAssignment, PPECategory, PPEInventory, PPETrainingFit, PPEZoneMapping (5 repos) | Create + non-Create, each | Yes | Yes |
| EmergencyDrillPlanning, HazardIsolationSequencing, PMScheduling, ShiftFatigue, WorkOrderPriority (5 repos) | Create + non-Create, each | Yes | Yes |
| Observability's 5 sub-aggregates (5 repos) | Create + non-Create, each | Yes | Yes |

23 repositories, every transition on each affected by both bugs simultaneously — this is the
single largest, most severe cluster: it includes the solution's best-guarded, richest aggregates
(`PermitWorkflow`, `AuditEngagement`-adjacent `SafetyPolicy`) — the ones a shallow audit would most
expect to be the "well-instrumented" examples.

### 2b. The remaining ~35 write-repositories hit by Bug #1 only (not confirmed to carry Bug #2)

Every other repository among the 58 total `outboxWriter.Enqueue(...)` call sites — including
`AccidentManagementCategory`, the `ContractorSafety` cluster (`ActiveMonitoringAuditing`,
`IncidentTrackingPerformanceEvaluation`, `PermitToWork` [ContractorSafety's own, distinct from
`PermitToWork.PermitWorkflow`], `PreQualificationVetting`, `SiteOrientation`), the `EmployeeSafety`
cluster (7 sub-aggregates), the `MachineSafety` cluster (6 sub-aggregates), the `SafetyProcedures`
cluster (8 sub-aggregates), `Checklists`/`ChecklistCompletion`/`ChecklistTemplate`,
`AuditEngagement`/`AuditFinding`, `SafetyCulture` (`CultureAssessment`, `StopWorkEvent`), and
`Dashboards.Dashboard`/`Widget` — hardcodes the same `"system"`/`Guid.Empty` literal at its own
`Enqueue` call site (Bug #1, confirmed universal, no exception found across all 58). This document
does not claim Bug #2 for these repositories' procs (not individually re-verified this phase which
of their stored procedures write an `AuditLog` row at all, vs. the 23 in §2a which were the ones
directly confirmed); **every one of these transitions is affected by at least Bug #1**, which
alone is sufficient to break actor/correlation attribution end-to-end.

### 2c. Aggregates that never reach either bug — because they never reach a repository at all

The 13 zero-implementation aggregates from `sql-shape/_shared-findings.md` G1 (`Culture`,
`ScoreCards` [Analytics — a duplicate of top-level `Scorecards.Scorecard`, itself in §2a],
`Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`, `Procedures`,
`RegulatoryCalendar` [Compliance — 7 real aggregates], `EmergencyManagement`, `IncidentManagement`,
`Observation` [SafetyEvents — 3 aggregates]) plus the 10-11 aggregates from `inventory.md`
(`AiGovernance`, `AiSafetyIntelligence`, `ManagementOfChange`, `RegulatoryCalendar`,
`ContractorVisitor`, `Governance`, `HumanFactors`, `OperationalSafety`, `ProcessDefinition`,
`Sustainability`, `WorkerWellBeing`) with **zero confirmed SQL persistence of any kind** overlap
substantially (`ManagementOfChange`/`RegulatoryCalendar` appear in both lists for the same reason:
no implementing class exists). For these ~15-18 real aggregates' Create transitions (these are
overwhelmingly the "create-only" cluster — Compliance and SafetyEvents' aggregates are all
Create-only per `inventory.md` §3), **calling `Create` fails at dependency-injection resolution
before `outboxWriter.Enqueue` is ever reached** — there is no C# code path here for either bug to
manifest in. This is the literal, correct answer to "any transition affected by neither bug" — but
it is not a success case. It is a third, more severe failure (the operation cannot execute at all),
consistent with `sql-shape/_shared-findings.md` G5's ranking of G1 as "the single worst — and most
solution-wide — shape-alignment finding" precisely because it's not recoverable by fixing a
validator or an outbox call; it requires writing the repository from scratch.

## 3. DataLake, Intelligence, Azure — a third failure shape, not an exemption

| Module | Transitions | Actor/CorrelationId shape | Affected by Bug #1/#2 as literally defined? | Actually correct end-to-end? |
|---|---|---|---|---|
| DataLake | 7 command-shaped transitions | `ActorId` never a first-class outbox column at all (Phase 2, reconfirmed no change) | No (different infra, no `IUnitOfWork.EnqueueEvent`/`OutboxWriter.Enqueue` call site exists here) | **No** — there's no column to hold the value in the first place, a strictly worse gap than "the column exists and is hardcoded" |
| Intelligence | 13 transitions (7 TrainingJob + 6 Investigation, both already confirmed guard-bypassed at runtime per `intelligence.md`) | Neither `TenantId` nor `ActorId` preserved past the API layer; `RequestingUserId` required at API but dropped | No | **No** |
| Azure | ~15 command-handler-shaped transitions | No `ActorId` anywhere in the module; `CorrelationId` only | No | **No** — half the pair is entirely absent, not just discarded |
| Reporting | 0 | No aggregates, no persistence, no lifecycle | N/A | N/A — nothing exists to evaluate |

**None of DataLake's/Intelligence's/Azure's transitions are "unaffected" in any meaningful sense —
they simply don't reach the two specifically-named PlantSafety bugs because they run on different
infrastructure. The underlying symptom (persisted record can't say who acted) is identical or
worse.**

## 4. Answering the phase's exact question: any transition affected by NEITHER bug — i.e., genuinely correct?

**No.** Restating the three buckets against the full ~344-transition population:

- **~15-18 real aggregates' Create transitions (PlantSafety, Bucket B/§2c)**: unaffected by Bug
  #1/#2 by name, but only because the operation cannot execute at all. Not correct.
- **~78 real aggregates' ~291 transitions (PlantSafety, Buckets A/§2a+§2b)**: universally hit by
  Bug #1; the 23-aggregate §2a subset additionally hit by Bug #2. Not correct.
- **DataLake (7) + Intelligence (13) + Azure (~15) = ~35 transitions**: a third failure shape,
  equally or more severe. Not correct.
- **Reporting (0)**: no transitions exist to be correct or incorrect.

Every single transition in the solution's ~344-row inventory lands in a bucket where actor/
correlation attribution is either broken, absent, or unreachable. **There is no transition — rich,
create-only, or otherwise — this phase or any prior phase found to have working end-to-end
Actor/CorrelationId propagation.** This is a stronger, more totalizing statement than the ingestion
discovery's own framing ("the 61-controller envelope pattern... is actor-aware only up to the
command object; it is not actor-aware end-to-end") — that document already showed the API layer
does its part correctly; this document's contribution is confirming that **zero** transitions
survive all the way through to a persisted record with that correctness intact, once the
repository/outbox and SQL-AuditLog layers are both accounted for.

## 5. Fix ordering, given both audits together

Per the ingestion discovery: rewiring the 58 `outboxWriter.Enqueue(...)` call sites to
`IUnitOfWork.EnqueueEvent(...)` (already-built, already-correct, zero new design work) fixes Bug #1
for every transition in §2a/§2b simultaneously, since both `ActorId` and `CorrelationId` flow
through the same unused method. It does **not** touch Bug #2 (the SQL `SYSTEM_USER` binding inside
each proc's `AuditLog` INSERT for the 23 §2a aggregates) — that requires a separate, per-stored-
procedure change to accept and bind a real `@ActorId`/`@ChangedBy` parameter instead of the T-SQL
built-in. Neither fix touches the §2c zero-implementation aggregates (a repository has to be
written before there's an outbox call to correct) or the DataLake/Intelligence/Azure gaps (a
different mechanism needs to be built, not corrected).

## Summary table

| # | Finding | Severity | Confidence |
|---|---|---|---|
| 1 | 23 PlantSafety aggregates' every transition is hit by **both** the C#-outbox hardcode and the SQL `AuditLog.ChangedBy=SYSTEM_USER` bug simultaneously | **High** — includes the solution's richest, best-guarded aggregates | High — inherited G2, cross-referenced against `inventory.md` transition counts |
| 2 | ~35 additional write-repositories (all other real PlantSafety aggregates) are hit by the C#-outbox bug alone | High | High |
| 3 | ~15-18 aggregates' transitions never reach either bug because no repository implementation exists to reach — a third, more severe failure mode, not a clean bill of health | Critical (per sql-shape G5's own ranking) | High — cross-referenced against sql-shape G1 |
| 4 | DataLake/Intelligence/Azure's ~35 transitions fail a different way (no first-class ActorId column/field at all) — not exempt, just differently broken | High | High — inherited, reconfirmed no change |
| 5 | **Zero of ~344 solution-wide transitions have confirmed correct end-to-end actor/correlation propagation** | Critical | High — first phase to state this as a totality across the full transition inventory |
