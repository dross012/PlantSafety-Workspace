# Phase 2 — Cross-Cutting Findings (referenced from every per-module file)

These four findings are established by **direct code read this phase** and apply, to varying
degrees of confirmation, across most/all of the 25 module files. Each per-module file references
this one by name rather than repeating the full explanation.

## F1 — Systemic tenancy-enforcement gap (NEW, most consequential finding of this phase)
Every repository's `GetByIdAsync(<AggregateId> id, ...)` method takes **only the aggregate's own
identifier** — never `TenantId`. Confirmed by direct read of:
`AccidentDapperRepository.GetByIdAsync` (inherited-cluster example),
`PermitToWorkDapperRepository.GetByIdAsync` (ContractorSafety, new cluster),
`MachineGuardingDapperRepository.GetByIdAsync` (EmployeeSafety, new cluster).
All three call a `usp_<Aggregate>_GetById` proc parameterized by `<Aggregate>Id` alone. The
corresponding command handlers (`ClosePermitToWorkCommandHandler`, `RetireMachineGuardingCommandHandler`,
`CloseAuditEngagementCommandHandler`, and by extension every other `Get→Mutate→Update` handler
built on the same `StoredProcedureBuilder`/`AggregateHydrator` template) load the aggregate by ID,
invoke the domain mutator, and save — **with no comparison of the acting caller's `TenantId` (from
the request envelope) against the loaded aggregate's own `TenantId` field, at either the repository
or the domain-method layer.** No aggregate's domain method signature (`Close`, `Retire`, `Approve`,
etc., across every aggregate read this phase) accepts or checks a caller tenant.
**Practical consequence: tenant isolation for every mutating transition in this inventory depends
entirely on `AggregateId` GUIDs being unguessable, not on any enforced check.** If a caller from
Tenant A learns/enumerates a Tenant B aggregate's GUID, every transition on it — Close, Approve,
Archive, Cancel, Retire, etc. — succeeds with no cross-tenant guard, because no guard exists to
trip. This is asserted as a **general pattern** (all read repositories share the identical
generated shape: `StoredProcedureBuilder().Add<AggregateId>(...)` and nothing else for GetById),
directly confirmed for 3 aggregates spanning old and new clusters, and pattern-inferred (same
generator, same shape) for the remainder. Per-row "Tenancy Rules" below record **"Stated only —
TenantId stored on record, not verified at read or write time (F1)"** unless a specific aggregate
was found to differ.

## F2 — Handler-bypass pattern (Phase 1 open question — does NOT recur outside Intelligence)
Read handlers across all 4 "new this phase" clusters (ContractorSafety, EmployeeSafety,
MachineSafety, SafetyProcedures) plus Auditing: `ClosePermitToWorkCommandHandler`,
`RetireMachineGuardingCommandHandler`, `CloseAuditEngagementCommandHandler`, and a `grep` across
every `EmployeeSafety`/`MachineSafety` command handler for a direct `_repository.<Verb>Async` call
(the Intelligence bypass shape) found **zero matches** — every handler in these clusters follows
`Load → aggregate.Verb(...) → repository.UpdateAsync(aggregate)`, i.e. the domain guard actually
executes. **Conclusion: the Intelligence handler-bypass pattern is an Intelligence-specific defect,
not a recurring PlantSafety pattern.** Confirmed for a representative sample (5 clusters); not
individually re-verified for every one of the 48 rich aggregates.

## F3 — Silent-archive pattern does NOT extend to the 3 newly-found Archive-havers
Direct read of `PermitTemplate.Archive`, `Dashboard.Archive`, and `Operational.Alerts.Alert.Archive`
method bodies: **all three call `AddDomainEvent`** (`PermitTemplateArchivedEvent`,
`DashboardArchivedEvent`, `AlertArchivedEvent` respectively). The "silent archive" defect
(guarded, status-transitioned, but never publishes an event) found in the prior discovery for
`Accident`/`Risk`/`Scorecard`/(presumed)`SafetyPolicy` is **confirmed NOT present** in these 3 new
instances — they are correctly instrumented. 8 confirmed Archive-havers total; 4 silent
(Accident, Risk, Scorecard, SafetyPolicy-presumed), 3 event-emitting (PermitTemplate, Dashboard,
Operational.Alerts.Alert), 1 not independently checked (`PermitToWork.PermitWorkflow.Archive` —
still only pattern-inferred).

## F4 — Alert duplicate cluster: resolved, and it is the 2nd "guarded but dead" lifecycle
Direct read of `PlantSafety.Api\Alerts\Controllers\V1\AlertsV1Controller.cs` (routes
`/api/v1/alerts/...`) confirms **all 8** of shape #1 (`Alerts.Alert`)'s mutating transitions
(Trigger/Acknowledge/Dispatch/Escalate/Resolve/Close/RecordDelivery/RecordResponseAction) are
wired to live `[HttpPost]` routes — this is the genuinely live shape.
Direct read of `PlantSafety.Api\Operational\Alerts\Controllers\V1\AlertsV1Controller.cs` **and**
`V2\AlertsV2Controller.cs` (routes `/api/v{v}/operational/alerts`) shows **both are GET-only** —
one real read query, one explicit "not yet implemented" stub. **No controller anywhere exposes a
POST/PUT route for shape #2 (`Operational.Alerts.Alert`)** — none of its 9 transitions
(Acknowledge/AddMetadata/Archive/AssignTo/Close/Escalate/Resolve/SetSlaExpiration/StartProgress),
each backed by a dedicated `AlertLifecycleGuard.EnsureCan*` guard, is reachable from any HTTP
route. **This is the richest, best-factored guarded lifecycle in the entire solution, and it is
100% dead code from the API's perspective** — the same category of finding as Intelligence's
guarded-but-bypassed aggregates, but here the guard is never even reached rather than reached-and-
bypassed. The 3rd Alert shape flagged in Phase 1 as "not re-located" was not found this phase either
(still open).

## F5 — ContractorSafety.PermitToWork vs PermitToWork.PermitWorkflow: confirmed distinct-but-overlapping (not a code duplicate)
Direct read of both aggregate files. Both model permit-to-work issuance for hazardous contractor
work and share the same real-world subject (draft/request → hazard assessment → issue → close/expire),
but they are **not the same state machine**:
- `PermitWorkflow` (`PermitToWork.PermitWorkflow`): 7-status formal workflow with hazard assessment
  and authorization as **separate** steps (`AssessHazards` → `ImplementControls` → `Approve` → `Issue`),
  a multi-stage `Reject` reachable from 4 different statuses, and `Archive` as a distinct terminal
  step after `Close`.
- `PermitToWork` (`ContractorSafety.PermitToWork`): simpler 6-status flow that folds assessment and
  authorization together (`RecordHazardAssessment` → `Issue` directly, no separate approval step),
  has confined-space-specific atmospheric-testing validation `RecordHazardAssessment` does not share
  with `PermitWorkflow`, and uniquely has **`Suspend`** (pause an issued permit for an unsafe
  condition) — a transition `PermitWorkflow` has no equivalent for at all.
**Conclusion: this is a genuine duplicate-concept risk (same real-world object — "permit to work" —
modeled twice with no cross-reference between the two aggregates), but not an identical/redundant
pair.** Practical risk: nothing in either aggregate or its handlers prevents the same physical permit
from being represented as both a `PermitWorkflow` and a `ContractorSafety.PermitToWork` row, with no
shared identity, no synchronization, and divergent status voccustionabularies — a data-integrity
exposure for compliance reporting (a query against one will silently miss permits recorded only in
the other), not a runtime-crash risk.

## F6 — New cross-module rule violation found
`CloseAuditEngagementCommandHandler` (`Auditing.AuditEngagement`) accepts `ComplianceScore` and
`ComplianceRating` as **raw caller-supplied input** on the close command and passes them straight
into `aggregate.Close(...)` with no query against the Compliance module (no
`ICompliance*Repository` dependency in the handler) to verify the score/rating against actual
Compliance-module state, and no check for open `AuditFinding`s before allowing Close. This is a
transition that plausibly *should* check another module's (or a sibling aggregate's) state but
does not — flagged as a new, this-phase-discovered cross-module rule gap, distinct from the
Accident/RegulatoryFilingRequired check (which is a same-aggregate field, not a real cross-module
call — no PlantSafety transition found this phase makes a genuine cross-aggregate-repository call
from within another aggregate's command handler).
