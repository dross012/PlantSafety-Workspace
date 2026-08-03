# Domain Mapping — Scheduling

See `_shared-findings.md` §F1 (tenancy).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PMScheduling | Create (Draft) | Starts `Draft` | Y (trivial) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| PMScheduling | SchedulePreventiveMaintenance | Must be `Draft` (pattern-inferred) | Pattern-inferred Y | Schedule date required | Plausibly should check equipment status in MachineSafety before scheduling — no confirmed link | Not enforced (F1) | Directly verified (inherited) | |
| PMScheduling | RecordCompletion | Must be `Scheduled` (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| PMScheduling | Escalate | — | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| PMScheduling | Cancel | Terminal-state check + non-empty reason (Phase 1, inherited full read); fully guarded, correctly raises `PMSchedulingCancelledEvent` | **Y** — fully guarded, but **confirmed unreachable from any HTTP route** (`SchedulingV1Controller.cs`, full read, inherited) | Reason required | None | Not enforced (F1) | Directly verified (inherited) | The one Cancel method in all of PlantSafety.Domain — guarded correctly, dead at the API layer. Same "guarded but unreachable" category as F4 (Operational.Alerts.Alert) and Intelligence |
| WorkOrderPriority | Create/Escalate/Resolve/StartWork/Close | 5 methods (Phase 1 counts) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (counts, inherited) | |
| WorkOrderPriority | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| EmergencyDrillPlanning | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Dormant Update SQL proc exists, unused |
| HazardIsolationSequencing | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Same dormant-Update shape |
| ShiftFatigue | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Same dormant-Update shape |

**Summary:** `PMScheduling.Cancel` remains the clearest confirmed instance of "correctly guarded,
unreachable in production" outside Intelligence and the newly-confirmed `Operational.Alerts.Alert`
cluster (F4) — a 3rd instance of this exact failure category, reinforcing that it is a recurring
class of defect (guard quality is not the gap; route wiring is), not a one-off.
