# Event Map — Observability (AutomatedIncidentDetection, MachineProcessMonitoring, EnvironmentalAssetMonitoring, FacilityTrafficMonitoring, PPEBehavioralMonitoring)

See `_shared-findings.md` (F14-F17) and `command-handler-map/observability.md` (esp. F11 —
`EscalateToIncident` is not a real cross-module call). **Fully directly verified this phase**: read
of all 5 `AutomatedIncidentDetection` events, all 4 `MachineProcessMonitoring` events, and all 3
Create-only events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| AutomatedIncidentDetection | Create | Y | `AutomatedIncidentDetectionCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| AutomatedIncidentDetection | Acknowledge | Y | `AutomatedIncidentDetectionAcknowledgedEvent` | Y — `AcknowledgedBy (UserId), AcknowledgedAtUtc` | **N** | Directly verified | |
| AutomatedIncidentDetection | Dismiss | Y | `AutomatedIncidentDetectionDismissedEvent` | Y — `Reason (string), DismissedAtUtc` | **N** | Directly verified | |
| AutomatedIncidentDetection | **EscalateToIncident** | Y | `AutomatedIncidentDetectionEscalatedToIncidentEvent` | **Y for what it claims to do, but the claim itself is the problem**: payload is `IncidentReferenceId (Guid — untyped, not `IncidentId`), EscalatedAtUtc` — per F11 (inherited/re-confirmed), this `Guid` is never validated against SafetyEvents/IncidentManagement before being stamped into the event, so **the event asserts a relationship that the domain layer never checked exists**; a consumer trusting this field could be propagating a reference to a nonexistent or already-closed Incident | **N** | Directly verified | The clearest "payload complete but the field's trustworthiness is separately broken" case in this phase's whole review, sharper than `AuditEngagementClosedEvent`'s F6 caveat because here the untyped `Guid` (vs. every other cross-aggregate reference in the solution using a typed ID) is itself a small tell that this field was bolted on without the same rigor as the rest of the event |
| AutomatedIncidentDetection | NotifySupervisor | Y | `AutomatedIncidentDetectionSupervisorNotifiedEvent` | Y — `NotifiedSupervisorUserId (UserId), NotifiedAtUtc` — complete payload, though (per command-handler-map) it is unconfirmed whether any real notification integration (email/SMS/push) actually fires when this event is raised, or whether this is purely a status-field write | **N** | Directly verified | |
| AutomatedIncidentDetection | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| MachineProcessMonitoring | Create | Y | `MachineProcessMonitoringCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| MachineProcessMonitoring | Acknowledge | Y | `MachineProcessMonitoringAcknowledgedEvent` | Y — `AcknowledgedBy (UserId), AcknowledgedAtUtc` | **N** | Directly verified | |
| MachineProcessMonitoring | RaiseAnomalyFlag | Y | `MachineProcessMonitoringAnomalyFlaggedEvent` | Y (**F15 caveat, sharpest form**) — `AnomalyDescription (string), FlaggedAtUtc` — since (per inherited finding) **no Close/Resolve method exists anywhere in this aggregate**, this event is not just missing an "old value," the aggregate itself has no mechanism to ever emit a corresponding "anomaly cleared" event; a consumer has no way, even in principle, to know when this flag stops being relevant | **N** | Directly verified | The starkest example in the solution of a payload that's complete for what it says, paired with a domain model that structurally cannot say anything more later |
| MachineProcessMonitoring | RecordSensorReading | Y | `MachineProcessMonitoringSensorReadingRecordedEvent` | Y — **best payload of this cluster**: `Reading (decimal), ThresholdBreached (bool), RecordedAtUtc` — the explicit boolean flag means a consumer doesn't need to independently know the threshold to react to a breach | **N** | Directly verified | |
| EnvironmentalAssetMonitoring | Create only | Y | `EnvironmentalAssetMonitoringCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Dormant `_Update` proc, unused |
| FacilityTrafficMonitoring | Create only | Y | `FacilityTrafficMonitoringCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |
| PPEBehavioralMonitoring | Create only | Y | `PPEBehavioralMonitoringCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition (5 for AutomatedIncidentDetection, 4 for MachineProcessMonitoring, 1 each for the 3
Create-only aggregates) — no naming evidence of a double-publish.

**Module-level summary:** every transition in this module publishes with a payload complete for its
own literal fields (F14), but this file has the phase's two sharpest non-F15 findings:
`EscalateToIncident`'s untyped, unverified `IncidentReferenceId` (compounds F11 at the payload
level) and `RaiseAnomalyFlag`'s "nothing can ever formally clear this" structural gap. Zero live
delivery for any of the ~15 transitions (F9) — no `AutomatedIncidentDetection*Consumer`/
`MachineProcessMonitoring*Consumer`/etc. exists anywhere, so even a valid `IncidentReferenceId`
would not currently reach IncidentManagement regardless.
