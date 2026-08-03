# Repository Tenant-Context Classification — Rollout Order

Companion to `docs/lifecycle/security/rls-audit.md`. Covers all 303 `*DapperRepository.cs` files
under `PlantSafety.Infrastructure` (6 parallel batches of ~50 files each, every file read in full).
Rollout order per the staged remediation plan: **fix Class B first** (highest risk — unfiltered
writes), then Class A, then D, then C (already correct, no action needed).

## Totals

| Class | Count | Definition | Action needed? |
|---|---|---|---|
| **B** | **67** | `GetByIdAsync`/`UpdateAsync` on the same aggregate, at least one missing `TenantId` | **Yes — priority fix list below** |
| A | ~42 | Pure read-only or GetById-only repos with a tenant gap on a lesser-risk path (e.g. child-collection methods) | Lower priority, see notes per-file in source batch reports |
| D | ~194 | Correctly-built newer `Features/*` pattern (TenantId threaded everywhere), pure list read-models with TenantId already present, server-level/cross-database platform infra deliberately out of tenant-RLS scope (Operations/Archival, Backup, Monitoring, etc.), or bulk/cross-aggregate operations | No fix needed |
| C | 0 | Write-only Create repos (already correct by construction) | No fix needed |

Note: several repositories additionally have **zero implementation at all** for their Domain write
interface (confirmed via direct grep against `ICertificationRepository`, `IManagementOfChangeRepository`,
`IPermitsRepository`, `IPolicyRepository`, `IRegulatoryCalendarRepository`, and others) — this is the
separate G1 finding from `docs/lifecycle/sql-shape/_shared-findings.md`, not a Class B tenant-context
gap, since there is no write path to fix in the first place.

## Class B — full priority fix list (67 repositories)

Grouped by module/feature area. `AccidentDapperRepository` (row 1) is the completed pilot template —
every other row follows the identical fix shape: add `Guid tenantId` to `GetByIdAsync`, thread it
into `UpdateAsync` via the aggregate's own `TenantId` property, update the paired stored procedures
to accept and filter by `@TenantId`, update the Domain interface, update all Application-layer callers.

**AccidentManagement** (pilot, done)
1. `AccidentManagement/Persistence/AccidentDapperRepository.cs` — ✅ fixed this session

**Alerts / Dashboards / Auditing / Checklists**
2. `Alerts/Alert/Persistence/AlertDapperRepository.cs`
3. `Auditing/AuditEngagement/Persistence/AuditEngagementDapperRepository.cs`
4. `Auditing/AuditFinding/Persistence/AuditFindingDapperRepository.cs`
5. `Checklists/ChecklistCompletion/Persistence/ChecklistCompletionDapperRepository.cs`
6. `Checklists/ChecklistTemplate/Persistence/ChecklistTemplateDapperRepository.cs`
7. `Dashboards/Dashboard/Persistence/DashboardDapperRepository.cs`
8. `Dashboards/Widget/Persistence/WidgetDapperRepository.cs`

**ContractorSafety**
9. `ContractorSafety/ActiveMonitoringAuditing/Persistence/ActiveMonitoringAuditingDapperRepository.cs`
10. `ContractorSafety/IncidentTrackingPerformanceEvaluation/Persistence/IncidentTrackingPerformanceEvaluationDapperRepository.cs`
11. `ContractorSafety/PermitToWork/Persistence/PermitToWorkDapperRepository.cs`
12. `ContractorSafety/PreQualificationVetting/Persistence/PreQualificationVettingDapperRepository.cs`
13. `ContractorSafety/SiteOrientation/Persistence/SiteOrientationDapperRepository.cs`

**EmployeeSafety**
14. `EmployeeSafety/EmergencyPreparedness/Persistence/EmergencyPreparednessDapperRepository.cs`
15. `EmployeeSafety/EnvironmentalHealthMonitoring/Persistence/EnvironmentalHealthMonitoringDapperRepository.cs`
16. `EmployeeSafety/ErgonomicsAndMaterialHandling/Persistence/ErgonomicsAndMaterialHandlingDapperRepository.cs`
17. `EmployeeSafety/HazardCommunicationTraining/Persistence/HazardCommunicationTrainingDapperRepository.cs`
18. `EmployeeSafety/LotoPrograms/Persistence/LotoProgramsDapperRepository.cs`
19. `EmployeeSafety/MachineGuarding/Persistence/MachineGuardingDapperRepository.cs`
20. `EmployeeSafety/Ppe/Persistence/PpeDapperRepository.cs`

**MachineSafety**
21. `MachineSafety/EmergencyStopDevices/Persistence/EmergencyStopDevicesDapperRepository.cs`
22. `MachineSafety/LotoSystems/Persistence/LotoSystemsDapperRepository.cs`
23. `MachineSafety/MachineGuardsBarriers/Persistence/MachineGuardsBarriersDapperRepository.cs`
24. `MachineSafety/PresenceSensingDevices/Persistence/PresenceSensingDevicesDapperRepository.cs`
25. `MachineSafety/SafetyValvesRelays/Persistence/SafetyValvesRelaysDapperRepository.cs`
26. `MachineSafety/TwoHandControls/Persistence/TwoHandControlsDapperRepository.cs`

**Observability**
27. `Observability/AutomatedIncidentDetection/Persistence/AutomatedIncidentDetectionDapperRepository.cs`
28. `Observability/EnvironmentalAssetMonitoring/Persistence/EnvironmentalAssetMonitoringDapperRepository.cs`
29. `Observability/FacilityTrafficMonitoring/Persistence/FacilityTrafficMonitoringDapperRepository.cs`
30. `Observability/MachineProcessMonitoring/Persistence/MachineProcessMonitoringDapperRepository.cs`
31. `Observability/PPEBehavioralMonitoring/Persistence/PPEBehavioralMonitoringDapperRepository.cs`

**PPE**
32. `PPE/PPEAssignment/Persistence/PPEAssignmentDapperRepository.cs`
33. `PPE/PPECategory/Persistence/PPECategoryDapperRepository.cs`
34. `PPE/PPEInventory/Persistence/PPEInventoryDapperRepository.cs`
35. `PPE/PPEZoneMapping/Persistence/PPEZoneMappingDapperRepository.cs`

**PermitToWork**
36. `PermitToWork/PermitExpiration/Persistence/PermitExpirationDapperRepository.cs`
37. `PermitToWork/PermitScheduling/Persistence/PermitSchedulingDapperRepository.cs`
38. `PermitToWork/PermitTemplate/Persistence/PermitTemplateDapperRepository.cs`
39. `PermitToWork/PermitWorkflow/Persistence/PermitWorkflowDapperRepository.cs`

**RiskManagement / SafetyCulture / SafetyPolicies / Scorecards**
40. `RiskManagement/Persistence/RiskDapperRepository.cs`
41. `SafetyCulture/CultureAssessment/Persistence/CultureAssessmentDapperRepository.cs`
42. `SafetyCulture/StopWorkEvent/Persistence/StopWorkEventDapperRepository.cs`
43. `SafetyPolicies/Persistence/SafetyPolicyDapperRepository.cs`
44. `Scorecards/Persistence/ScorecardDapperRepository.cs`

**SafetyProcedures** (dual write+read-model class shape — same fix, no separate Read sibling to touch)
45. `SafetyProcedures/EmergencyResponsePlanning/Persistence/EmergencyResponsePlanningDapperRepository.cs`
46. `SafetyProcedures/ErgonomicsWorkstationDesign/Persistence/ErgonomicsWorkstationDesignDapperRepository.cs`
47. `SafetyProcedures/HazardCommunicationRightToKnow/Persistence/HazardCommunicationRightToKnowDapperRepository.cs`
48. `SafetyProcedures/LotoProcedures/Persistence/LotoProceduresDapperRepository.cs`
49. `SafetyProcedures/MachineGuardingInspections/Persistence/MachineGuardingInspectionsDapperRepository.cs`
50. `SafetyProcedures/MaterialHandlingStorage/Persistence/MaterialHandlingStorageDapperRepository.cs`
51. `SafetyProcedures/PpePolicies/Persistence/PpePoliciesDapperRepository.cs`
52. `SafetyProcedures/RoutineSafetyAuditsIncidentReporting/Persistence/RoutineSafetyAuditsIncidentReportingDapperRepository.cs`

**Scheduling**
53. `Scheduling/EmergencyDrillPlanning/Persistence/EmergencyDrillPlanningDapperRepository.cs`
54. `Scheduling/HazardIsolationSequencing/Persistence/HazardIsolationSequencingDapperRepository.cs`
55. `Scheduling/PMScheduling/Persistence/PMSchedulingDapperRepository.cs`
56. `Scheduling/ShiftFatigue/Persistence/ShiftFatigueDapperRepository.cs`
57. `Scheduling/WorkOrderPriority/Persistence/WorkOrderPriorityDapperRepository.cs`

## Notable cross-cutting observations from the classification pass

- **Several files self-document the gap already** (a good sign for remediation — the original
  authors knew): `AlertDapperRepository`, `AuditEngagementDapperRepository`, `AuditFindingDapperRepository`,
  `ChecklistCompletionDapperRepository`, `ChecklistTemplateDapperRepository`, `CultureAssessmentDapperRepository`,
  `StopWorkEventDapperRepository`, and the `AccidentDapperRepository` pilot itself all have header
  comments acknowledging `ListAsync` has no tenant scoping and evaluates in-memory. `ListAsync` on
  every Class B repository has this same second-order gap and should be fixed in the same pass as
  `GetByIdAsync`/`UpdateAsync` for that file, even though it wasn't the audit's primary focus.
- **Naming-collision risk for any scripted/automated fix**: several class *and interface* names
  repeat verbatim across unrelated namespaces (`ApprovalsDapperRepository`/`IApprovalsRepository`,
  `ChemicalHandlingDapperRepository`, `EPADapperRepository`/`IEPARepository`, `DOTDapperRepository`-family,
  `PPEManagementDapperRepository` (3 unrelated classes), `RiskManagementDapperRepository` (2 unrelated
  classes), `SchedulesDapperRepository` (2 unrelated classes)). Any fix must be scoped by full file
  path/namespace, never by bare class name.
- **No new G1 zero-implementation aggregates** were found beyond what `sql-shape/_shared-findings.md`
  already listed.
