# Command-Handler Map — MachineSafety

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 6 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-B]), F10, F2 (handler-bypass confirmed NOT to
recur in this cluster, `domain-mapping/_shared-findings.md`). **Event-completeness directly
re-verified this phase** for all 6 aggregates — zero methods found missing an `AddDomainEvent` call
(same scripted-scan method as `employeesafety.md`, correcting for the `ClearDomainEvents`
false-positive). Command/handler class names pattern-inferred; not individually opened.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EmergencyStopDevices | BeginRepair / Create / RecordTestResult / Retire / ScheduleTest | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` (F1/F2 pattern) | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| LotoSystems | ApplyIsolation / Create / RecordAnnualAudit / Release / VerifyZeroEnergy | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Conceptually parallel to `EmployeeSafety.LotoPrograms`/`SafetyProcedures.LotoProcedures` — a 3rd distinct LOTO aggregate, not confirmed as a duplicate (inherited) |
| MachineGuardsBarriers | BeginCorrectiveAction / Create / RecordInspectionResult / Retire / ScheduleInspection / VerifyCorrectiveAction | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 8 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Related but distinct from `EmployeeSafety.MachineGuarding` — same inspect/correct/verify shape, not confirmed duplicates |
| PresenceSensingDevices | Create / RecordCalibrationResult / RecordTriggerEvent / Retire / ScheduleCalibration | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| SafetyValvesRelays | BeginRepair / Create / RecordInspectionResult / Retire / ScheduleInspection | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| TwoHandControls | Create / RecordTestResult / Retire / ScheduleTest | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| All 6 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | `Retire` functions as the de facto terminal transition module-wide — more naming-consistent than the rest of the solution |

**Event delivery reality (F9 [EVT-B]):** no consumer class of any kind exists for any event across
this cluster (confirmed no `EmergencyStopDevices*Consumer`/`LotoSystems*Consumer`/etc.). Net: zero
live delivery for all ~31 transitions in this file despite full event-raising correctness confirmed
this phase.
