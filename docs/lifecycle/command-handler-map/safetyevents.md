# Command-Handler Map — SafetyEvents

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 3 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-A]), F10. All 3 are Create-only marker
aggregates with no genuine write proc found (file-name census, inherited). Command/handler names
pattern-inferred; not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| EmergencyManagement | Create only | `CreateEmergencyManagementCommand` | matching handler | `AddAsync` — no genuine write proc (inherited) | `EmergencyManagementCreatedEvent` | Y (Outbox row regardless of SQL outcome) | **N** | Directly verified (SQL census, inherited) | Distinct from `EmployeeSafety.EmergencyPreparedness`, which does have a full write-proc pair — same real-world concern modeled twice at very different maturity levels |
| IncidentManagement | Create only | `CreateIncidentManagementCommand` | matching handler | `AddAsync` — no genuine write proc | `IncidentManagementCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | This is the aggregate `AutomatedIncidentDetection.EscalateToIncident` (see `observability.md`, F11) *implies* it escalates into — and it is itself among the thinnest, least-persisted concepts in the solution. Even if `EscalateToIncident`'s reference check existed, the target it would be checking against has no confirmed write path of its own. |
| Observation | Create only | `CreateObservationCommand` | matching handler | `AddAsync` — no genuine write proc; a related `usp_BehavioralObservationCompletion_GetItems` may or may not be the same concept (unconfirmed) | `ObservationCreatedEvent` | Y | **N** | Directly verified (SQL census, inherited) | |
| All 3 above | Update/Close/Cancel/Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-A]):** all 3 events have named, unregistered consumer stubs
(`EmergencyManagementCreatedConsumer`, `IncidentManagementCreatedConsumer`, `ObservationCreatedConsumer`
— `publishing-map.md` lines 80-82/`consumption-map.md` lines 43-45), subject to the standard
unregistered + wrong-CLR-type double defect. Net: zero live delivery, compounding the fact that none
of the 3 underlying aggregates has a confirmed SQL write path in the first place.

**Cross-reference note (ties to `observability.md` F11):** `IncidentManagement`'s apparent thinness
here reinforces F11's conclusion that `AutomatedIncidentDetection.EscalateToIncident` cannot be
performing a meaningful cross-module verification — even if it tried, there is no confirmed
`IIncidentManagementRepository`-style write/read path on this side to verify against.
