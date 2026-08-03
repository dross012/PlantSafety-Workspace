# Event Map — PermitToWork (PermitWorkflow, PermitTemplate, PermitExpiration, PermitScheduling)

See `_shared-findings.md` (F14-F17) and `command-handler-map/permittowork.md`. **Fully directly
verified this phase**: read of all 9 `PermitWorkflow` event `record`s, all 5 `PermitTemplate`
events, and both Create-only events (`PermitExpirationCreatedEvent`, `PermitSchedulingCreatedEvent`).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| PermitWorkflow | Create | Pattern-inferred Y | `PermitWorkflowCreatedEvent` | Y — identity/actor/correlation only | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | AssessHazards | Pattern-inferred Y | `PermitWorkflowHazardsAssessedEvent` | Y — `RequiredControls (string), RiskScore (int), AssessedAtUtc` — carries both the computed score and the free-text controls list, a genuinely complete payload | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | ImplementControls | Pattern-inferred Y | `PermitWorkflowControlsImplementedEvent` | Y — `ControlsConfirmedBy (string), ImplementedAtUtc` | **N** | Pattern-inferred (event body directly read) | `ControlsConfirmedBy` is a raw `string`, not a `UserId` — inconsistent typing vs. every other "who did it" field in this module (all others are typed `UserId`) |
| PermitWorkflow | Approve | Pattern-inferred Y | `PermitWorkflowApprovedEvent` | Y — `ApprovedBy (UserId), ApprovedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | Issue | Pattern-inferred Y | `PermitWorkflowIssuedEvent` | Y — `IssuerUserId (UserId), IssuedAtUtc, ExpiresAtUtc (UtcTimestamp?)` — carries the expiry forward, useful for a downstream expiry-tracking consumer | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | RecordClosureSignature (Close) | Pattern-inferred Y | `PermitWorkflowClosedEvent` | Y — `ClosureSignedBy (UserId), ClosureNotes (string?), ClosedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | Reject | Pattern-inferred Y | `PermitWorkflowRejectedEvent` | Y — `RejectedBy (UserId), Reason (string), RejectedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | Expire | Pattern-inferred Y | `PermitWorkflowExpiredEvent` | Y — identity/timestamp/actor only (no reference back to the `ExpiresAtUtc` that was set at Issue, though that's implicit in "it's now expired") | **N** | Pattern-inferred (event body directly read) | |
| PermitWorkflow | Archive | **Unconfirmed whether the domain method calls `AddDomainEvent`** — but the event class itself **is confirmed to exist** this phase (`PermitWorkflowArchivedEvent.cs`), which is new information beyond command-handler-map's "top remaining open item" framing | `PermitWorkflowArchivedEvent` | Y (if raised) — `EventId, PermitWorkflowId, ArchivedAtUtc, Actor, CorrelationId, CausationId` | **N** | Directly verified (event class existence); call-site not traced this phase | The event-class's mere existence is suggestive (all 3 previously-confirmed non-silent Archives — `PermitTemplate`, `Dashboard`, `Operational.Alerts.Alert` — also have a dedicated `*ArchivedEvent` file; `SafetyPolicy`, by contrast, has none). This raises the confirmed non-silent-Archive count to a likely 4th, pending a direct read of the `PermitWorkflow.Archive` method body to confirm the call site itself |
| PermitWorkflow | Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | `Reject` covers the negative path informally |
| PermitTemplate | Create | Pattern-inferred Y | `PermitTemplateCreatedEvent` | Y — identity/actor/correlation only | **N** | Pattern-inferred (event body directly read) | |
| PermitTemplate | Publish | Pattern-inferred Y | `PermitTemplatePublishedEvent` | Y — identity/timestamp/actor only | **N** | Pattern-inferred (event body directly read) | |
| PermitTemplate | Deprecate | Pattern-inferred Y | `PermitTemplateDeprecatedEvent` | Y — `Reason (string), DeprecatedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| PermitTemplate | Retire | Pattern-inferred Y | `PermitTemplateRetiredEvent` | Y — identity/timestamp/actor only (no reason field, unlike `Deprecate`'s sibling event — an inconsistency within the same aggregate's own lifecycle) | **N** | Pattern-inferred (event body directly read) | |
| PermitTemplate | Archive | **Y — confirmed raised** (F3, inherited + directly re-read this phase) | `PermitTemplateArchivedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | One of the (now likely 4, pending PermitWorkflow confirmation) non-silent Archives in the solution |
| PermitTemplate | Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| PermitExpiration | Create only | Pattern-inferred Y | `PermitExpirationCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified (event body, this phase) | `_Update` proc exists as dormant capacity (inherited) — no domain method exercises it |
| PermitScheduling | Create only | Pattern-inferred Y | `PermitSchedulingCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified (event body, this phase) | Same dormant-`_Update` note |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition across all 4 aggregates (9 for PermitWorkflow, 5 for PermitTemplate, 1 each for the
2 Create-only aggregates) — no naming evidence of a double-publish; aggregate method bodies not
individually re-opened this phase to rule out an internal 2nd `AddDomainEvent` call.

**Module-level summary:** this is one of the best-instrumented feature areas in the solution —
`PermitWorkflowHazardsAssessedEvent`'s `RiskScore`+`RequiredControls` pair and
`PermitWorkflowIssuedEvent`'s forwarded `ExpiresAtUtc` are genuinely useful payloads a real
consumer could act on without a callback query. The one payload inconsistency found is
`ControlsConfirmedBy` being a raw `string` where every sibling "who did it" field elsewhere in the
same aggregate is a typed `UserId`. Zero live delivery for any of the ~19 transitions in this file
(F9) — no `Permit*Consumer` class exists anywhere; the solution's only "Permits"-named consumer
(`PermitsCreatedConsumer`) is bound to the unrelated `Compliance.Permits` marker aggregate.
