# Module Inventory — MachineSafety

**Entirely new to this phase.** 6 sub-aggregates, all rich, none individually read by the prior
discovery. Same caveats as `contractorsafety.md`/`employeesafety.md`: all 6 confirmed to have
`_Create`, `_GetAll`, `_GetById`, `_GetItems`, `_Update` procs by exact-prefix match; live-vs-legacy
status is pattern-inferred, not controller-confirmed, this phase.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| EmergencyStopDevices | BeginRepair / Create / RecordTestResult / Retire / ScheduleTest | Yes (all 5, 3 guard + 7 event sites) | Yes — `usp_EmergencyStopDevices_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | |
| LotoSystems | ApplyIsolation / Create / RecordAnnualAudit / Release / VerifyZeroEnergy | Yes (all 5, 2 guard + 6 event sites) | Yes — `usp_LotoSystems_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | Conceptually parallel to `EmployeeSafety.LotoPrograms` — same isolation/verify/release shape, different aggregate; not confirmed as a duplicate (LOTO = "lockout/tagout," a real cross-cutting safety procedure that legitimately touches both employee-training and machine-equipment bounded contexts) |
| MachineGuardsBarriers | BeginCorrectiveAction / Create / RecordInspectionResult / Retire / ScheduleInspection / VerifyCorrectiveAction | Yes (all 6, 4 guard + 8 event sites) | Yes — `usp_MachineGuardsBarriers_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | Same inspect/correct/verify shape as `EmployeeSafety.MachineGuarding` — related but distinct aggregate names, not confirmed as duplicates |
| PresenceSensingDevices | Create / RecordCalibrationResult / RecordTriggerEvent / Retire / ScheduleCalibration | Yes (all 5, 3 guard + 7 event sites) | Yes — `usp_PresenceSensingDevices_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | |
| SafetyValvesRelays | BeginRepair / Create / RecordInspectionResult / Retire / ScheduleInspection | Yes (all 5, 3 guard + 7 event sites) | Yes — `usp_SafetyValvesRelays_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | |
| TwoHandControls | Create / RecordTestResult / Retire / ScheduleTest | Yes (all 4, 3 guard + 6 event sites) | Yes — `usp_TwoHandControls_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | |
| All 6 aggregates above | Archive / Cancel / Delete | **No** — none found on any of the 6 | N/A | N/A | Directly verified | `Retire` functions as the de facto terminal transition across this entire feature area — consistent naming choice across all 6 aggregates, notably more consistent than the Archive/Close/Retire naming variance seen elsewhere in the solution |
