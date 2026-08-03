# Domain Mapping — SafetyEvents

See `_shared-findings.md` §F1 (tenancy). 3 create-only aggregates, condensed.

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| EmergencyManagement | Create only | — | N/A | Not confirmed | Distinct from `EmployeeSafety.EmergencyPreparedness` (which has full C#+SQL) — same real-world concern, thinner shape here; no coordination confirmed | Not enforced (F1) | Directly verified (file-name census, Phase 1) | No genuine write proc found |
| IncidentManagement | Create only | — | N/A | — | 3rd "incident" concept (with `AccidentManagement.Accident`, `ContractorSafety.IncidentTrackingPerformanceEvaluation`) — thinnest of the three, no coordination confirmed | Not enforced (F1) | Directly verified | No genuine write proc found |
| Observation | Create only | — | N/A | — | Related `usp_BehavioralObservationCompletion_GetItems` may or may not be the same aggregate — not disambiguated | Not enforced (F1) | Directly verified | No genuine write proc found |

**Summary:** All 3 remain the thinnest-shaped "incident/emergency/observation" concepts in the
solution, each shadowed by a richer sibling elsewhere (EmployeeSafety.EmergencyPreparedness,
AccidentManagement.Accident/ContractorSafety.IncidentTrackingPerformanceEvaluation) with no
confirmed coordination in either direction.
