# Event Map — Auditing (AuditEngagement, AuditFinding)

See `_shared-findings.md` (F14-F17) and `command-handler-map/auditing.md` (esp. F6 —
`CloseAuditEngagementCommandHandler` accepts unchecked `ComplianceScore`). **Fully directly verified
this phase**: read of all 5 `AuditEngagement` events and all 4 `AuditFinding` events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| AuditEngagement | Create | Y | `AuditEngagementCreatedEvent` | Y — identity/actor/correlation only | **N** — bound consumer (`AuditingCreatedConsumer`) is for the unrelated `Compliance.Auditing` marker | Directly verified | |
| AuditEngagement | Schedule | Y | `AuditEngagementScheduledEvent` | Y — `LeadAuditorId (UserId), ScheduledForUtc, PlannedAtUtc` — complete, names the responsible auditor | **N** | Directly verified | |
| AuditEngagement | Start | Y | `AuditEngagementStartedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| AuditEngagement | RecordFinding | Y | `AuditEngagementFindingRecordedEvent` | Y — `AuditFindingId (Guid — untyped, unlike every other cross-reference in this file), Severity (string), RunningFindingsCount (int), RecordedAtUtc` — the running-count field is a genuinely useful, if partial, mitigation of F15 (a consumer can at least see the tally trend across successive events even without a single "old count" field on this one) | **N** | Directly verified | `AuditFindingId` is a bare `Guid`, not the strongly-typed `AuditFindingId` value object used everywhere else in this module — a minor typing inconsistency worth flagging alongside the payload review |
| AuditEngagement | Close | Y | `AuditEngagementClosedEvent` | Y (**F15 + F6 caveat**) — `ComplianceScore (decimal?), ComplianceRating (string), FindingsCount (int), ClosedAtUtc` — the payload itself is complete for what the domain method computes, but **per F6 (inherited) `ComplianceScore`/`ComplianceRating` are raw caller-supplied input with no cross-module validation against Compliance module state or a check that `FindingsCount` open findings are resolved** — the event faithfully propagates an unverified number, it doesn't introduce a new gap but does guarantee any future consumer inherits F6's problem verbatim | **N** | Directly verified | The clearest case in the solution where "payload complete" and "payload trustworthy" diverge — every field is present, but per F6 nothing guarantees `ComplianceScore` reflects reality |
| AuditEngagement | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| AuditFinding | Create | Y | `AuditFindingCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| AuditFinding | Remediate | Y | `AuditFindingRemediatedEvent` | Y — `RemediationNotes (string), RemediatedAtUtc` | **N** | Directly verified | |
| AuditFinding | TriggerCapa | Y | `AuditFindingCapaTriggeredEvent` | Y — **best payload in this module**: `AssignedToUserId (UserId), DueDateUtc (UtcTimestamp), Severity (string), TriggeredAtUtc` — everything a CAPA-tracking consumer would need | **N** | Directly verified | Command-handler-map flags this transition's name as suggesting a cross-module touch into AccidentManagement's CorrectiveAction concept — the event payload itself carries no such cross-reference (no `AccidentId`/`CorrectiveActionId` field), consistent with F6/F11's broader finding that these "implies integration" names are same-aggregate field writes, not real cross-module calls |
| AuditFinding | Verify | Y | `AuditFindingVerifiedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| AuditFinding | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Auditing (Compliance duplicate marker) | Create only | Unconfirmed | `AuditingCreatedEvent` | Unconfirmed | **N** (own consumer is a stub anyway, per F9) | Directly verified (inherited) | Distinct controller tree; read-only SQL surface |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition for both aggregates (5 for AuditEngagement, 4 for AuditFinding) — no naming
evidence of a double-publish; `AuditEngagement.cs`/`AuditFinding.cs` method bodies not re-opened for
an internal 2nd `AddDomainEvent` call this phase (`AuditEngagement.cs` was fully read in Phase 3,
inherited: 5 `AddDomainEvent` sites for 5 methods, 1:1, no duplicates).

**Module-level summary:** this remains the solution's clearest case (per `command-handler-map`'s own
framing) of a rich, well-guarded, genuinely live lifecycle whose events are payload-complete (F14)
but zero-delivered (F9) and, uniquely for `Close`, propagate an input that F6 already flagged as
unverified. `TriggerCapa`'s payload is a high-water mark for this phase's completeness review; `Close`
is the sharpest example of "complete but not trustworthy."
