# Rule Enforcement Validation — SafetyProcedures

See `_shared-findings.md`. 8 rich sub-aggregates: 6 use a document lifecycle
(Draft→SubmitForApproval→Publish→Retire), 2 use an inspection-cycle shape (Conduct/Execute→
BeginCorrectiveAction→Close).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| EmergencyResponsePlanning — Create/Publish/RecordDrill/Retire/ScheduleDrill/SubmitForApproval | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `EmergencyResponsePlanningDapperRepository` confirmed hardcoded site | Medium | |
| ErgonomicsWorkstationDesign — Create/Publish/RecordAssessment/Retire/ScheduleAssessment/SubmitForApproval | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `ErgonomicsWorkstationDesignDapperRepository` confirmed hardcoded site | Medium | |
| HazardCommunicationRightToKnow — Create/Publish/Retire/ScheduleRevision/SubmitForApproval | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `HazardCommunicationRightToKnowDapperRepository` confirmed hardcoded site | Medium | Publish reached directly from Draft via SubmitForApproval, no mid-step — narrower sequence, still guarded per Phase 1 counts |
| LotoProcedures — Create/Publish/RecordAnnualAudit/Retire/ScheduleRevision/SubmitForApproval | Pass — 4 guard sites | Unconfirmed | Pass — 3rd LOTO-named aggregate in the solution, no shared FK with the other two | Pass | Fail (F1, pattern) | Fail (G2) — `LotoProceduresDapperRepository` confirmed hardcoded site | Medium | |
| MachineGuardingInspections — BeginCorrectiveAction/Close/Create/ExecuteInspection | Pass — 3 guard sites; genuine `Close` | Unconfirmed | Pass — no FK to `EmployeeSafety.MachineGuarding`/`MachineSafety.MachineGuardsBarriers` found (checked this phase) | Pass | Fail (F1, pattern) | Fail (G2) — `MachineGuardingInspectionsDapperRepository` confirmed hardcoded site | Medium | Checked directly this phase for a cross-module equipment-FK gap — none found |
| MaterialHandlingStorage — Create/FlagObstruction/Publish/ResolveObstruction/Retire/SubmitForApproval | Pass — 4 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `MaterialHandlingStorageDapperRepository` confirmed hardcoded site | Medium | |
| PpePolicies — Create/Publish/RecordEnforcementAudit/Retire/ScheduleRevision/SubmitForApproval | Pass — 4 guard sites | Unconfirmed | Pass — governs the policy document, not physical PPE units; distinct from `PPE.PPEAssignment`/`EmployeeSafety.Ppe` | Pass | Fail (F1, pattern) | Fail (G2) — `PpePoliciesDapperRepository` confirmed hardcoded site | Medium | |
| RoutineSafetyAuditsIncidentReporting — BeginCorrectiveAction/Close/Conduct/Create | Pass — 3 guard sites | Unconfirmed | Pass — 4th Auditing-adjacent aggregate; no shared identity with `Auditing.AuditEngagement`/`Compliance.Auditing` found | Pass | Fail (F1, pattern) | Fail (G2) — `RoutineSafetyAuditsIncidentReportingDapperRepository` confirmed hardcoded site | Medium | |
| All 8 above — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | Missing entirely on all 8 |
