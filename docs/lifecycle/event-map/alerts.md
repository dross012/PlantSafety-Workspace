# Event Map — Alerts

See `_shared-findings.md` (F14-F17) and `command-handler-map/alerts.md` for full Command/Handler
detail. Two parallel shapes (F4, inherited): shape #1 `Alerts.Alert` (live), shape #2
`Operational.Alerts.Alert` (guarded, 0 HTTP routes). **Directly re-verified this phase**: read of
all 7 shape-#1 event `record`s (`AlertTriggeredEvent`, `AlertAcknowledgedEvent`,
`AlertDispatchedEvent`, `AlertEscalatedEvent`, `AlertResolvedEvent`, `AlertClosedEvent`,
`AlertDeliveryRecordedEvent`) and shape #2's `AlertArchivedEvent`.

## Shape #1 — `Alerts.Alert` (live)

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Trigger (Create) | Y | `AlertTriggeredEvent` | Y — `EventId, AlertId, Category, Severity, SourceType, TriggeredAtUtc, Actor, CorrelationId, CausationId` | **N (EVT-A) — `AlertsCreatedConsumer` exists but unregistered, and would get the wrong CLR type from OutboxProcessor anyway** | Directly verified | Note: this is a distinct CLR type (`AlertTriggeredEvent`) from `AlertCreatedEvent` referenced in `publishing-map.md`'s summary table — naming is not perfectly 1:1 across this phase's and the ingestion discovery's documents, but both point at the same dead-end (unregistered consumer + wrong envelope type) |
| Acknowledge | Y | `AlertAcknowledgedEvent` | Y — adds `AcknowledgedAtUtc` only (no "acknowledged by which role/team" beyond generic `Actor`) | **N** | Directly verified | Thinnest payload of this aggregate's 7 events — acceptable since acknowledgment carries little state beyond "someone looked at it" |
| Dispatch | Y | `AlertDispatchedEvent` | Y — adds `DispatchedTo (string)`, `DispatchedAtUtc` | **N** | Directly verified | |
| Escalate | Y | `AlertEscalatedEvent` | Y (F15 caveat) — adds `EscalationReason (string)`, `WasAutoEscalated (bool)`, `EscalatedAtUtc` — **does not carry the severity/priority level being escalated from or to**, only the free-text reason; a consumer wanting to know "escalated from Medium to Critical" cannot from this payload | **N** | Directly verified | `WasAutoEscalated` is a nice touch (distinguishes system-driven vs. human escalation) that most other modules' events lack |
| Resolve | Y | `AlertResolvedEvent` | Y — adds `ResponseActionsTaken (string)`, `ResolvedAtUtc` | **N** | Directly verified | |
| Close | Y | `AlertClosedEvent` | Y — adds `CloseOutVerification (string)`, `ClosedAtUtc` | **N** | Directly verified | `CK_Alert_Status` SQL CHECK constraint backs the guard (inherited) — the event itself doesn't restate prior status |
| RecordDelivery | **Unconfirmed in command-handler-map; directly confirmed this phase — event class exists** (`AlertDeliveryRecordedEvent`), but whether the method that would raise it is actually called by `RecordAlertDeliveryCommandHandler` was not re-verified | `AlertDeliveryRecordedEvent` | Y — `Channel (AlertDeliveryChannel), DeliverySucceeded (bool), RecordedAtUtc` — complete if raised | **N** | Pattern-inferred (event class confirmed to exist; call-site not re-traced) | Resolves half of command-handler-map's "Unconfirmed" flag — the event type is real and well-formed; whether `RecordAlertDeliveryCommandHandler` actually triggers it is still open |
| RecordResponseAction | Unconfirmed (same caveat as above — no distinct event class found under this name; may reuse `AlertResolvedEvent`'s `ResponseActionsTaken` field instead of its own type) | Unconfirmed | N/A | **N** | Pattern-inferred | |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

## Shape #2 — `Operational.Alerts.Alert` (0 HTTP routes, F4)

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Archive | Would publish if ever reached — **confirmed `AddDomainEvent` call exists** (inherited F3) | `AlertArchivedEvent` | Y (if reached) — `EventId, AlertId, ArchivedAtUtc, Actor, CorrelationId, CausationId` — a complete, if minimal, payload | N/A — **unreachable from any HTTP route**, so delivery is moot regardless of payload quality | Directly verified (event body, this phase) | The one Alert-concept Archive that is NOT silent (unlike shape #1's aggregates elsewhere in the solution) — but it doesn't matter, because nothing can ever call it |
| Acknowledge, AddMetadata, AssignTo, Close, Escalate, Resolve, SetSlaExpiration, StartProgress (8 remaining) | Unconfirmed whether each calls `AddDomainEvent` (not individually re-opened this phase) | Presumed named per convention (`Alert*Event` classes confirmed to exist in the `Operational/Alerts/Events` folder: `AlertAssignedEvent`, `AlertInProgressEvent`, `AlertMetadataAddedEvent`, `AlertMetadataRemovedEvent`, `AlertMetadataUpdatedEvent`, `AlertEscalatedEvent`, `AlertAcknowledgedEvent`, `AlertClosedEvent`) | Pattern-inferred Y per F14 (not opened) | N/A — unreachable | Pattern-inferred | Whole cluster moot regardless of payload — 0 routes reach any of it |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** shape #1's `Alert.cs` has 8 `AddDomainEvent` sites for 8 methods
(1:1, directly re-verified this phase) — no duplicates.

**Module-level summary:** shape #1 delivers 6-8 of ~9 transitions with well-formed payloads and zero
consumers; shape #2 is entirely moot regardless of payload quality because no route reaches it. Between
the two shapes, this module has the richest Alert-specific vocabulary in the solution (12 distinct
event classes across both shapes) and zero live delivery for any of them.
