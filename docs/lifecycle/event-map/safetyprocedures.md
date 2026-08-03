# Event Map — SafetyProcedures (EmergencyResponsePlanning, ErgonomicsWorkstationDesign, HazardCommunicationRightToKnow, LotoProcedures, MachineGuardingInspections, MaterialHandlingStorage, PpePolicies, RoutineSafetyAuditsIncidentReporting)

See `_shared-findings.md` (F14-F17) and `command-handler-map/safetyprocedures.md` (event-completeness
scan: zero methods found missing `AddDomainEvent` across all 8 aggregates — the largest single
cluster in the solution, ~48 transitions). Same consolidated-file naming pattern confirmed again here.

**Directly re-verified this phase**: `LotoProcedures.cs`'s 7 `AddDomainEvent` sites (1:1 to its 7
methods, no duplicates — F17 clean), events named `LotoProcedureDraftedEvent`,
`SubmittedForApprovalEvent`, `PublishedEvent`, `AuditPassedEvent`, `AuditFlaggedEvent`,
`RevisionScheduledEvent`, `RetiredEvent`. This confirms the module's document-lifecycle shape
(Draft→SubmitForApproval→Publish→Retire, shared by 5 of 8 aggregates per command-handler-map) maps
1:1 onto a matching event per stage, with no gaps.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| LotoProcedures | Create (Draft) | Y | `LotoProcedureDraftedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | SubmitForApproval | Y | `LotoProcedureSubmittedForApprovalEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | Publish | Y | `LotoProcedurePublishedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | RecordAnnualAudit (Pass) | Y | `LotoProcedureAuditPassedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | RecordAnnualAudit (Flag) | Y | `LotoProcedureAuditFlaggedEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | ScheduleRevision | Y | `LotoProcedureRevisionScheduledEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | |
| LotoProcedures | Retire | Y | `LotoProcedureRetiredEvent` | Pattern-inferred Y | **N** | Directly verified (call site) | 3rd distinct LOTO-named aggregate in the solution (with `EmployeeSafety.LotoPrograms`, `MachineSafety.LotoSystems`) — not confirmed duplicates |
| EmergencyResponsePlanning | Create / Publish / RecordDrill / Retire / ScheduleDrill / SubmitForApproval | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | Document-lifecycle shape, same as directly-confirmed `LotoProcedures` |
| ErgonomicsWorkstationDesign | Create / Publish / RecordAssessment / Retire / ScheduleAssessment / SubmitForApproval | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | |
| HazardCommunicationRightToKnow | Create / Publish / Retire / ScheduleRevision / SubmitForApproval | Pattern-inferred Y (6 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | No mid-lifecycle `RecordX` step, unlike siblings |
| MachineGuardingInspections | BeginCorrectiveAction / Close / Create / ExecuteInspection | Pattern-inferred Y (6 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | Inspection-cycle shape, not document-shape — has a real `Close` |
| MaterialHandlingStorage | Create / FlagObstruction / Publish / ResolveObstruction / Retire / SubmitForApproval | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | |
| PpePolicies | Create / Publish / RecordEnforcementAudit / Retire / ScheduleRevision / SubmitForApproval | Pattern-inferred Y (7 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | 3rd PPE-adjacent aggregate (with `PPE.PPEAssignment`, `EmployeeSafety.Ppe`) |
| RoutineSafetyAuditsIncidentReporting | BeginCorrectiveAction / Close / Conduct / Create | Pattern-inferred Y (6 event sites, inherited) | Not individually opened this phase | Pattern-inferred Y | **N** | Pattern-inferred | 4th Auditing-adjacent aggregate — inspection-cycle shaped |
| All 8 aggregates above | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | 6 of 8 use document-lifecycle shape, 2 use inspection-cycle shape |

**Duplicate-publish check (F17):** `LotoProcedures.cs` — 7 `AddDomainEvent` sites for 7 methods,
1:1, no duplicates (directly re-verified this phase). Other 7 aggregates not individually re-opened;
inherited scripted scan found zero methods missing an `AddDomainEvent` call across the whole
8-aggregate, ~48-transition cluster.

**Module-level summary:** the largest single transition count of any file in this phase (~48) is
consistent with every other rich PlantSafety aggregate reviewed — clean 1:1 event mapping, no
duplicates found in the directly-sampled aggregate. Zero live delivery for any transition (F9) — no
consumer class of any kind exists for any of the 8 aggregates.
