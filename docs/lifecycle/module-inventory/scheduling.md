# Module Inventory — Scheduling

5 aggregates: `PMScheduling`, `WorkOrderPriority` (rich), `EmergencyDrillPlanning`, `HazardIsolationSequencing`,
`ShiftFatigue` (create-only).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PMScheduling | Create (Draft) | Yes | Yes — `usp_PMScheduling_Insert` (AuditLog-backed) | Live | Directly verified (inherited) | |
| PMScheduling | SchedulePreventiveMaintenance | Yes | Yes — `usp_PMScheduling_Update` | Live | Directly verified (inherited) | |
| PMScheduling | RecordCompletion | Yes | Yes — `usp_PMScheduling_Update` | Live | Directly verified (inherited) | |
| PMScheduling | Escalate | Yes | Yes — `usp_PMScheduling_Update` | Live | Directly verified (inherited) | |
| PMScheduling | **Cancel** | Yes — fully guarded, correctly raises `PMSchedulingCancelledEvent` | Yes — would persist via `usp_PMScheduling_Update` if ever invoked | **Guarded in C# and SQL-persistable, but confirmed unreachable from any HTTP route** (`SchedulingV1Controller.cs`, full read, inherited — no `cancel` route exists) | Directly verified (inherited) | **The headline finding of the original discovery's Phase 6, reconfirmed structurally this phase** — the one Cancel method in all of PlantSafety.Domain, dead from the API's perspective |
| WorkOrderPriority | Create / Escalate / Resolve / StartWork / Close | Yes (all 5) | Yes — `usp_WorkOrderPriority_Insert`/`_Update` | Live | Directly verified (counts, inherited) | |
| WorkOrderPriority | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| EmergencyDrillPlanning | Create only | Yes | Yes — `usp_EmergencyDrillPlanning_Insert`/`_Update` (Update proc present, dormant — no domain method uses it) | Live (Create only) | Directly verified (this phase) | Update/Close/Cancel/Delete: missing entirely |
| HazardIsolationSequencing | Create only | Yes | Yes — `usp_HazardIsolationSequencing_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| ShiftFatigue | Create only | Yes | Yes — `usp_ShiftFatigue_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
