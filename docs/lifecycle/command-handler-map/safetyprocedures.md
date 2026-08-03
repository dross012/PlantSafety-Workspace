# Command-Handler Map — SafetyProcedures

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 8 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-B]), F10, F2 (handler-bypass confirmed NOT to
recur in this cluster, `domain-mapping/_shared-findings.md`). **Event-completeness directly
re-verified this phase** for all 8 aggregates — zero methods found missing an `AddDomainEvent` call
(same scripted-scan method as `employeesafety.md`/`machinesafety.md`). Command/handler class names
pattern-inferred; not individually opened. Largest single "newly discovered rich" cluster in the
solution (8 aggregates, ~48 transitions).

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EmergencyResponsePlanning | Create / Publish / RecordDrill / Retire / ScheduleDrill / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` (F1/F2 pattern) | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Draft→Submit→Publish document-lifecycle shape, shared by 5 of this cluster's 8 aggregates |
| ErgonomicsWorkstationDesign | Create / Publish / RecordAssessment / Retire / ScheduleAssessment / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| HazardCommunicationRightToKnow | Create / Publish / Retire / ScheduleRevision / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | No mid-lifecycle `RecordX` step, unlike siblings |
| LotoProcedures | Create / Publish / RecordAnnualAudit / Retire / ScheduleRevision / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 8 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | 3rd distinct LOTO-named aggregate in the solution (with `EmployeeSafety.LotoPrograms`, `MachineSafety.LotoSystems`) — not confirmed duplicates of each other |
| MachineGuardingInspections | BeginCorrectiveAction / Close / Create / ExecuteInspection | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | Inspection-cycle shape, not document-shape — has a real `Close` |
| MaterialHandlingStorage | Create / FlagObstruction / Publish / ResolveObstruction / Retire / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | |
| PpePolicies | Create / Publish / RecordEnforcementAudit / Retire / ScheduleRevision / SubmitForApproval | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 7 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | 3rd PPE-adjacent aggregate (with `PPE.PPEAssignment`, `EmployeeSafety.Ppe`) — governs the policy *document*, not physical PPE units; distinct concept |
| RoutineSafetyAuditsIncidentReporting | BeginCorrectiveAction / Close / Conduct / Create | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | 6 events confirmed present, all methods covered | Y | **N** | Pattern-inferred (event completeness directly verified) | 4th Auditing-adjacent aggregate (with `Auditing.AuditEngagement`/`AuditFinding`, `Compliance.Auditing`) — inspection-cycle shaped, not confirmed overlapping |
| All 8 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | 6 of 8 use document-lifecycle shape (Draft→SubmitForApproval→Publish→Retire); 2 use inspection-cycle shape (Conduct/Execute→BeginCorrectiveAction→Close) — a genuine, consistent design-pattern split within one feature area |

**Event delivery reality (F9 [EVT-B]):** no consumer class of any kind exists for any event across
this 8-aggregate, ~48-transition cluster (confirmed no `EmergencyResponsePlanning*Consumer`/
`LotoProcedures*Consumer`/etc.). Net: **zero live delivery for the single largest transition count of
any module file in this document set**, despite full event-raising correctness confirmed this phase.
