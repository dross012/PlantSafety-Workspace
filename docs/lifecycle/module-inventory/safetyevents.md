# Module Inventory — SafetyEvents

3 create-only aggregates: `EmergencyManagement`, `IncidentManagement`, `Observation`. **[NEW this
phase]**: exact-prefix SQL cross-reference found no genuine write proc for any of the 3 — only
adjacent read-side "Items" reporting procs with related-but-different compound names, the same shape
found in `compliance.md`.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| EmergencyManagement | Create only | Yes | **No genuine write proc found** — only `usp_EmergencyManagementItems_GetEmergencies` (read-only, different compound name) | Unconfirmed — candidate write-orphan | Directly verified (file-name census) | Distinct from `EmployeeSafety.EmergencyPreparedness` (which does have a full `_Create`/`_Update` pair) — same real-world concern, two different-shaped aggregates |
| IncidentManagement | Create only | Yes | **No genuine write proc found** — only `usp_IncidentManagementItems_GetIncidents` (read-only) | Unconfirmed — candidate write-orphan | Directly verified (file-name census) | Distinct from `AccidentManagement.Accident` and `ContractorSafety.IncidentTrackingPerformanceEvaluation` — a 3rd, thinnest "incident" concept in the solution |
| Observation | Create only | Yes | **No genuine write proc found** — only `usp_ObservationItems_GetObservations` (read-only); a related `usp_BehavioralObservationCompletion_GetItems` also exists but is not confirmed to be the same aggregate | Unconfirmed — candidate write-orphan | Directly verified (file-name census) | |
| All 3 above | Update/Close/Cancel/Delete | **No** — none exist | N/A | N/A | Directly verified | Same caveat as `compliance.md`: file-name matching only, not a repository-code read — a differently-named write proc is possible and was not ruled out |
