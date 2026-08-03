# Event Map — EmployeeSafety (EmergencyPreparedness, EnvironmentalHealthMonitoring, ErgonomicsAndMaterialHandling, HazardCommunicationTraining, LotoPrograms, MachineGuarding, Ppe)

See `_shared-findings.md` (F14-F17) and `command-handler-map/employeesafety.md` (event-completeness
scan: zero methods found missing an `AddDomainEvent` call across all 7 aggregates). **New structural
finding this phase**: non-Create events for this cluster are **not** in separate per-transition
files — they are defined as multiple `record` types inside the single file named
`<Aggregate>CreatedEvent.cs` (confirmed by directly reading `MachineGuarding/Events/
MachineGuardingCreatedEvent.cs`, which contains 8 distinct event records, not 1). This filename
convention is misleading by inspection alone and explains why an earlier file-existence-only pass
could undercount this cluster's true event vocabulary.

**Fully directly verified this phase**: `MachineGuarding`'s all 8 events (`Installed`,
`InspectionScheduled`, `InspectionPassed`, `InspectionFailed`, `BypassDetected`,
`CorrectiveActionScheduled`, `CorrectiveActionVerified`, `Retired`) read in full and cross-checked
against `MachineGuarding.cs`'s 8 `AddDomainEvent` sites (1:1, no duplicates — F17 confirmed clean).
The other 6 aggregates' events were not individually opened this phase (pattern-inferred, on the
strength of this direct sample plus command-handler-map's inherited method-count scan).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| MachineGuarding | Install (Create) | Y | `MachineGuardingInstalledEvent` | Y — `GuardType (MachineGuardType), EquipmentTag (string)` beyond identity/actor | **N** | Directly verified | |
| MachineGuarding | ScheduleInspection | Y | `MachineGuardingInspectionScheduledEvent` | Y — `DueAtUtc` | **N** | Directly verified | |
| MachineGuarding | RecordInspectionResult (Pass) | Y | `MachineGuardingInspectionPassedEvent` | Y — `GuardConditionScore (decimal)` (**F15 caveat**: no prior score) | **N** | Directly verified | |
| MachineGuarding | RecordInspectionResult (Fail) | Y | `MachineGuardingInspectionFailedEvent` | Y — `DeficiencyCount (int), Notes (string)` | **N** | Directly verified | |
| MachineGuarding | FlagBypass | Y | `MachineGuardingBypassDetectedEvent` | Y — `FindingNotes (string), DetectedByUserId (UserId)` — complete, names the observer | **N** | Directly verified | |
| MachineGuarding | ScheduleCorrectiveAction | Y | `MachineGuardingCorrectiveActionScheduledEvent` | Y — `DueAtUtc` | **N** | Directly verified | |
| MachineGuarding | VerifyCorrectiveAction | Y | `MachineGuardingCorrectiveActionVerifiedEvent` | Y — `VerifiedByUserId (UserId)` | **N** | Directly verified | |
| MachineGuarding | Retire | Y | `MachineGuardingRetiredEvent` | Y — `Reason (string)` | **N** | Directly verified | Richest, most consistently-typed event set of any "new this phase" cluster aggregate reviewed |
| EmergencyPreparedness | Create / ConductDrill / RecordReadiness / ScheduleDrill | Pattern-inferred Y (5 event sites confirmed present by count, inherited) | Named per convention, likely consolidated in `EmergencyPreparednessCreatedEvent.cs` per the newly-confirmed file-naming pattern above | Pattern-inferred Y per F14/this phase's MachineGuarding sample | **N** | Pattern-inferred | |
| EnvironmentalHealthMonitoring | Create / BeginCorrectiveAction / RecordMeasurement / ResolveExceedance | Pattern-inferred Y (5 event sites, inherited) | Same consolidated-file pattern presumed | Pattern-inferred Y | **N** | Pattern-inferred | Guard density is the thinnest of this cluster (inherited) — doesn't affect event-raising completeness |
| ErgonomicsAndMaterialHandling | Create / Close / ImplementControls / RecordAssessment / ScheduleFollowUp | Pattern-inferred Y (6 event sites, inherited) | Same consolidated-file pattern presumed | Pattern-inferred Y | **N** | Pattern-inferred | Has a genuine `Close` — a formal terminal transition, unlike most of this cluster |
| HazardCommunicationTraining | Create / Close / DeliverTraining / ScheduleNextCycle / VerifyComprehension | Pattern-inferred Y (7 event sites, inherited) | Same consolidated-file pattern presumed | Pattern-inferred Y | **N** | Pattern-inferred | |
| LotoPrograms | ApplyIsolation / Document / RecordAnnualAudit / Release / VerifyZeroEnergy (+ implied Create) | Pattern-inferred Y (6 event sites, inherited) | Same consolidated-file pattern presumed | Pattern-inferred Y | **N** | Pattern-inferred | No plain `Create` method name found (inherited) |
| Ppe (EmployeeSafety) | Create / FlagForReplacement / RecordFitTest / RecordInspection / Retire | Pattern-inferred Y (5 event sites, inherited) | Same consolidated-file pattern presumed | Pattern-inferred Y | **N** | Pattern-inferred | Possible undocumented overlap with `PPE.PPEAssignment` (inherited, unconfirmed) |
| All 7 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** `MachineGuarding.cs` — 8 `AddDomainEvent` sites for 8 methods,
1:1, no duplicates (directly re-verified this phase). The other 6 aggregates not individually
re-opened, but command-handler-map's inherited scripted scan (corrected for the `ClearDomainEvents`
false-positive) found zero methods missing an `AddDomainEvent` call across the whole cluster, which
is consistent with — though doesn't independently confirm — an absence of double-publishes.

**Module-level summary:** `MachineGuarding`'s directly-verified event set is one of the most
complete and well-typed in the entire solution (every "who" field is a typed `UserId`, no untyped
`Guid` cross-references found, unlike Auditing/Observability/Dashboards). Zero live delivery for any
of the ~37 transitions in this file (F9) despite full event-raising correctness.
