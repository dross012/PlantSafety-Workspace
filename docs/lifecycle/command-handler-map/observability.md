# Command-Handler Map — Observability

See `_shared-findings.md` for F7 (Outbox universal), F8 (**4 of 5 aggregates in this file are on the
23-aggregate SQL-audit list**: `AutomatedIncidentDetection`, `EnvironmentalAssetMonitoring`,
`FacilityTrafficMonitoring`, `PPEBehavioralMonitoring` all confirmed present — `MachineProcessMonitoring`
**is also confirmed present**, making it 5 of 5), F9 (event delivery, [EVT-B]), F10, and **F11
(re-verification: `EscalateDetectionToIncidentCommandHandler` does NOT make a genuine cross-module
call into IncidentManagement — CONFIRMED, resolved this phase)**.
**`EscalateDetectionToIncidentCommandHandler` fully directly verified this phase.**

| Aggregate | Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| AutomatedIncidentDetection | Create | `CreateAutomatedIncidentDetectionCommand` (pattern-inferred) | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| AutomatedIncidentDetection | Acknowledge | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| AutomatedIncidentDetection | Dismiss | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| AutomatedIncidentDetection | **EscalateToIncident** | `EscalateDetectionToIncidentCommand` | `EscalateDetectionToIncidentCommandHandler` | `IAutomatedIncidentDetectionRepository.GetByIdAsync` → `UpdateAsync` — **only dependency; no `IIncidentRepository`/SafetyEvents repository of any kind** | `AutomatedIncidentDetection`'s own escalation event (pattern-inferred name; not individually confirmed) | Y | **Y** (SQL row still fires, F8) | **Directly verified (F11)** | **F11 resolved:** `IncidentReferenceId` is a bare caller-supplied `Guid`, never validated against SafetyEvents/IncidentManagement. Despite the name, this is **not** a cross-module transition — it is a same-aggregate field write that merely *implies* a relationship. Same category as F6 (AuditEngagement.Close's unchecked ComplianceScore). If the caller supplies a nonexistent or already-closed Incident's ID, nothing detects it. |
| AutomatedIncidentDetection | NotifySupervisor | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Name implies a notification side-effect (email/SMS/push) — not confirmed this phase whether any such integration actually fires, or whether this is purely a status-field write like `EscalateToIncident` turned out to be |
| AutomatedIncidentDetection | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| MachineProcessMonitoring | Create | Command name pattern-inferred | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| MachineProcessMonitoring | Acknowledge | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| MachineProcessMonitoring | RaiseAnomalyFlag | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | **No Close/Resolve method exists anywhere in the domain model** (inherited finding) — once raised, an anomaly flag has no confirmed path back to a resolved state; distinct from the Create-only-aggregate pattern since this one demonstrably grew, just not to closure |
| MachineProcessMonitoring | RecordSensorReading | Command name pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| EnvironmentalAssetMonitoring | Create only | Command name pattern-inferred | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |
| FacilityTrafficMonitoring | Create only | Command name pattern-inferred | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |
| PPEBehavioralMonitoring | Create only | Command name pattern-inferred | matching handler | `AddAsync` | pattern-inferred name | Y | **Y** | Directly verified (SQL, inherited) | Dormant `_Update` proc, unused |

**Event delivery reality (F9 [EVT-B]):** no consumer class exists for any event in this module —
confirmed no `AutomatedIncidentDetection*Consumer`/`MachineProcessMonitoring*Consumer`/etc. anywhere
in `publishing-map.md`/`consumption-map.md`. Net: zero live delivery for every transition, including
`EscalateToIncident` — so even setting aside F11's "not a real cross-module call" finding, no
downstream system (IncidentManagement or otherwise) would learn about the escalation even if the
referenced Incident ID were valid.

**Audit summary for this module:** highest audit-coverage-to-module-size ratio outside Scheduling —
all 5 aggregates are on the F8 list, so every transition writes a best-effort/anonymous `AuditLog`
row despite the module having zero working event/consumer coverage and (for `EscalateToIncident`)
an unverified cross-module reference.
