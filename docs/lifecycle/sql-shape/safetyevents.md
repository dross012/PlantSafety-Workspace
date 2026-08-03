# SQL Shape Audit — SafetyEvents

Phase 6 of 10. Covers the 3 Create-only aggregates under `PlantSafety.Domain.SafetyEvents`:
`EmergencyManagement`, `IncidentManagement`, `Observation`. Per `module-inventory/safetyevents.md`,
none has a genuine write proc (file-name census found only adjacent `*Items_Get*` read reports). This
phase's new contribution, as with `analytics.md`/`compliance.md`, is a direct DI-implementation check
of each aggregate's domain repository interface.

## Worst issues in this module

1. **[NEW THIS PHASE] All 3 domain write-repository interfaces in this module have zero
   implementations anywhere** — direct grep of `PlantSafety.Domain.SafetyEvents.
   {EmergencyManagement,IncidentManagement,Observation}.Interfaces` found, in every case, only the
   interface file and its paired `I<Aggregate>Factory.cs` referencing the namespace. Same F13-class
   finding as `analytics.md`/`compliance.md` — this module is the 3rd of the 3 Create-only clusters this
   phase checked, and all 3 come back with the same result: not one of their 14 combined aggregates
   (4 Analytics + 8 Compliance + 3 SafetyEvents, excluding the 1 already known `RegulatoryCalendar`) has
   a working write-repository registration. See `_shared-findings.md` for the consolidated count.
2. **`IncidentManagement` is the aggregate `AutomatedIncidentDetection.EscalateToIncident` (Observability
   module, F11) implies it escalates into** — this phase's DI check adds a sharper edge to F11's
   conclusion: not only does `EscalateToIncident` skip verifying the referenced incident exists, **there
   is no working repository on the `IncidentManagement` side to verify against even if the check were
   added**. A future fix to F11's gap would need to build the `IncidentManagement` write path from
   scratch, not just add a lookup call.
3. **`Observation` has an unconfirmed naming-adjacent sibling** (`usp_BehavioralObservationCompletion_
   GetItems`, inherited) that may or may not be the same concept — not disambiguated this phase either;
   irrelevant in practice since neither `Observation`'s own repository nor this sibling proc has a
   confirmed write path.
4. **Truncation/enum-drift questions are moot for all 3 aggregates**, same reasoning as
   `analytics.md`/`compliance.md` — no implementing class means no parameter-binding code exists to
   examine for a truncation pattern.
5. **All 3 `*CreatedEvent`s are written to Outbox regardless of the (impossible) persistence outcome**
   (F7) — same caveat as the other two Create-only-cluster files this phase produced.

## Method

Direct grep of each aggregate's fully-qualified `Interfaces` namespace against the whole solution
(excluding worktree scratch copies and test projects), following the same method used for
`analytics.md`/`compliance.md`. No stored-procedure body or repository parameter-binding code exists to
read for any of the 3 aggregates, so the field-set/nullability/enum columns below are uniformly N/A.

| Aggregate | Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| EmergencyManagement | Create | N/A — no implementing class (`IEmergencyManagementRepository`, 0 implementations) | N/A | N/A | N/A — fails at DI before any parameter binds | Directly verified (interface-implementation grep, this phase) | Distinct from `EmployeeSafety.EmergencyPreparedness`, which has a full, working `_Create`/`_Update` pair (inherited) — same real-world concern, opposite maturity |
| IncidentManagement | Create | N/A — no implementing class (`IIncidentManagementRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | Worst-issue #2 — ties directly to Observability's F11 open item |
| Observation | Create | N/A — no implementing class (`IObservationRepository`, 0 implementations) | N/A | N/A | N/A | Directly verified (this phase) | Naming-adjacency to `BehavioralObservationCompletion` unresolved (inherited) |
| All 3 above | Update/Close/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified (inherited) | |
