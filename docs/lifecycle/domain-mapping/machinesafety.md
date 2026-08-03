# Domain Mapping — MachineSafety

See `_shared-findings.md` §F1 (tenancy), §F2 (handler-bypass — no bypass found in a grep of this
cluster's handlers, same conclusion as EmployeeSafety/ContractorSafety).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| EmergencyStopDevices | BeginRepair/Create/RecordTestResult/Retire/ScheduleTest | 3 guard + 7 event sites (Phase 1 counts) | Pattern-inferred Y | Test result pass/fail fields plausible | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| EmergencyStopDevices | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely; `Retire` is the de facto terminal transition (Phase 1 finding) |
| LotoSystems | ApplyIsolation/Create/RecordAnnualAudit/Release/VerifyZeroEnergy | Conceptually parallel to `EmployeeSafety.LotoPrograms` — same isolation/verify/release shape, different aggregate (Phase 1 finding, not confirmed duplicate) | Pattern-inferred Y | Zero-energy verification plausible | 3rd LOTO-named aggregate cluster (with `EmployeeSafety.LotoPrograms`, `SafetyProcedures.LotoProcedures`) — no cross-aggregate check confirmed among any of the 3 | Not enforced (F1) | Directly verified (counts, Phase 1) | Genuinely 3 distinct bounded-context views (employee training / machine equipment / procedure document) per Phase 1's own read — plausibly correct design, but zero confirmed coordination between them is still a real gap if the same physical isolation needs to be reflected in more than one |
| LotoSystems | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| MachineGuardsBarriers | BeginCorrectiveAction/Create/RecordInspectionResult/Retire/ScheduleInspection/VerifyCorrectiveAction | Same inspect/correct/verify shape as `EmployeeSafety.MachineGuarding` (Phase 1 finding, not confirmed duplicate) | Pattern-inferred Y | Corrective-action verification plausible | Related but distinct name from `EmployeeSafety.MachineGuarding` — no shared identity confirmed | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| MachineGuardsBarriers | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PresenceSensingDevices | Create/RecordCalibrationResult/RecordTriggerEvent/Retire/ScheduleCalibration | 3 guard + 7 event sites | Pattern-inferred Y | Calibration tolerance fields plausible | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| PresenceSensingDevices | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| SafetyValvesRelays | BeginRepair/Create/RecordInspectionResult/Retire/ScheduleInspection | 3 guard + 7 event sites | Pattern-inferred Y | Inspection pass/fail fields plausible | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| SafetyValvesRelays | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| TwoHandControls | Create/RecordTestResult/Retire/ScheduleTest | 3 guard + 6 event sites | Pattern-inferred Y | Test result fields plausible | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| TwoHandControls | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** No individually-confirmed enforcement gaps this phase beyond the module-wide guard-count
pattern already captured in Phase 1; the recurring LOTO/MachineGuarding naming overlap with
EmployeeSafety remains a real coordination question (not a code duplicate) worth a dedicated Phase 3
cross-aggregate read.
