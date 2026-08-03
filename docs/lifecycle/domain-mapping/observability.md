# Domain Mapping — Observability

See `_shared-findings.md` §F1 (tenancy).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| AutomatedIncidentDetection | Create/Acknowledge/Dismiss/EscalateToIncident/NotifySupervisor | 4 guard + 6 event sites | Pattern-inferred Y | — | `EscalateToIncident` plausibly should create/link a real `SafetyEvents.IncidentManagement` or `AccidentManagement.Accident` record — no confirmed cross-aggregate call found | Not enforced (F1) | Directly verified (counts, inherited) | Candidate cross-module gap: "escalate to incident" that doesn't provably touch the Incident module |
| AutomatedIncidentDetection | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| MachineProcessMonitoring | Create/Acknowledge/RaiseAnomalyFlag/RecordSensorReading | **No Close/Resolve method exists** — an anomaly flag, once raised, has no confirmed way to be marked resolved (Phase 1 confirmed real lifecycle gap) | **N** — confirmed structural gap, not merely unenforced | Sensor reading bounds plausible | None | Not enforced (F1) | Directly verified (inherited) | This aggregate *did* grow past Create, just not far enough — distinct category from "never grew past Create" |
| EnvironmentalAssetMonitoring | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | Dormant Update SQL proc |
| FacilityTrafficMonitoring | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | Same |
| PPEBehavioralMonitoring | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified (this phase) | No confirmed overlap with EmployeeSafety/MachineSafety behavioral concepts |

**Summary:** `MachineProcessMonitoring`'s missing Close/Resolve remains the clearest structural
lifecycle gap in this module — an anomaly, once flagged, is permanently open in the domain model
regardless of what happens in the real world.
