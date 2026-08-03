# Event Map — SafetyEvents (EmergencyManagement, IncidentManagement, Observation)

See `_shared-findings.md` (F14-F17) and `command-handler-map/safetyevents.md`. All 3 are Create-only
marker aggregates with no genuine write proc found. Event bodies directly read this phase
(`EmergencyManagementCreatedEvent`, `IncidentManagementCreatedEvent`, `ObservationCreatedEvent` —
all confirmed identity/actor/correlation-only).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| EmergencyManagement | Create only | Y | `EmergencyManagementCreatedEvent` | Y — minimal-but-correct | **N (EVT-A)** | Directly verified | Distinct from `EmployeeSafety.EmergencyPreparedness`, which has a full write-proc pair and richer event set (`employeesafety.md`) — same real-world concern modeled twice at very different maturity levels, and this is the thinner one |
| IncidentManagement | Create only | Y | `IncidentManagementCreatedEvent` | Y — minimal-but-correct | **N (EVT-A)** | Directly verified | This is the aggregate `Observability.AutomatedIncidentDetection.EscalateToIncident` implies it escalates into (F11, cross-referenced in `observability.md`) — and per this phase's review it is itself among the thinnest, least-persisted concepts in the solution, with no event field of its own that could ever corroborate an inbound escalation reference even if one were checked |
| Observation | Create only | Y | `ObservationCreatedEvent` | Y — minimal-but-correct | **N (EVT-A)** | Directly verified | A related `usp_BehavioralObservationCompletion_GetItems` may or may not be the same concept (unconfirmed, inherited) |
| All 3 above | Update/Close/Cancel/Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** 1 event per aggregate, 1 `AddDomainEvent` call per Create factory
— no possibility of a duplicate.

**Cross-reference note (ties to `observability.md` F11):** `IncidentManagement`'s payload thinness
reinforces F11's conclusion twice over — not only does `AutomatedIncidentDetection.EscalateToIncident`
never validate its `IncidentReferenceId` against this aggregate, but even if it did, this aggregate's
own `Created` event carries no field a validating consumer could cross-check the reference against
(no status, no correlation back to a detection source) — the gap is symmetric on both sides of the
implied relationship.

**Module-level summary:** payloads are minimal-but-complete for all 3 (F14); the real problems are
persistence (no confirmed SQL write path for any of the 3) and the cross-module reference gap noted
above, not payload incompleteness in isolation. All 3 have named, unregistered consumer stubs (F9) —
zero live delivery.
