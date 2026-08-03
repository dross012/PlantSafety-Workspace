# Module Inventory — Observability

5 aggregates: `AutomatedIncidentDetection`, `MachineProcessMonitoring` (rich); `EnvironmentalAssetMonitoring`,
`FacilityTrafficMonitoring`, `PPEBehavioralMonitoring` (create-only).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| AutomatedIncidentDetection | Create / Acknowledge / Dismiss / EscalateToIncident / NotifySupervisor | Yes (all 5, 4 guard + 6 event sites) | Yes — `usp_AutomatedIncidentDetection_Insert`/`_Update` | Live | Directly verified (counts, inherited) | |
| AutomatedIncidentDetection | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| MachineProcessMonitoring | Create / Acknowledge / RaiseAnomalyFlag / RecordSensorReading | Yes (all 4, 3 guard + 5 event sites) | Yes — `usp_MachineProcessMonitoring_Insert`/`_Update` | Live | Directly verified (counts, inherited) | **No Close/Resolve method exists** — an anomaly flag, once raised, has no confirmed way to be marked resolved in the domain model. A real lifecycle gap, distinct from the "never grew past Create" pattern: this aggregate *did* grow, just not far enough to close its own alert |
| EnvironmentalAssetMonitoring | Create only | Yes | Yes — `usp_EnvironmentalAssetMonitoring_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| FacilityTrafficMonitoring | Create only | Yes | Yes — `usp_FacilityTrafficMonitoring_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create |
| PPEBehavioralMonitoring | Create only | Yes | Yes — `usp_PPEBehavioralMonitoring_Insert`/`_Update` (dormant Update) | Live (Create only) | Directly verified (this phase) | Missing entirely beyond Create; distinct from `EmployeeSafety.ErgonomicsAndMaterialHandling` and `MachineSafety.*` behavioral-adjacent concepts — no confirmed overlap |
