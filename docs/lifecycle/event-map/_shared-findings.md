# Phase 5 — Cross-Cutting Event-Payload Findings (referenced from every per-module event-map file)

Builds on `command-handler-map/_shared-findings.md` (F7-F13, esp. **F9** "zero events reach a working
consumer" and **F7** "Outbox is universal/aggregate-agnostic") and `rule-enforcement/_shared-findings.md`
(G1-G6, esp. **G2** "ActorId=system/CorrelationId=Guid.Empty at 58 outbox call sites"). This phase's job
is per-transition publish/no-publish plus **payload completeness** — a question the prior phases did not
ask. Findings below (F14-F17) are established by **direct read of ~60 event `record` definitions** this
phase (grep across `PlantSafety.Domain/**/Events/*.cs`, full files opened for AccidentManagement, Risk,
SafetyPolicies, Alerts, Scorecards, Auditing, SafetyCulture, Checklists, Dashboards, PermitToWork,
Scheduling, Observability, PPE) and cross-referenced against `AddDomainEvent` call sites in the
corresponding aggregate files.

## F14 — Every PlantSafety domain event follows one uniform, well-formed shape: `EventId, <AggregateId>, <new-state fields>, <TimestampUtc>, Actor, CorrelationId, CausationId`
Confirmed by direct read across 13 modules, ~60 individual event `record` definitions, no exceptions
found. This is a **positive finding, not a gap**: contrary to the task's working hypothesis (that events
would be missing obvious fields like "who approved" or "what changed"), the actor/who-did-it question is
almost always answered correctly at the event-body level — `SafetyPolicyApprovedEvent.ApprovedBy`,
`SafetyPolicyRejectedEvent.RejectedBy`+`Reason`, `RiskClosedEvent.ClosedBy`, `AccidentInvestigationClosedEvent.ClosedBy`,
`AuditEngagementScheduledEvent.LeadAuditorId`, `AuditFindingCapaTriggeredEvent.AssignedToUserId`+`DueDateUtc`+`Severity`,
`AccidentRegulatoryFilingCompletedEvent.FiledBy`+`RegulatoryAgency`+`ReferenceNumber` are all present and
correctly typed. "Obviously missing field" bugs of the kind the task hypothesized (an Approved event with
no approver, a Filed event with no agency/reference) were **not found** in any of the ~60 events read this
phase. Every event additionally carries a generic `PerformedByActor Actor` value object alongside any
specific `<Verb>By: UserId` field, so actor attribution is frequently doubly-represented in the event body
itself.

## F15 — The one payload gap that recurs on almost every non-Create event in the solution: no event carries the PRE-transition (old) value, only the post-transition (new) one
Confirmed across every "value-changing" event read this phase: `RiskScoredEvent` carries the new
`Score`/`Level` but no prior score/level; `RiskPrioritizedEvent` carries the new `Priority` only;
`AuditEngagementFindingRecordedEvent` carries `RunningFindingsCount` (a running total, which is at least a
usable proxy) but the parent `AuditEngagementClosedEvent` carries only the final `ComplianceScore`/
`ComplianceRating`, no baseline to diff against; `SafetyPolicyReviewCompletedEvent` carries the
`NewPolicyVersion` but not the version being superseded; `ChecklistItemResultRecordedEvent` carries the new
`Passed` bool with no prior-attempt value; `WorkOrderPriorityEscalatedEvent`/`AlertEscalatedEvent` carry
`EscalationReason` but not the priority/severity level being escalated *from*. **No "old status"/"old
value" field of any kind was found on any of the ~50 non-Create events read this phase** — a downstream
consumer wanting to compute "risk increased from Medium to High" or "priority escalated from P3 to P1" or
"policy version 3 superseded by version 4" cannot do so from the event payload alone; it would need to
separately query current-vs-previous aggregate state (which, per F9, is moot today since nothing consumes
these events, but would be a real gap the moment any of the 22+30 dead consumer stubs were ever wired up
and given real logic). This is the single most consistent, solution-wide payload-completeness finding of
this phase — more pervasive than any per-aggregate one-off, and it applies equally whether or not the
event in question is F8-audited or has a named consumer stub.

## F16 — G2's ActorId/CorrelationId bug means the event body's own actor/correlation fields are the only reliable copy — the Outbox envelope's copy is always wrong
Cross-referencing `rule-enforcement/_shared-findings.md` G2: all 58 `outboxWriter.Enqueue(...)` call sites
hardcode `"system"`/`Guid.Empty` for the **Outbox row's** ActorId/CorrelationId columns, regardless of what
the event's own `Actor`/`CorrelationId` properties (constructed correctly from the command envelope, per
F14) actually contain. **Practical consequence:** if a future fix ever registers a working consumer, that
consumer must read `Actor`/`CorrelationId` from the deserialized event body itself, not from the Outbox
envelope's metadata columns — the envelope's copy is unconditionally wrong (F9 already established the
envelope is also the *wrong CLR type* for the 22 Created events; this compounds that with "and its
metadata columns lie about who/what caused it" for literally every event in every module, Created and
non-Created alike). This is a **new, sharper framing** of G2 specific to this phase's payload angle, not a
restatement.

## F17 — No new duplicate-publish (2 events from 1 call) or cross-transition identical-shape event found beyond the 1 already known
Directly re-checked this phase: `Risk.cs` (8 `AddDomainEvent` sites for 8 non-Archive methods, 1:1, no
duplicates), `Alert.cs` shape #1 (8 sites for 8 methods, 1:1), `Scorecard.cs` (7 sites for 7 non-Archive
methods, 1:1), `Accident.cs` (7 sites for 7 methods, 1:1) — no aggregate re-checked this phase raises more
than one event per method call. The **one confirmed double-publish in the entire solution remains
`ChecklistCompletion.FlagFailedItem`** (`checklists.md`, inherited from Phase 3's command-handler-map read):
internally calls `RecordItemResult` (raising `ChecklistItemResultRecordedEvent`) and then itself raises a
second, distinct `ChecklistItemCriticalFailureFlaggedEvent` — 2 Outbox rows from 1 command. This is a
deliberate composition (the method's own doc comment states intent for Auditing/SafetyCulture/Alerts to
react to the 2nd event "immediately" — per F9 nothing does), not a bug, but it is the only 2-events-per-
call-site instance found either this phase or inherited. No event-loop candidate exists (nothing consumes
anything, so nothing can re-publish in reaction — same conclusion as `ingestion/events/publishing-map.md`'s
"Event loops: None found").

## Practical reading for every per-module table below
"Payload Complete?" is scored **Y** where every field a same-shaped consumer would plausibly need (aggregate
ID, actor, correlation, the specific new-state values the transition changed) is present per F14, with the
**recurring, solution-wide F15 caveat** ("no old-value/old-status field") noted once per file rather than
repeated per row. It is scored **Y (F15 caveat only)** for the majority of non-Create events. Events not
individually opened this phase (the ~30+ pattern-inferred non-Create events across EmployeeSafety/
MachineSafety/SafetyProcedures/ContractorSafety and the Create-only marker clusters) are scored **Pattern-
inferred Y** on the strength of F14's zero-exception rate across 13 directly-read modules, flagged as such,
not claimed as directly verified.
