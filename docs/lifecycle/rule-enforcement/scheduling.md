# Rule Enforcement Validation — Scheduling

See `_shared-findings.md`. `PMScheduling`/`WorkOrderPriority` (rich), 3 create-only (batched).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PMScheduling — Create (Draft) | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `PMSchedulingDapperRepository` confirmed hardcoded site | High | Audit-logged (F8) |
| PMScheduling — SchedulePreventiveMaintenance | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PMScheduling — RecordCompletion | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PMScheduling — Escalate | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| PMScheduling — Cancel | **Pass (domain-level)** — fully guarded, correctly raises `PMSchedulingCancelledEvent` | Pass (inherited) | Pass | Pass (domain-level) | Fail (F1, pattern) | Fail (G2) — would apply if ever invoked | High | **Moot in practice**: confirmed unreachable from any HTTP route (`SchedulingV1Controller.cs`, no `cancel` route) — every Pass above describes a transition nothing can ever call in production |
| WorkOrderPriority — Create/Escalate/Resolve/StartWork/Close | Pass — all 5 present (inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `WorkOrderPriorityDapperRepository` confirmed hardcoded site | High | |
| WorkOrderPriority — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| EmergencyDrillPlanning — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `EmergencyDrillPlanningDapperRepository` confirmed hardcoded site | High (Actor); Medium (Constraints) | Missing entirely beyond Create; dormant `_Update` proc unused |
| HazardIsolationSequencing — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `HazardIsolationSequencingDapperRepository` confirmed hardcoded site | High (Actor); Medium (Constraints) | Same dormant-`_Update` shape |
| ShiftFatigue — Create only (batched) | Pass — factory guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Fail (G2) — `ShiftFatigueDapperRepository` confirmed hardcoded site | High (Actor); Medium (Constraints) | Same dormant-`_Update` shape |
