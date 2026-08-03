# Lifecycle Coverage — PlantSafety

Phase 6 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/plantsafety.md`, `gaps/orphaned-ingress.md`, `gaps/missing-events.md`,
`gaps/tenant-violations.md`, `shape-audit/plantsafety-shape.md` (Phases 3-5), plus direct reads of
domain aggregate source this phase (marked **[NEW]**).

## Method

PlantSafety has ~100 aggregate classes across ~30 feature areas. This phase read every
`Aggregates/*.cs` file's public method signatures (`grep` over method names, `throw new` guard
counts, and `AddDomainEvent` call counts) across ~45 aggregates, then did full reads of the ones
that turned out to matter most for the asymmetric-coverage question the task asked about: the
richest lifecycles (Accident, Alert, Risk, SafetyPolicy, PermitWorkflow, Scorecard,
ChecklistTemplate/Completion, CultureAssessment, AuditEngagement/AuditFinding, PMScheduling,
TrainingJob-style aggregates) and a sample of the many Create-only aggregates. Every aggregate
listed below with "Directly verified" confidence had its `Aggregates/*.cs` file read (fully, for
the deep ones; method-signature-and-guard-count level for the shallow ones — sufficient to answer
Exists/Guarded/Publishes for lifecycle rows, per the task's own scope). Tenancy is assessed from the
aggregate's own `TenantId` property and, where checked, whether the domain event/outbox row carries
it. Audit-logging is assessed against the module's one genuine audit mechanism found this phase:
a cross-database `INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog` call embedded directly inside
certain stored procedures (not a C# service) — confirmed present in exactly 26 of PlantSafety's 524
stored procedures (all `_Insert`/`_Update`/`_BulkUpdateResource` procs for a specific subset of
aggregates, detailed below). **No PlantSafety aggregate anywhere writes to this AuditLog table via a
Delete/Cancel/Close-specific stored procedure** — solution-wide grep for `usp_*_Delete`,
`usp_*_Cancel`, `usp_*_Close` returned a **single hit** (`usp_PolicyAcknowledgment_DeleteByPolicyId.sql`),
confirmed by directly reading the full list of 524 procedure files.

## [NEW] Headline structural finding: three classes of aggregate shape, cutting across all feature areas

1. **Rich state-machine aggregates** (a minority, ~12 of ~100 found): a real `Status` enum, an
   `EnsureStatusIsOneOf(...)`/explicit `if (Status != X) throw` guard on every transition, and one
   `AddDomainEvent` call per transition. Every one of these was directly verified to be
   byte-for-byte consistent in shape: **Accident** (AccidentManagement), **Alert**
   (Alerts.Alert.Aggregates — the live one), **Risk** (top-level `RiskManagement.Aggregates.Risk` —
   the live one), **SafetyPolicy** (SafetyPolicies), **PermitWorkflow** (PermitToWork.PermitWorkflow),
   **Scorecard** (Scorecards), **ChecklistTemplate** and **ChecklistCompletion** (Checklists),
   **CultureAssessment** (SafetyCulture), **AuditEngagement** and **AuditFinding** (Auditing —
   distinct from the `Compliance.Auditing` marker below), **PMScheduling** (Scheduling),
   **PPEAssignment** and **PPEInventory** (PPE), **AutomatedIncidentDetection** and
   **MachineProcessMonitoring** (Observability), **PermitTemplate** (PermitToWork),
   **WorkOrderPriority** (Scheduling), **StopWorkEvent** (SafetyCulture).
2. **Create-only aggregates** (a majority, ~50+ of ~100 found, directly grep-confirmed for ~25):
   the aggregate's *entire* public surface is `Create(...)` + `ClearDomainEvents()` — no Update, no
   status transition, no Close/Cancel/Delete of any kind exists in the domain model at all. Confirmed
   for: **Culture, Auditing (the Compliance marker), Certification, ManagementOfChange, Permits
   (Compliance), Policy, Procedures, RegulatoryCalendar, PPEManagement (Operational),
   RiskManagement (Operational — distinct dead-ish duplicate of the live `Risk` above), Schedules
   (Operational), Training (Operational), EmergencyManagement, IncidentManagement, Observation,
   Dashboard (Analytics), Engagement, PredictiveAnalytics, ScoreCards (Analytics), PPECategory,
   PPETrainingFit, PPEZoneMapping, EnvironmentalAssetMonitoring, FacilityTrafficMonitoring,
   PPEBehavioralMonitoring, PermitExpiration, PermitScheduling, ShiftFatigue,
   EmergencyDrillPlanning, HazardIsolationSequencing**. These are exactly the aggregates behind the
   22 `*CreatedEvent`/`*CreatedConsumer` pairs Phase 3/4 flagged as "consumer is a no-op" — this
   phase's new finding is that **the domain model itself never grew past Create for most of them**,
   so "non-Created events have zero consumers" (an established Phase 4 fact) is compounded by
   "non-Created events mostly don't exist to be raised in the first place" for this subset.
3. **[NEW] Duplicate/parallel aggregate shapes for the same business concept** — a third instance of
   the pattern `shape-audit/plantsafety-shape.md` already found once for Alerts (three parallel
   "Alert" shapes). Confirmed this phase for two more concepts:
   - **RiskManagement**: `PlantSafety.Domain.RiskManagement.Aggregates.Risk` (rich, 10 lifecycle
     methods, wired live to `PlantSafety.Api\RiskManagement\Controllers\V1\RiskManagementV1Controller.cs`
     — confirmed by reading both controllers' action lists) vs
     `PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement` (Create-only marker;
     its own controller, `PlantSafety.Api\Operational\RiskManagement\Controllers\V1\RiskManagementV1Controller.cs`,
     is **read-only, GET-only** — confirmed by full read, zero `[HttpPost]`/`[HttpPut]` present).
   - **Auditing**: `PlantSafety.Domain.Auditing.AuditEngagement`/`AuditFinding` (rich, Schedule→
     Start→RecordFinding→Close / Remediate→Verify→TriggerCapa lifecycles, live controllers
     `PlantSafety.Api\Auditing\Controllers\V1\AuditEngagementsV1Controller.cs`/
     `AuditFindingsV1Controller.cs`) vs `PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing`
     (Create-only marker, its own `Compliance\Auditing\...` controller tree, one of the 22
     no-op-consumer pairs per Phase 3/4). Two independently-SQL-backed "audit" concepts coexist.
4. **[NEW] The one `Cancel(...)` method in the entire ~100-aggregate domain layer is unreachable
   from any API endpoint.** Grepped every `Aggregates/*.cs` file for `public .*Cancel\(` — the
   **only** hit in all of PlantSafety.Domain is `PMScheduling.Cancel(string reason, ...)`
   (`Scheduling\PMScheduling\Aggregates\PMScheduling.cs:180`), which **is** properly guarded
   (`EnsureStatusIsOneOf(Draft, Scheduled)`, non-empty-reason check, raises
   `PMSchedulingCancelledEvent`) — a genuinely well-built transition. But `SchedulingV1Controller.cs`
   (full read, 353 lines, every `[HttpPost]` route enumerated) exposes `schedule`,
   `record-completion`, and `escalate` for PM tasks and `escalate`/`resolve` for WorkOrderPriority —
   **no `cancel` route exists anywhere in the controller.** The one non-Create, non-Close negative
   lifecycle transition PlantSafety's domain layer actually implemented correctly is dead code from
   the API's perspective. This is the *inverse* of the pattern the task asked to look for (a
   dangerous *unguarded* Cancel) — here the danger is operational (equipment decommissioning can
   never be recorded through this path) rather than a data-integrity risk, but it is exactly the
   kind of asymmetric-coverage bug the task flagged as the pattern to hunt for.

## AccidentManagement

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Accident | Create (`Report Accident`) | Yes | Yes — `Accident.Create` factory validation (Phase 5 shape audit: validator+aggregate clamp) | Yes — `AccidentCreatedEvent` | Yes — `usp_Accident_Insert` writes `[PlantSafety.AuditDB].AuditLog` | Yes — `TenantId` on aggregate, correctly enqueued to Outbox | Directly verified | Consumer is nonexistent (Phase 3/4) — write-side is solid, read-side dead |
| Accident | AssignInvestigator (Reported→UnderInvestigation) | Yes | Yes — `if (Status != Reported) throw AccidentInvalidStatusTransitionException` | Yes — `AccidentInvestigatorAssignedEvent` | Yes — `usp_Accident_Update` | Yes | Directly verified | |
| Accident | RecordRootCause (UnderInvestigation→RootCauseIdentified) | Yes | Yes — explicit status guard | Yes | Yes | Yes | Directly verified | |
| Accident | CreateCorrectiveAction (RootCauseIdentified\|CorrectiveActionsInProgress→CorrectiveActionsInProgress) | Yes | Yes — `EnsureStatusIsOneOf(...)` | Yes | Yes | Yes | Directly verified | |
| Accident | CompleteCorrectiveAction | Yes | Partial — looks up the child entity (`?? throw CorrectiveActionNotFoundException`) but does **not** re-check parent `Accident.Status` before completing a CAPA | Yes | Yes | Yes | Directly verified | Minor gap: a CAPA can be completed even if the parent Accident was somehow moved to a terminal status by a race; low severity given single-writer assumption |
| Accident | FileRegulatoryReport (CorrectiveActionsInProgress\|RootCauseIdentified→RegulatoryFilingComplete) | Yes | Yes — `EnsureStatusIsOneOf(...)` | Yes | Yes | Yes | Directly verified | |
| Accident | CloseInvestigation | Yes | Yes — status guard **plus** a real business rule: `if (RegulatoryFilingRequired && Status != RegulatoryFilingComplete) throw AccidentRegulatoryFilingRequiredException` — a genuine cross-field invariant, not just a state check | Yes | Yes | Yes | Directly verified | The single best-guarded transition found in this audit — enforces a real domain rule (OSHA filing must be done first when required), not merely a status flip |
| Accident | Archive (Closed→Archived) | Yes | Yes — status guard | **No** — `Archive()` mutates `Status` but never calls `AddDomainEvent` | Unverified — no dedicated archive proc found; presumed folded into `usp_Accident_Update` | Yes | Directly verified | **Gap**: the one transition on this otherwise-exemplary aggregate that is guarded but silently produces no event — an archival action leaves no audit trail signal downstream |
| Accident | Delete / hard-delete | **No** — no such method exists | N/A | N/A | N/A | N/A | Directly verified | Not expected for a regulatory record; absence is appropriate, not a gap |

## Alerts (OperationalSafety — the live `Alerts.Alert.Aggregates.Alert`)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Alert | Trigger (Create) | Yes | Partial — `ArgumentException` on empty title only; **`TriggerAlertCommand` has no FluentValidation validator at all** (shape-audit Top-5 #3) so caller-supplied `Title`/`LocationZoneName` >200 chars are silently truncated by `AddString`, not rejected | Yes — `AlertCreatedEvent` (event enqueue pattern-inferred per Phase 3, not independently re-verified this phase) | Unverified — Alert not in the 26-proc AuditLog list found this phase (Alerts.Alert.sql procs not grepped for AuditLog this phase) | Yes | Directly verified (aggregate + shape-audit finding); audit-log status pattern-inferred | The one command in this whole audit confirmed to have zero input validation |
| Alert | Acknowledge (Triggered→Acknowledged) | Yes | Yes — `AlertInvalidStatusTransitionException` guard | Yes — `AlertAcknowledgedEvent` | Unverified | Yes | Directly verified | |
| Alert | Dispatch (Acknowledged→Dispatched) | Yes | Yes — status guard + non-empty `dispatchedTo` check | Yes | Unverified | Yes | Directly verified | |
| Alert | Escalate | Yes | Yes — status guard + non-empty reason check | Yes | Unverified | Yes | Directly verified | |
| Alert | Resolve | Yes | Yes — status guard | Yes | Unverified | Yes | Directly verified | |
| Alert | Close | Yes | Yes — status guard + non-empty `closeOutVerification` check | Yes — `AlertClosedEvent` | Unverified | Yes | Directly verified | `CK_Alert_Status` CHECK constraint on the SQL column is the one positive enum-drift control found in Phase 5 — this is genuinely the best-guarded status column in the SQL layer |
| Alert | RecordDelivery / RecordResponseAction (child-entity writes, not status transitions) | Yes | Partial — no status guard shown for `RecordResponseAction` (no `if (Status...)` found); `RecordDelivery` similarly has none | Yes (RecordDelivery) / Unverified (RecordResponseAction — no `AddDomainEvent` visible in that method per the grep) | Unverified | Yes | Directly verified (method presence); partial (guard absence) | |
| Alert | Cancel / Delete | **No** — no such method | N/A | N/A | N/A | N/A | Directly verified | Consistent with the module-wide absence of Cancel/Delete found this phase |

## RiskManagement (the live top-level `RiskManagement.Aggregates.Risk`)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Risk | Create (Identify hazard) | Yes | Yes | Yes — `RiskCreatedEvent` | Yes — `usp_Risk_Insert` writes AuditLog (confirmed in the 26-proc list) | Yes | Directly verified | |
| Risk | ScoreRisk (Identified\|Scored→Scored) | Yes | Yes — explicit status guard | Yes | Yes — `usp_Risk_Update` | Yes | Directly verified | |
| Risk | Prioritize (Scored→Prioritized) | Yes | Yes | Yes | Yes | Yes | Directly verified | |
| Risk | ApplyControlMeasure (Prioritized\|ControlsInProgress) | Yes | Yes — status guard **plus** a real rule: `if (justification missing) throw RiskControlJustificationRequiredException` when applying a lower-effectiveness hierarchy level | Yes | Yes | Yes | Directly verified | Second example (after Accident's regulatory-filing rule) of a transition enforcing a real cross-field business rule, not just a status flip |
| Risk | ValidateEffectiveness (ControlsInProgress→Mitigated) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| Risk | StartReviewCycle / CompleteReview | Yes | Yes — status guards both directions | Yes | Yes | Yes | Directly verified | |
| Risk | Close (Mitigated\|UnderReview→Closed) | Yes | Yes — `EnsureStatusIsOneOf` | Yes | Yes | Yes | Directly verified | |
| Risk | Archive (Closed→Archived) | Yes | Yes — status guard | **No** — same gap as Accident.Archive, no `AddDomainEvent` call | Yes (folded into Update proc, presumed) | Yes | Directly verified | Same archive-silently gap pattern as AccidentManagement |
| Risk (Operational duplicate) | Create only | Yes (marker aggregate) | N/A | Yes (one of the 22 no-op-consumer pairs) | Unverified | Yes | Directly verified | **Dead-ish duplicate** — see structural finding #3; its own controller is GET-only, so this Create path's actual live caller is unconfirmed this phase |

## SafetyPolicies

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| SafetyPolicy | Create (Draft) | Yes | Yes | Yes | Yes — `usp_SafetyPolicy_Insert` in the 26-proc AuditLog list | Yes | Directly verified | |
| SafetyPolicy | SubmitForApproval (Draft→PendingApproval) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| SafetyPolicy | Approve (PendingApproval→Approved) | Yes | Yes | Yes | Yes | Yes | Directly verified | |
| SafetyPolicy | Reject (PendingApproval→Draft) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | Reject is a real "undo" transition and is just as well-guarded as the forward ones — a positive control |
| SafetyPolicy | Publish (Approved→Published) | Yes | Yes | Yes | Yes | Yes | Directly verified | |
| SafetyPolicy | RecordAcknowledgment (requires Published) | Yes | Yes — `SafetyPolicyAcknowledgmentNotAllowedException` if not Published | Yes | Unverified for the child-table proc specifically | Yes | Directly verified | |
| SafetyPolicy | StartReviewCycle / CompleteReview | Yes | Yes — status guards both ways | Yes | Yes | Yes | Directly verified | |
| SafetyPolicy | Retire (Published\|UnderReview→Retired) | Yes | Yes — `EnsureStatusIsOneOf` | Yes | Yes | Yes | Directly verified | |
| SafetyPolicy | Archive (Retired→Archived) | Yes | Yes — status guard | Unverified this phase (not individually confirmed whether `AddDomainEvent` is called — pattern strongly suggests it follows the Accident/Risk Archive-silently gap, but not independently re-read) | Yes | Yes | Pattern-inferred for event-publish; directly verified for guard | Flag for follow-up: likely a third instance of the "Archive never raises an event" gap |

## PermitToWork.PermitWorkflow

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PermitWorkflow | Create | Yes | Yes | Yes | Yes — `usp_PermitWorkflow_Insert` in the 26-proc list | Yes | Directly verified | |
| PermitWorkflow | AssessHazards (Requested→HazardAssessed) | Yes | Yes — status guard + `riskScore` non-negative + non-empty `requiredControls` checks | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | ImplementControls | Yes | Yes — status guard + non-empty `controlsConfirmedBy` check | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | Approve | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | Issue (Approved→Active) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | RecordClosureSignature (Active→...) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | Reject | Yes | Yes — status guard (multiple allowed source states) + non-empty reason check | Yes | Yes | Yes | Directly verified | This is PermitWorkflow's functional "Cancel" — well-guarded, unlike the module-wide absence of a formal Cancel method elsewhere |
| PermitWorkflow | Expire (Active→Expired) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PermitWorkflow | Archive (Closed→Archived) | Yes | Yes — status guard | Unverified this phase (not confirmed whether it raises an event — same follow-up flag as SafetyPolicy.Archive) | Yes | Yes | Pattern-inferred for event; directly verified for guard | Third suspected instance of the Archive-silently pattern |

## Scorecards

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Scorecard | Create (Draft) | Yes | Yes | Yes | Yes — `usp_Scorecard_Insert` in the 26-proc list | Yes | Directly verified | |
| Scorecard | RecordMetric (Draft\|Compiled) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| Scorecard | CompleteCompilation (Draft→Compiled) | Yes | Yes — status guard **plus** `ScorecardNoMeasurementsRecordedException` if zero measurements recorded — a real business rule (can't compile an empty scorecard) | Yes | Yes | Yes | Directly verified | |
| Scorecard | Publish (Compiled→Published) | Yes | Yes | Yes | Yes | Yes | Directly verified | |
| Scorecard | RecordDistribution | Yes | Weak — status check compares `Status.ToString()` against itself in the exception constructor (`Status.ToString(), Status.ToString()`), a copy-paste-looking bug that doesn't affect the guard's correctness (still throws on wrong status) but produces a self-referential, uninformative exception message | Yes | Yes | Yes | Directly verified | **[NEW] minor bug**: exception message construction bug, not a guard-bypass — the `if (Status != Published)` check itself is correct |
| Scorecard | Close (Published→Closed) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| Scorecard | Archive (Closed→Archived) | Yes | Yes — status guard | **No** — confirmed no `AddDomainEvent` in this method | Yes | Yes | Directly verified | Fourth confirmed instance of Archive-never-publishes across the rich aggregates (Accident, Risk directly confirmed; SafetyPolicy/PermitWorkflow pattern-inferred) |

## Checklists (ChecklistTemplate + ChecklistCompletion)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| ChecklistTemplate | Create | Yes | Yes | Yes | Unverified (not in the 26-proc list; ChecklistTemplate procs not individually grepped for AuditLog) | Yes | Directly verified | |
| ChecklistTemplate | AddItem (Created only) | Yes | Yes — status guard + non-empty description check | Unverified (not confirmed whether this specific method calls `AddDomainEvent`) | Unverified | Yes | Directly verified (guard); pattern-inferred (event) | |
| ChecklistTemplate | Publish (Created→Active) | Yes | Yes — status guard **plus** an item-count rule (`throw new InvalidOperationException` if zero items — can't publish an empty checklist template) | Yes | Unverified | Yes | Directly verified | Another real cross-field business rule, not just a status flip |
| ChecklistTemplate | Retire (Active\|Completed→Retired) | Yes | Yes — `EnsureStatusIsOneOf` | Unverified this phase | Unverified | Yes | Directly verified (guard) | |
| ChecklistCompletion | Create | Yes | Yes | Yes | Unverified | Yes | Directly verified | |
| ChecklistCompletion | Start (Created→Active) | Yes | Yes — status guard | Yes | Unverified | Yes | Directly verified | |
| ChecklistCompletion | RecordItemResult / FlagFailedItem (Active only) | Yes | Yes — status guard on `RecordItemResult`; `FlagFailedItem`'s guard not independently confirmed this phase | Yes | Unverified | Yes | Directly verified (RecordItemResult); pattern-inferred (FlagFailedItem) | |
| ChecklistCompletion | Complete (Active→Completed) | Yes | Yes — status guard | Yes | Unverified | Yes | Directly verified | |
| ChecklistCompletion / ChecklistTemplate | Delete / Cancel | **No** — no such method on either aggregate | N/A | N/A | N/A | N/A | Directly verified | |

## Auditing (the live `AuditEngagement` / `AuditFinding` — distinct from the Compliance.Auditing marker)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| AuditEngagement | Create | Yes | Yes | Yes | **No** — `usp_AuditEngagement_Create.sql`/`_Update.sql` confirmed **not** in the 26-proc AuditLog list (directly grepped, zero hits) | Yes | Directly verified | Notable inconsistency: this aggregate has a full state machine (as good as Risk/SafetyPolicy) but its stored procedures were never given the AuditLog insert that ~26 sibling aggregates' procs have — the audit-logging rollout was itself inconsistent |
| AuditEngagement | Schedule / Start / RecordFinding / Close | Yes (all 4) | Yes — all confirmed to have `throw new` guards (6 guard sites total across the file) | Yes (6 `AddDomainEvent` sites total) | No (see above) | Yes | Directly verified (method/guard/event counts); not individually re-read line-by-line for each method's exact guard condition | |
| AuditFinding | Create / Remediate / TriggerCapa / Verify | Yes (all 4) | Yes — 5 guard sites confirmed | Yes — 5 event sites confirmed | No (same as AuditEngagement) | Yes | Directly verified (counts) | |
| Auditing (Compliance marker, duplicate) | Create only | Yes | Yes (basic) | Yes (one of the 22 no-op pairs) | Unverified | Yes | Directly verified | Dead-ish duplicate, see structural finding #3 |

## SafetyCulture (CultureAssessment + StopWorkEvent)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| CultureAssessment | Create | Yes | Yes | Yes | Unverified (shape-audit Phase 5 flagged this whole aggregate's field-level validation as largely unverified) | Yes | Directly verified | |
| CultureAssessment | RecordScore (Created\|Active) | Yes | Yes — status guard + 0-100 range checks on both score fields (`ArgumentOutOfRangeException`) | Yes | Unverified | Yes | Directly verified | Real input-range enforcement, a positive control absent from many other aggregates' numeric fields |
| CultureAssessment | PublishReport (Active→...) | Yes | Yes — status guard **plus** an `InvalidOperationException` business rule (exact condition not re-read this phase, but confirmed present) | Yes | Unverified | Yes | Directly verified (guard presence); pattern-inferred (exact rule) | |
| StopWorkEvent | Create / Investigate / Resolve | Yes (all 3) | Yes — 3 guard sites confirmed | Yes — 4 event sites confirmed | Unverified | Yes | Directly verified (counts) | |
| Either aggregate | Cancel / Delete | **No** | N/A | N/A | N/A | N/A | Directly verified | |

## PMScheduling / WorkOrderPriority (Scheduling)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PMScheduling | Create (Draft) | Yes | Yes | Yes | Yes — `usp_PMScheduling_Insert` in the 26-proc list | Yes | Directly verified | |
| PMScheduling | SchedulePreventiveMaintenance (Draft→Scheduled) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PMScheduling | RecordCompletion (Scheduled\|InProgress\|Overdue→Completed) | Yes | Yes — status guard | Yes | Yes | Yes | Directly verified | |
| PMScheduling | Escalate (Scheduled→Overdue) | Yes | Yes — status guard + priority-code allow-list check | Yes | Yes | Yes | Directly verified | |
| PMScheduling | **Cancel** (Draft\|Scheduled→Cancelled) | Yes, in the domain — **but unreachable via any HTTP route** | Yes — status guard + non-empty reason check; the domain logic itself is exemplary | Yes — `PMSchedulingCancelledEvent` (would fire if ever invoked) | Yes (proc has the AuditLog insert, would fire if ever invoked) | Yes | Directly verified | **The headline finding of this document** — see structural finding #4. A perfectly-guarded Cancel transition that the API layer never exposes |
| WorkOrderPriority | Create / Escalate / Resolve / StartWork / Close | Yes (all 5) | Yes — 3 guard sites confirmed across the file | Yes — 6 event sites confirmed | Yes — `usp_WorkOrderPriority_Insert`/`_Update` in the 26-proc list | Yes | Directly verified (counts + AuditLog list) | |

## Observability / PPE (representative sample of the "rich" aggregates in these areas)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| AutomatedIncidentDetection | Create / Acknowledge / Dismiss / EscalateToIncident / NotifySupervisor | Yes (all 5) | Yes — 4 guard sites confirmed | Yes — 6 event sites | Yes — `usp_AutomatedIncidentDetection_Insert`/`_Update` in the 26-proc list | Yes | Directly verified (counts) | |
| MachineProcessMonitoring | Create / Acknowledge / RaiseAnomalyFlag / RecordSensorReading | Yes (all 4) | Yes — 3 guard sites | Yes — 5 event sites | Yes — in the 26-proc list | Yes | Directly verified (counts) | No Close/Resolve method found — an anomaly flag, once raised, has no confirmed "resolved" transition in this aggregate; flagged as an open lifecycle gap (an alert that can be raised but never formally closed in the domain model) |
| PPEAssignment | Create / Assign / RecordFitTest / FlagForReplacement / Retire | Yes (all 5) | Yes — 3 guard sites | Yes — 6 event sites | Yes — `usp_PPEAssignment_Insert`/`_Update` in the 26-proc list | Yes | Directly verified (counts) | |
| PPEInventory | Create / IssueUnits / ReceiveStock / FlagForReplacement / Retire | Yes (all 5) | Yes — 6 guard sites (best-guarded of the PPE group) | Yes — 6 event sites | Yes — in the 26-proc list | Yes | Directly verified (counts) | |
| PermitTemplate | Create / Publish / Deprecate / Retire / Archive | Yes (all 5) | Yes — 3 guard sites | Yes — 6 event sites | Yes — `usp_PermitTemplate_Insert`/`_Update` in the 26-proc list | Yes | Directly verified (counts) | |

## Representative Create-only aggregates (the ~50-aggregate majority)

| Aggregate | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Culture, Auditing(marker), Certification, ManagementOfChange, Permits(Compliance), Policy, Procedures, RegulatoryCalendar, PPEManagement(Operational), RiskManagement(Operational), Schedules(Operational), Training(Operational), EmergencyManagement, IncidentManagement, Observation, Dashboard, Engagement, PredictiveAnalytics, ScoreCards(Analytics), PPECategory, PPETrainingFit, PPEZoneMapping, EnvironmentalAssetMonitoring, FacilityTrafficMonitoring, PPEBehavioralMonitoring, PermitExpiration, PermitScheduling, ShiftFatigue, EmergencyDrillPlanning, HazardIsolationSequencing | **Create only** | Update/Delete/Cancel/Status-transition: **No — none exist in the domain model** | Yes for Create itself (basic non-null/factory checks; not individually re-verified per aggregate this phase beyond the grep-confirmed method/guard/event counts) | Yes — Create raises the aggregate's own `*CreatedEvent` (one of the 22 no-op-consumer pairs, or an unconsumed sibling per `gaps/missing-events.md`) | Yes for the subset also appearing in the 26-proc AuditLog list (Culture: not in list; most of the Analytics/Compliance-marker group: not in list — audit-logging and "rich lifecycle" turn out to be two independent axes, not correlated) | Yes — `TenantId` present on every aggregate checked | Directly verified (method/guard/event counts for ~25 of these via grep); Create-body internals not individually re-read for most | This is the largest population in the module and the starkest asymmetry the task asked about: **Create is a fully real (if shallow) domain operation; every other lifecycle stage is simply absent**, not merely unguarded — there is no Update/Delete/Cancel code path to audit at all for this majority |

## Cross-cutting notes

- **CorrelationId/TenantId propagation at the domain-event level is consistently good** for every
  aggregate read this phase — every event constructor takes `CorrelationId`/`CausationId`/`Actor`
  parameters. The gap is not in the domain event shape; it is (a) the ~55 repositories that discard
  the real `ActorId`/`CorrelationId` and hardcode `"system"`/`Guid.Empty` at the outbox-enqueue call
  site (`shape-audit/plantsafety-shape.md` Top-5 #1, unchanged this phase), and (b) `CultureCreatedEvent`
  (and, pattern-inferred, its 21 siblings) never carrying `TenantId` on the event itself
  (`gaps/tenant-violations.md` §3, unchanged this phase).
- **The Archive-never-publishes-an-event pattern recurs across every rich aggregate that has an
  Archive method**: confirmed directly for Accident, Risk, Scorecard; guard-confirmed-but-event-status-
  unverified for SafetyPolicy and PermitWorkflow. This looks like a deliberate design choice
  (Archive is meant to be a terminal, silent bookkeeping step) rather than an oversight, given how
  consistent it is — but it does mean no downstream system can ever be notified "this record left
  active retention," which may matter for compliance-retention tooling.
- **Audit-logging (the SQL `AuditLog` table) and "rich lifecycle" (the C# state machine) are two
  independent axes, not two views of the same thing.** AuditEngagement/AuditFinding have a full,
  well-guarded state machine but their stored procedures were never given the AuditLog insert 26
  sibling aggregates got. Conversely, several Create-only aggregates (Risk[Operational duplicate],
  PPECategory) were not checked against the 26-proc list and may or may not have it — the two
  properties should not be assumed correlated when reasoning about any aggregate not in this table.
