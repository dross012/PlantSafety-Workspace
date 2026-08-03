# Domain Mapping — SafetyProcedures

See `_shared-findings.md` §F1 (tenancy), §F2 (handler-bypass — no bypass found in this cluster).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| EmergencyResponsePlanning | Create/Publish/RecordDrill/Retire/ScheduleDrill/SubmitForApproval | Draft→Submit→Publish document-lifecycle shape (Phase 1 finding) | Pattern-inferred Y | Approval fields plausible | Plausibly should link to `EmployeeSafety.EmergencyPreparedness` (the drills executed against this plan) — no confirmed link | Not enforced (F1) | Directly verified (counts, Phase 1) | 3rd Emergency-named aggregate — see `employeesafety.md` note |
| EmergencyResponsePlanning | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| ErgonomicsWorkstationDesign | Create/Publish/RecordAssessment/Retire/ScheduleAssessment/SubmitForApproval | Same document-lifecycle shape | Pattern-inferred Y | — | Plausibly related to `EmployeeSafety.ErgonomicsAndMaterialHandling` — no confirmed link | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| ErgonomicsWorkstationDesign | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| HazardCommunicationRightToKnow | Create/Publish/Retire/ScheduleRevision/SubmitForApproval | No mid-lifecycle `RecordX` step, unlike siblings (Phase 1 finding) | Pattern-inferred Y | — | Related to `EmployeeSafety.HazardCommunicationTraining` — no confirmed link | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| HazardCommunicationRightToKnow | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| LotoProcedures | Create/Publish/RecordAnnualAudit/Retire/ScheduleRevision/SubmitForApproval | 3rd LOTO-named aggregate (with `EmployeeSafety.LotoPrograms`, `MachineSafety.LotoSystems`) | Pattern-inferred Y | — | Same 3-way LOTO coordination gap noted in `machinesafety.md` | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| LotoProcedures | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| MachineGuardingInspections | BeginCorrectiveAction/Close/Create/ExecuteInspection | Inspection-cycle shape, not document-shaped; has a real `Close` | Pattern-inferred Y | — | Related to `EmployeeSafety.MachineGuarding`/`MachineSafety.MachineGuardsBarriers` — no confirmed link | Not enforced (F1) | Directly verified (counts, Phase 1) | 3rd MachineGuarding-adjacent aggregate |
| MachineGuardingInspections | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| MaterialHandlingStorage | Create/FlagObstruction/Publish/ResolveObstruction/Retire/SubmitForApproval | Document lifecycle + obstruction sub-flow | Pattern-inferred Y | — | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| MaterialHandlingStorage | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PpePolicies | Create/Publish/RecordEnforcementAudit/Retire/ScheduleRevision/SubmitForApproval | 3rd PPE-adjacent aggregate (with `PPE.PPEAssignment`, `EmployeeSafety.Ppe`) — governs the policy document, not physical PPE (Phase 1 finding, not a duplicate) | Pattern-inferred Y | — | Distinct concept from PPEAssignment/Ppe — no confirmed link needed or found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| PpePolicies | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| RoutineSafetyAuditsIncidentReporting | BeginCorrectiveAction/Close/Conduct/Create | 4th Auditing-adjacent aggregate (with `Auditing.AuditEngagement`/`AuditFinding`, `Compliance.Auditing`) | Pattern-inferred Y | — | Not confirmed as overlapping with the other 3 Auditing-named concepts | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| RoutineSafetyAuditsIncidentReporting | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** This is the largest "newly discovered rich" cluster (8 aggregates) and the one with the
most naming-overlap candidates (Emergency, Ergonomics, HazardCommunication, LOTO, MachineGuarding,
PPE, Auditing all have a sibling elsewhere in the solution). None were confirmed as true duplicates
this phase, but none were confirmed as cross-checked with their siblings either — a recurring
"parallel concept, zero coordination" shape worth a dedicated Phase 3 pass across all these pairs at
once rather than per-module.
