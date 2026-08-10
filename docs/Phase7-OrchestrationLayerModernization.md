# PHASE 7 — ORCHESTRATION LAYER MODERNIZATION (PLANTSAFETY-INTERNAL SCOPE)

**Program:** PlantSafety Modernization
**Scope:** WorkflowEngine (`Shared.WorkflowEngine`, `Enterprise.WorkflowOrchestration`, `Shared.Workflow`), Facility, Employee, Machine, Audit (partial), `Shared.EventBus`, `Shared.SqlGateway` — internal to the PlantSafety modernization program only.
**Explicitly excluded:** Routing, Scheduling, Diagnostics (external repos, no integration point, not referenced anywhere in this document). No new modules.
**Status:** Specification only. No code changes in this phase.

---

## 0. Foundational Findings (govern every section below)

**F0-01 — "WorkflowEngine" is three real, distinct pieces, not one.** Verified in the Phase 4 WorkflowEngine domain pass and re-confirmed here:
- `Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate` — a pure function, `WorkflowContext → WorkflowDecision`. No state, no persistence.
- `Enterprise.WorkflowOrchestration.Abstractions.Orchestrator.EnterpriseWorkflowOrchestrator` — `BuildExecutionPlan` (deterministic step ordering) and `Transition` (guarded lifecycle state changes). Also pure functions, no state.
- `Shared.Workflow.Abstractions.Execution.WorkflowExecutionEngine.Execute` — sequences a plan against caller-supplied per-step outcomes, applying rollback rules. Also a pure function.

All three are already deterministic, side-effect-free, and correctly injected with `IClock`/`IRandomProvider` at their controller boundary (`WorkflowOrchestrationController`, verified in the WorkflowEngine domain pass). This phase does not change any of the three — it defines how they connect to real event input/output, which none of them do today.

**F0-02 — No scheduler or background-execution mechanism exists anywhere in this program.** Confirmed in Phase 3 (Background Processing was flagged, not built — zero `IHostedService`/`BackgroundService` anywhere) and re-confirmed here. `RequiredAction.DueWithinHours` is a plain `int` on a record `WorkflowEngine.Evaluate` returns — nothing anywhere reads it to schedule a future action. Section 3 of this document ("Internal Scheduling") is therefore scoped to what `DueWithinHours` actually is: **deterministic due-by metadata production**, not timed execution. Specifying a working scheduler is out of scope; specifying what the existing metadata means and how a future scheduler would consume it deterministically is in scope.

**F0-03 — No dispatch mechanism exists for triggers either.** `SopTrigger`, `PermitTrigger`, and `EvacuationTrigger` are records `WorkflowEngine.Evaluate` returns to its HTTP caller. Nothing in this program sends a `SopTrigger` to an SOP-execution system or an `EvacuationTrigger` to a notification system — the "internal routing" Section 2 asks for does not exist as running code today, only as the caller's responsibility to act on an HTTP response. This document specifies what deterministic internal routing *would* look like if `WorkflowEngine.Evaluate` were invoked by the event-driven consumers Phase 6 specified (§4.1: Facility/Employee/Machine → WorkflowEngine), not a retrofit of something already running.

**F0-04 — Rule engines exist only for Facility.** `HazardInferenceRules`, `SeverityInferenceRules`, and `RegulatoryInferenceRules` (all `Shared.FacilityDomain.Abstractions`) are the only deterministic inference-rule engines in the audited scope. Employee and Machine have no equivalent — their aggregates validate invariants (required-string guards) but perform no multi-factor inference. This asymmetry is restated in §4 rather than papered over with invented Employee/Machine rule engines.

**F0-05 — Audit has no rules.** `Shared.AuditLogging` is read-only (`IAuditLogRepository.SearchAsync` only), confirmed in Phase 5 and Phase 6. "Audit rules" (§4's own enumeration request) has no candidates — restated, not invented.

---

## 1. WorkflowEngine Orchestration Specification

### 1.1 Orchestration Flows (Real Inventory)

| Flow | Entry point | Input | Output | Determinism basis |
|---|---|---|---|---|
| Per-event decision evaluation | `WorkflowEngine.Evaluate` | `WorkflowContext` (HazardType, SeverityLevel, AffectedScope, EmployeeExposureCount, MachineFailureCount, EventChainLength) | `WorkflowDecision` (RequiredActions, SopTriggers, PermitTriggers, EvacuationTriggers, NotificationRoutings) | Pure function; branches only on input fields, never ambient state |
| Execution-plan ordering | `EnterpriseWorkflowOrchestrator.BuildExecutionPlan` | `IReadOnlyList<EnterpriseWorkflowStep>` | Same list, stably sorted by `ExecutionPriorityByStepType` | Fixed priority table (§1.2); stable sort preserves same-type relative order |
| Lifecycle transition guard | `EnterpriseWorkflowOrchestrator.Transition` | `(workflowId, fromState, toState, nowUtc)` | `EnterpriseWorkflowTransition` or `InvalidOperationException` | Fixed `ValidTransitions` set; `nowUtc` supplied by caller, never read internally |
| Plan execution with rollback | `WorkflowExecutionEngine.Execute` | `(plan, stepOutcomes, workflowId, correlationId, module, stepIdFactory)` | `IReadOnlyList<WorkflowStepEnvelope<TStep>>`, each step marked Completed/Failed/RolledBack/Skipped | First-failure-triggers-rollback logic, applied in plan order |

### 1.2 Deterministic Rule Ordering (Within WorkflowEngine)

`EnterpriseWorkflowOrchestrator`'s existing fixed priority table is the one and only rule-ordering authority for step execution:

| Priority | Step type | Rationale (from the orchestrator's own header comment, verified) |
|---|---|---|
| 0 | EvacuationTrigger | Life-safety first |
| 1 | ShutdownTrigger | Stop the hazard source next |
| 2 | PermitTrigger | Procedural gate: legal/safe-to-proceed check |
| 3 | SopTrigger | Procedural gate: how-to-proceed |
| 4 | RequiredAction | Ordinary follow-up work |
| 5 | RestartTrigger | Always last — nothing restarts before every other step has run |

This phase adds no new priority levels and reorders nothing — the existing table already implements worst-case-first ordering correctly (verified against `WorkflowEngine.Evaluate`'s own branch order in §4).

### 1.3 Orchestration State Machine Rules

`EnterpriseWorkflowState`'s guarded transitions (from `EnterpriseWorkflowOrchestrator.ValidTransitions`, verified real):

```
Pending      → InProgress
Pending      → Cancelled
InProgress   → Completed
InProgress   → Failed
InProgress   → Cancelled
```

No other transition is valid; `Transition` throws `InvalidOperationException` for any pair not in this set (already implemented, unchanged by this phase). `Completed`, `Failed`, and `Cancelled` are terminal — no transition originates from any of them in the existing set.

### 1.4 Orchestration Failure Handling

| Failure | Handling |
|---|---|
| A step in `WorkflowExecutionEngine.Execute` fails (`stepOutcomes[i] == false`) | Every prior `Completed` step is marked `RolledBack` (existing `WorkflowRollbackPolicy` behavior); every step after the failure is marked `Skipped`. No partial success is ever reported as success. |
| An invalid state transition is requested | `Transition` throws `InvalidOperationException` with the specific `(fromState, toState)` pair named; the caller (per Phase 4's `WorkflowOrchestrationController`) converts this into a 400 response, never a 500 — this is existing, correct behavior. |
| `WorkflowEngine.Evaluate` receives a `WorkflowContext` with a `HazardType` not matched by any `switch` arm | No SOP/permit trigger is added for that hazard (the `switch` in `WorkflowEngine.Evaluate` has no `default` arm producing a fallback trigger) — this is existing behavior, not a bug: an unrecognized hazard type correctly produces no procedural-gate trigger rather than guessing one. |

### 1.5 Orchestration Idempotency Rules

`WorkflowEngine.Evaluate`, `BuildExecutionPlan`, `Transition`, and `Execute` are all pure functions — calling any of them twice with identical input produces identical output, satisfying idempotency trivially. The idempotency concern that matters is at the *consumer* layer (Phase 6 §5.2: `SqlEventSubscriber`'s `ProcessedEvents` table, keyed by `(ConsumerGroup, EventId)`) — once WorkflowEngine's evaluation is triggered by a consumed event (§5), the event-delivery idempotency guarantee, not a new WorkflowEngine-internal one, is what prevents a duplicate evaluation from producing a duplicate `WorkflowEngine.DecisionEvaluated` event.

### 1.6 Orchestration Event Emission Rules (Using Shared.EventBus)

Per Phase 6 §1.4, `WorkflowEngine.DecisionEvaluated` (`EventEnvelopeV2`, `SchemaVersion = 1`) is emitted once per `WorkflowEngine.Evaluate` invocation that was triggered by a consumed event (not for direct HTTP-caller invocations via `WorkflowEngineController.Evaluate` — that path remains a synchronous request/response with no event emission, since it has no triggering event to set `CausationId` from). Payload: the `WorkflowContext` evaluated, the resulting `WorkflowDecision`, `CausationId` = the triggering event's `EventId`, `CorrelationId` = the triggering event's `CorrelationId` (propagated, not regenerated).

---

## 2. Internal Routing Orchestration Specification (PlantSafety Only)

### 2.1 What "Internal Routing" Means Here

Per F0-03, no dispatch mechanism exists today. "Internal routing" is the deterministic mapping from a `WorkflowDecision`'s trigger collections to the specific downstream action each trigger implies — a mapping that exists today only as a `WorkflowDecision`'s shape, never executed. This section specifies that mapping as a contract for a future dispatcher (Phase 8+), not a description of code that runs today.

### 2.2 Routing Table (Deterministic, by Trigger Type)

| Trigger type | Routes to (deterministic target) | Ordering constraint |
|---|---|---|
| `EvacuationTrigger` | Notification dispatch to every `NotificationRouting` with `Channel = Page` or `Channel = Sms`, at priority 0 (§1.2) | Must be dispatched before any other trigger type |
| `SopTrigger` | SOP-code lookup (no execution system exists to route to yet — the trigger's `SopCode` is the deterministic key a future SOP-execution system would key off) | After `PermitTrigger`, before `RequiredAction` |
| `PermitTrigger` | Permit-code lookup (same caveat as `SopTrigger`) | After `ShutdownTrigger`, before `SopTrigger` |
| `RequiredAction` | Action-code lookup, carrying `DueWithinHours` as scheduling metadata (§3) | After all trigger types, before `RestartTrigger` |
| `NotificationRouting` | Not itself a step type in `EnterpriseWorkflowStepType` — routed independently, immediately upon `WorkflowDecision` production, keyed by `(RecipientRole, Channel)` | Independent of step-priority ordering; notifications are advisory, not gating |

### 2.3 Routing Metadata Requirements

Every routed trigger must carry, at minimum, its own natural key (`SopCode`/`PermitCode`/`ActionCode`), the `CorrelationId` propagated from the triggering domain event (Phase 6 §1.2), and the `WorkflowContext.AffectedScope`/`EvacuationScope` value needed to determine blast radius — all three already exist on the relevant record types; no new fields are required.

### 2.4 Routing Ordering Guarantees

Identical to §1.2's step-priority table — internal routing ordering is not a separate concept from orchestration step ordering, it is the same table applied to dispatch instead of to plan construction. No second ordering scheme is introduced.

### 2.5 Routing Failure Handling

A routing target that does not yet exist (no SOP-execution system, no permit system — per F0-03) is not a failure to handle in this phase; it is explicitly out of scope until such a system exists. What this phase specifies is the deterministic *contract* such a system would need to honor (§2.2, §2.3) — not a retry/failure policy for a system that isn't built.

### 2.6 Routing Determinism Guarantees

Given the same `WorkflowDecision`, the routing table (§2.2) produces the same ordered dispatch sequence every time — a direct consequence of `WorkflowDecision`'s own construction being deterministic (§1.1) and the routing table itself being a fixed, static mapping with no runtime-conditional branching beyond trigger type.

---

## 3. Internal Scheduling Orchestration Specification (PlantSafety Only)

### 3.1 What "Internal Scheduling" Means Here

Per F0-02, no scheduler exists. This section specifies the deterministic *meaning* of `RequiredAction.DueWithinHours` and the ordering rule a future scheduler must follow — not a scheduler implementation.

### 3.2 Scheduling Constraints

1. `DueWithinHours` is relative to the `OccurredAtUtc` of the domain event that triggered the `WorkflowEngine.Evaluate` call producing it (Phase 6 §1.4's `CausationId` chain resolves this unambiguously) — never relative to "now" at the time some future scheduler happens to process the action.
2. A `RequiredAction` with `DueWithinHours = 0` (used today only for `"EMERGENCY-RESPONSE"` when `SeverityLevel >= Severe`, verified in `WorkflowEngine.Evaluate`) means "immediately," not "no deadline" — a future scheduler must treat 0 as the highest-urgency value, not a falsy/unset sentinel.

### 3.3 Deterministic Scheduling Ordering

`RequiredAction`s are ordered, when multiple are produced by one `WorkflowEngine.Evaluate` call, by ascending `DueWithinHours` (soonest-due first) — a pure sort over already-produced, deterministic values. This is the only ordering rule specified; it does not depend on or interact with §1.2's step-priority table, since `RequiredAction` is already the lowest-priority (4) step type there and this sort only orders *within* that one priority level.

### 3.4 Scheduling Failure Handling

Not specified — there is no scheduler to fail. This is stated explicitly rather than left silently unaddressed: a future scheduler's failure handling (missed deadline, retry policy) is Phase 8+ scope, contingent on a scheduler being built at all, which is itself a product decision not made yet.

---

## 4. Deterministic Rule Ordering Specification

### 4.1 Rule Evaluation Order Per Module

| Module | Rule engine | Evaluation order | Conflict resolution |
|---|---|---|---|
| Facility | `HazardInferenceRules.InferFromText` | First keyword match wins, in the fixed `if`-chain order already coded (Fire → ChemicalSpill/EnvironmentalRelease → ... ) | N/A — mutually exclusive keyword matches by construction |
| Facility | `SeverityInferenceRules.Infer` | All four factors (hazard type, injury count, failure count, chain length) evaluated independently, in parallel (no ordering dependency between them) | **Worst-case wins**: `Max` across all four resulting `SeverityLevel`s (verified real, existing code) — severity never under-reports |
| Facility | `RegulatoryInferenceRules.Infer` | Takes `(HazardType, SeverityLevel)` — depends on `SeverityInferenceRules.Infer`'s output, so must run after it | N/A — returns a set of applicable jurisdictions, not a single winner; no conflict to resolve |
| Employee | *(none — F0-04)* | N/A | N/A |
| Machine | *(none — F0-04)* | N/A | N/A |
| WorkflowEngine | `WorkflowEngine.Evaluate`'s internal branches (severity check → hazard-type switch → machine-failure check → employee-exposure check → event-chain-length check → notification routing) | Fixed order, already coded, each branch independently appends to its own output list (RequiredActions/SopTriggers/PermitTriggers/EvacuationTriggers) — no branch's output depends on another branch's result within one `Evaluate` call | **Additive, not competing**: multiple branches can each contribute triggers to the same `WorkflowDecision`; there is no "winner" concept here, only accumulation — distinct from `SeverityInferenceRules`' single-winner `Max` pattern |
| WorkflowEngine | `EnterpriseWorkflowOrchestrator.BuildExecutionPlan` | Fixed priority table, §1.2 | Stable sort — ties (same step type) preserve original relative order, never reordered arbitrarily |
| Audit | *(none — F0-05)* | N/A | N/A |

### 4.2 Rule Grouping and Prioritization

Two distinct patterns exist and must not be conflated:
1. **Winner-takes-all** (`SeverityInferenceRules.Infer`'s `Max`): used when multiple factors independently estimate the *same* single-valued output (severity). The worst estimate wins.
2. **Additive accumulation** (`WorkflowEngine.Evaluate`'s branches, `EnterpriseWorkflowOrchestrator.BuildExecutionPlan`'s step list): used when multiple factors each contribute *independent* items to a collection. Nothing "wins"; everything that applies is included, then ordered (§1.2).

Any new rule this program adds in a future phase must be classified as one of these two patterns explicitly — inventing a third pattern (e.g., weighted scoring) is out of scope for this phase and not evidenced as needed by any existing rule.

### 4.3 Rule Determinism Guarantees

Every rule enumerated in §4.1 is a pure function of its explicit inputs — none reads `DateTime.Now`/`DateTime.UtcNow`, ambient configuration, or randomness. This has been independently verified for `SeverityInferenceRules`, `HazardInferenceRules`, `RegulatoryInferenceRules` (Facility) and `WorkflowEngine.Evaluate`, `EnterpriseWorkflowOrchestrator.BuildExecutionPlan` (WorkflowEngine) by direct code inspection in this and prior phases. No exceptions found.

---

## 5. Cross-Module Orchestration Contracts (Inside PlantSafety Only)

### 5.1 Contracts

| Contract | Trigger event (Phase 6 §1.1) | Required `WorkflowContext` construction | Emitted event |
|---|---|---|---|
| Facility → WorkflowEngine | `FacilityLifecycleChangedEvent` (NewStatus = ShutDown), `FacilityHierarchyUpdatedEvent` | `HazardType`/`SeverityLevel` inferred via `HazardInferenceRules`/`SeverityInferenceRules` from the event's available fields (facility-level events carry no injury/failure/chain-length data directly — those default to 0 unless a richer context-assembly rule is added in a later phase); `AffectedScope` derived from the facility's hierarchy depth at the point of the event | `WorkflowEngine.DecisionEvaluated` |
| Employee → WorkflowEngine | `EmployeeUpdatedEvent` (Kind = InjuryProfile) | `EmployeeExposureCount`/injury-derived `SeverityLevel` factor from `SeverityInferenceRules.InferFromInjuryCount`; `HazardType` defaults to `HazardType.Injury` (already a real enum member, confirmed in `SeverityInferenceRules.InferFromHazardType`'s own switch) | `WorkflowEngine.DecisionEvaluated` |
| Machine → WorkflowEngine | `MachineFailureRecordedEvent` | `MachineFailureCount = 1` (per-event, not cumulative — cumulative failure count is a Facility/Machine-state concern, out of this contract's scope), `HazardType` = the event's own `ResultingHazardType` field (already present, carried through unchanged, no re-inference needed) | `WorkflowEngine.DecisionEvaluated` |
| WorkflowEngine → Audit | *(not a publish/subscribe contract — restated per F0-05 and Phase 6 §4.4)* | N/A | `WorkflowEngine.DecisionEvaluated`'s own presence in `dbo.EventQueue`'s processed-event history **is** the audit trail; no separate contract is defined or needed |

### 5.2 Required Metadata Fields

Identical to Phase 6 §1.2's six required `EventEnvelopeV2` fields (`EventId`, `EventType`, `SchemaVersion`, `CorrelationId`, `CausationId`, `SourceModule`, `OccurredAtUtc`) — this phase adds no new required fields beyond what Phase 6 already specified. `WorkflowEngine.DecisionEvaluated`'s `SourceModule` is `"WorkflowEngine"`.

### 5.3 Required Invariants

1. A `WorkflowEngine.DecisionEvaluated` event's `CausationId` must always resolve to a real, previously-published event's `EventId` — it is never null and never a freshly-generated GUID with no corresponding cause (this would break the causation chain Phase 6 §4.2 specifies).
2. `WorkflowContext` fields not derivable from the triggering event (per §5.1's per-contract notes) must default to their zero-value (`0` for counts, the least-severe `SeverityLevel`) rather than omit the field or throw — `WorkflowContext` is a plain record with no optional fields, so every field must always have a value; under-reporting via zero-defaults is the same "never guess, never overstate" discipline `HazardInferenceRules` already applies (F-verified, §0).

### 5.4 Required Event Emission Rules

Restates §1.6: exactly one `WorkflowEngine.DecisionEvaluated` event per triggering domain event consumed, never zero (even a `WorkflowDecision` with empty trigger/action lists is still a real, recordable outcome) and never more than one (idempotent consumption, Phase 6 §5.2, guarantees this even under at-least-once delivery).

### 5.5 Replay/Rebuild Alignment with Phase 6

`WorkflowEngine.DecisionEvaluated` is stateless from a replay perspective (Phase 6 §2.1: "replaying it means re-running `WorkflowEngine.Evaluate` against the same recorded `WorkflowContext`, which is trivially deterministic"). This phase adds nothing to Phase 6's replay/rebuild specification — it confirms that the four contracts in §5.1 produce events fully compatible with Phase 6's existing replay ordering (§2.2: per-aggregate-instance, `OccurredAtUtc` order), since `WorkflowEngine.DecisionEvaluated` events are keyed by their `CausationId`'s originating aggregate instance (the Facility/Employee/Machine ID embedded in the triggering event), not by any WorkflowEngine-owned aggregate ID — WorkflowEngine has no aggregate of its own to replay (F0-01).

---

## 6. Phase 7 Output: Unified Orchestration Specification

### 6.1 Consolidated Ruleset

| Area | Rule |
|---|---|
| **Orchestration flows** | Three real, pure-function pieces (`WorkflowEngine.Evaluate`, `EnterpriseWorkflowOrchestrator`, `WorkflowExecutionEngine`) — none introduced or removed by this phase. |
| **Step ordering** | Fixed priority: Evacuation(0) → Shutdown(1) → Permit(2) → SOP(3) → RequiredAction(4) → Restart(5). Single source of truth (`EnterpriseWorkflowOrchestrator.ExecutionPriorityByStepType`), reused for both plan-building and internal routing (§2.4) — not duplicated. |
| **State machine** | `Pending → {InProgress, Cancelled}`, `InProgress → {Completed, Failed, Cancelled}`. No other transitions valid. Terminal states: Completed, Failed, Cancelled. |
| **Failure handling** | First-failure-triggers-rollback (prior steps → RolledBack, later steps → Skipped); invalid transitions throw and surface as 400, never 500; unmatched hazard types produce no fallback trigger rather than a guessed one. |
| **Idempotency** | All orchestration functions are pure (trivially idempotent); event-delivery idempotency is Phase 6's `ProcessedEvents` table, not a new mechanism. |
| **Event emission** | One `WorkflowEngine.DecisionEvaluated` (`SchemaVersion=1`) per consumed triggering event, `CausationId` always resolving to that event's `EventId`. |
| **Internal routing** | Deterministic trigger-type → target mapping (§2.2), same priority table as step ordering, specified as a contract for a not-yet-built dispatcher — not a description of running code. |
| **Internal scheduling** | `DueWithinHours` is relative to the triggering event's `OccurredAtUtc`; 0 means immediate, never unset; ascending-`DueWithinHours` ordering within the RequiredAction priority level. No scheduler exists or is specified to be built in this phase. |
| **Rule ordering** | Two patterns only — winner-takes-all (`Max`, used by severity inference) and additive accumulation (used by trigger/step production). No third pattern introduced. Employee/Machine/Audit have no rule engines; restated, not invented. |
| **Cross-module contracts** | Four real contracts: Facility/Employee/Machine → WorkflowEngine (event-triggered evaluation, `WorkflowEngine.DecisionEvaluated` emitted), WorkflowEngine → Audit (not a publish relationship — the event-queue history is itself the audit trail). |
| **Replay/rebuild alignment** | `WorkflowEngine.DecisionEvaluated` replays via pure re-evaluation, keyed to its `CausationId`'s originating aggregate — fully consistent with Phase 6's existing per-aggregate replay ordering; no new replay machinery needed. |

### 6.2 Explicitly Not Specified (Out of Scope, Not Forgotten)

- A working scheduler or dispatcher for `RequiredAction`/`SopTrigger`/`PermitTrigger`/`EvacuationTrigger` — the contracts these would need to honor are specified (§2, §3); building them is a product decision and Phase 8+ scope.
- Employee/Machine rule engines equivalent to Facility's `HazardInferenceRules`/`SeverityInferenceRules`/`RegulatoryInferenceRules` — none exist; none are invented here.
- Any interaction with Routing, Scheduling, or Diagnostics — not referenced anywhere in this document, per the explicit constraint.

### 6.3 Readiness for Phase 8

No open product/engineering decisions remain from this phase — every section is grounded in code already verified to exist and already deterministic. Phase 8 implementation (wiring the four §5.1 contracts as real `SqlEventSubscriber` consumer groups, per Phase 6 §4.3) can begin once Phase 6's own single open item (already resolved: `dbo.EventQueue` lives in `PlantSafety.CoreDB`, confirmed) is implemented in code — this phase's specification does not add any further precondition.
