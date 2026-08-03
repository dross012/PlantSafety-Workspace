# SQL Shape Audit — Observability

Phase 6 of 10. Covers `AutomatedIncidentDetection`, `MachineProcessMonitoring` (rich);
`EnvironmentalAssetMonitoring`, `FacilityTrafficMonitoring`, `PPEBehavioralMonitoring` (create-only).

## Worst issues in this module

1. **`DismissDetectionCommand.Reason` — confirmed truncation risk, an 8th solution-wide instance.**
   `DismissDetectionCommandValidator.cs:22`: `MaximumLength(1000)` on the command's `Reason` field, which
   maps to the domain/column name `DismissalReason`. `AutomatedIncidentDetectionDapperRepository.cs:89,125`:
   `AddString("DismissalReason", aggregate.DismissalReason, 400)`. A 600-char gap, tied for the 2nd-largest
   found this phase. Directly verified.
2. **`RaiseMachineAnomalyFlagCommand.AnomalyDescription` — confirmed truncation risk, a 9th instance,
   same 600-char gap.** `RaiseMachineAnomalyFlagCommandValidator.cs:22`: `MaximumLength(1000)`.
   `MachineProcessMonitoringDapperRepository.cs:89,125`: `AddString("AnomalyDescription",
   aggregate.AnomalyDescription, 400)`. Directly verified.
3. **`MachineProcessMonitoring` has no `Close`/`Resolve`** (inherited) — an anomaly flag, once raised via
   the very field found truncatable in #2, has no confirmed way to be marked resolved in the domain model
   at all — a lifecycle gap compounding the shape gap.
4. **F11 re-confirms `AutomatedIncidentDetection.EscalateToIncident` does not verify the referenced
   Incident exists** (inherited) — same-process reference stored on faith, not a genuine cross-module
   call, despite the method name.
5. **No CHECK constraint found on any of this module's 5 tables.**

## Method

Direct read this phase: `DismissDetectionCommandValidator.cs`, `RaiseMachineAnomalyFlagCommandValidator.cs`,
`AutomatedIncidentDetectionDapperRepository.cs`, `MachineProcessMonitoringDapperRepository.cs` (`AddString`
call sites for both aggregates' full field lists). The 3 create-only aggregates' field shapes are
pattern-inferred from the same uniform `_Insert`/`_Update`(dormant) convention Phase 1 confirmed.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| AutomatedIncidentDetection.Create | Yes — `DetectionType`, `LocationDescription` present, both sized 400 with no confirmed validator smaller | Not individually re-verified | Structurally possible, no CHECK found | No — not confirmed smaller than 400 | Directly verified (param sizes) | |
| AutomatedIncidentDetection.Acknowledge | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| AutomatedIncidentDetection.Dismiss | Yes — `DismissalReason` present | Not verified | Same | **Yes — confirmed** (`Reason` 1000 validator vs `DismissalReason` 400 param) | Directly verified | Worst-issue #1 |
| AutomatedIncidentDetection.EscalateToIncident | Field set not re-opened; `IncidentReferenceId` is a raw caller-supplied `Guid`, no existence check (F11) | Not verified | Same | Not individually re-verified | Directly verified (F11, inherited) | Worst-issue #4 |
| AutomatedIncidentDetection.NotifySupervisor | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| AutomatedIncidentDetection.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| MachineProcessMonitoring.Create | Yes — `SensorType` present (400, not confirmed smaller than validated) | Not individually re-verified | Same | No — not confirmed smaller than 400 | Directly verified (param size) | |
| MachineProcessMonitoring.Acknowledge | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| MachineProcessMonitoring.RaiseAnomalyFlag | Yes — `AnomalyDescription` present | Not verified | Same | **Yes — confirmed** (1000 validator vs 400 param) | Directly verified | Worst-issue #2 |
| MachineProcessMonitoring.RecordSensorReading | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| MachineProcessMonitoring.Close/Resolve | N/A — transition does not exist (real lifecycle gap, inherited) | N/A | N/A | N/A | Directly verified | Worst-issue #3 |
| EnvironmentalAssetMonitoring.Create only | Yes — `SensorId`, `MeasurementType` present (400 each) | Not individually re-verified | N/A | Not individually re-verified | Directly verified (param sizes) | Dormant Update proc (inherited) |
| FacilityTrafficMonitoring.Create only | Yes — `ZoneCode`, `EventType` present (400 each) | Not individually re-verified | N/A | Not individually re-verified | Directly verified (param sizes) | Dormant Update proc (inherited) |
| PPEBehavioralMonitoring.Create only | Yes — `SensorId`, `ZoneCode` present (400 each) | Not individually re-verified | N/A | Not individually re-verified | Directly verified (param sizes) | Dormant Update proc (inherited) |
