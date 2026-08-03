# Command-Handler Map — EmployeeSafety

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 7 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-B]), F10, and F2 (handler-bypass confirmed
NOT to recur in this cluster — `RetireMachineGuardingCommandHandler` named example,
`domain-mapping/_shared-findings.md`). **Event-completeness directly re-verified this phase**: a
full `AddDomainEvent`-vs-method-count scripted scan of all 7 aggregate files (excluding the
`ClearDomainEvents` false-positive that inflated Phase 1's raw counts) found **zero methods missing
an `AddDomainEvent` call** across this cluster — every guarded transition raises its event. Command/
handler class names pattern-inferred (F10); not individually opened.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EmergencyPreparedness | Create / ConductDrill / RecordReadiness / ScheduleDrill | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` (F1/F2 pattern) | 5 events confirmed present, all methods covered (this phase's completeness re-scan) | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| EnvironmentalHealthMonitoring | Create / BeginCorrectiveAction / RecordMeasurement / ResolveExceedance | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 5 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Guard density is the thinnest of this cluster (1 guard site for 4 methods, inherited) — events are all present regardless; the risk here is under-guarded preconditions, not missing events |
| ErgonomicsAndMaterialHandling | Create / Close / ImplementControls / RecordAssessment / ScheduleFollowUp | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Has a genuine `Close` — a formal terminal transition, unlike most of this cluster |
| HazardCommunicationTraining | Create / Close / DeliverTraining / ScheduleNextCycle / VerifyComprehension | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| LotoPrograms | ApplyIsolation / Document / RecordAnnualAudit / Release / VerifyZeroEnergy (+ implied Create) | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | No plain `Create` method name found (inherited) — how an instance first comes into existence is still unconfirmed; flagged, not resolved this phase |
| MachineGuarding | FlagBypass / Install / RecordInspectionResult / Retire / ScheduleCorrectiveAction / ScheduleInspection / VerifyCorrectiveAction | matching commands | `RetireMachineGuardingCommandHandler` (named F2 example) + matching siblings | `AddAsync`/`GetByIdAsync`→`UpdateAsync` — **F2 confirms this handler actually invokes the domain method, not bypassed** | 8 events confirmed present, all methods covered | Y | **N** | Directly verified (`RetireMachineGuardingCommandHandler`, F2); event completeness directly verified for all 7 | Most transitions of any single aggregate in the solution (tied with `SafetyPolicy`) — richest EmployeeSafety concept |
| Ppe (EmployeeSafety) | Create / FlagForReplacement / RecordFitTest / RecordInspection / Retire | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 5 events (pattern-inferred names) | Y | **N** | Pattern-inferred | Possible undocumented overlap with `PPE.PPEAssignment` (inherited, unconfirmed) — if the two are ever merged/reconciled, note `PPEAssignment` **is** F8-audited while this one is not, a real behavioral difference between the "duplicates" |
| All 7 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-B]):** no consumer class of any kind exists for any event across
this entire 7-aggregate cluster (confirmed no `EmergencyPreparedness*Consumer`/`MachineGuarding*Consumer`
/etc. in `publishing-map.md`/`consumption-map.md`). Net: **zero live delivery for all ~37 transitions
in this file** despite this phase's confirmation that every one of them correctly raises its event
and reaches the Outbox.
