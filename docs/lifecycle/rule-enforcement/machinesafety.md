# Rule Enforcement Validation — MachineSafety

See `_shared-findings.md`. 6 rich sub-aggregates; `Retire` is the consistent de facto terminal transition
across all 6.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| EmergencyStopDevices — BeginRepair/Create/RecordTestResult/Retire/ScheduleTest | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `EmergencyStopDevicesDapperRepository` confirmed hardcoded site | Medium | |
| LotoSystems — ApplyIsolation/Create/RecordAnnualAudit/Release/VerifyZeroEnergy | Pass — 2 guard sites | Unconfirmed | Pass — parallel to `EmployeeSafety.LotoPrograms` but no shared FK/identity found; distinct bounded contexts, not confirmed duplicates | Pass | Fail (F1, pattern) | Fail (G2) — `LotoSystemsDapperRepository` confirmed hardcoded site | Medium | |
| MachineGuardsBarriers — BeginCorrectiveAction/Create/RecordInspectionResult/Retire/ScheduleInspection/VerifyCorrectiveAction | Pass — 4 guard sites | Unconfirmed | Pass — related to but not confirmed duplicate of `EmployeeSafety.MachineGuarding` | Pass | Fail (F1, pattern) | Fail (G2) — `MachineGuardsBarriersDapperRepository` confirmed hardcoded site | Medium | |
| PresenceSensingDevices — Create/RecordCalibrationResult/RecordTriggerEvent/Retire/ScheduleCalibration | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `PresenceSensingDevicesDapperRepository` confirmed hardcoded site | Medium | |
| SafetyValvesRelays — BeginRepair/Create/RecordInspectionResult/Retire/ScheduleInspection | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `SafetyValvesRelaysDapperRepository` confirmed hardcoded site | Medium | |
| TwoHandControls — Create/RecordTestResult/Retire/ScheduleTest | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `TwoHandControlsDapperRepository` confirmed hardcoded site | Medium | |
| All 6 above — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | Missing entirely on all 6; `Retire` covers the terminal case informally |
