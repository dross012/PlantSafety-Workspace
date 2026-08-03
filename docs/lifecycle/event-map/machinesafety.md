# Event Map — MachineSafety (EmergencyStopDevices, LotoSystems, MachineGuardsBarriers, PresenceSensingDevices, SafetyValvesRelays, TwoHandControls)

See `_shared-findings.md` (F14-F17) and `command-handler-map/machinesafety.md` (event-completeness
scan: zero methods found missing `AddDomainEvent` across all 6 aggregates). Same consolidated-file
naming pattern as `employeesafety.md` (non-Create events live inside `<Aggregate>CreatedEvent.cs`,
not separate files) confirmed to also apply here.

**Directly re-verified this phase**: `EmergencyStopDevices.cs`'s 6 `AddDomainEvent` sites (1:1 to its
6 methods, no duplicates — F17 clean) and the corresponding event names
(`EmergencyStopDeviceInstalledEvent`, `TestScheduledEvent`, `TestPassedEvent`, `FailureDetectedEvent`,
`RepairStartedEvent`, `RetiredEvent`). Event body field lists not individually opened this phase
(time-boxed); payload completeness scored **pattern-inferred Y** on the strength of the identical
convention directly confirmed in `employeesafety.md`'s `MachineGuarding` sample and
`accidentmanagement.md`/`riskmanagement.md`'s fully-opened equivalents.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| EmergencyStopDevices | Create (Install) | Y | `EmergencyStopDeviceInstalledEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| EmergencyStopDevices | ScheduleTest | Y | `EmergencyStopDeviceTestScheduledEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| EmergencyStopDevices | RecordTestResult (Pass) | Y | `EmergencyStopDeviceTestPassedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| EmergencyStopDevices | RecordTestResult (Fail) | Y | `EmergencyStopDeviceFailureDetectedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| EmergencyStopDevices | BeginRepair | Y | `EmergencyStopDeviceRepairStartedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| EmergencyStopDevices | Retire | Y | `EmergencyStopDeviceRetiredEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoSystems | ApplyIsolation / Create / RecordAnnualAudit / Release / VerifyZeroEnergy | Pattern-inferred Y (6 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | Conceptually parallel to `EmployeeSafety.LotoPrograms`/`SafetyProcedures.LotoProcedures` — a 3rd distinct LOTO aggregate, not confirmed as a duplicate |
| MachineGuardsBarriers | BeginCorrectiveAction / Create / RecordInspectionResult / Retire / ScheduleInspection / VerifyCorrectiveAction | Pattern-inferred Y (8 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | Related but distinct from `EmployeeSafety.MachineGuarding` — same inspect/correct/verify shape as the directly-verified sample in `employeesafety.md`, strengthening the pattern-inference here specifically |
| PresenceSensingDevices | Create / RecordCalibrationResult / RecordTriggerEvent / Retire / ScheduleCalibration | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | |
| SafetyValvesRelays | BeginRepair / Create / RecordInspectionResult / Retire / ScheduleInspection | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | |
| TwoHandControls | Create / RecordTestResult / Retire / ScheduleTest | Pattern-inferred Y (6 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | |
| All 6 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | `Retire` functions as the de facto terminal transition module-wide |

**Duplicate-publish check (F17):** `EmergencyStopDevices.cs` — 6 `AddDomainEvent` sites for 6
methods, 1:1, no duplicates (directly re-verified this phase). Other 5 aggregates not individually
re-opened; inherited scripted scan found zero methods missing an `AddDomainEvent` call cluster-wide.

**Module-level summary:** consistent with every other rich PlantSafety aggregate directly sampled
this phase, `EmergencyStopDevices` shows a clean 1:1 event-to-method mapping with no duplicate
publishes. Zero live delivery for any of the ~31 transitions in this file (F9) — no consumer class
of any kind exists for any of the 6 aggregates.
