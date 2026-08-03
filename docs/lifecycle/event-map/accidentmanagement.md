# Event Map — AccidentManagement

See `_shared-findings.md` for F14 (uniform event shape), F15 (no old-value field), F16 (Outbox
envelope metadata always wrong), F17 (no duplicate-publish found). See
`command-handler-map/accidentmanagement.md` for the full Command/Handler/Repository detail this
file does not repeat. **Fully directly verified this phase**: read of `Accident.cs` (all 7
`AddDomainEvent` sites) + `AccidentCreatedEvent.cs` + `AccidentInvestigationEvents.cs` (all 6
non-Create event `record` definitions).

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Create (Report) | Y | `AccidentCreatedEvent` | Y — `EventId, AccidentId, CreatedAtUtc, Actor, CorrelationId, CausationId` | **N — no consumer class of any kind** (confirmed, `consumption-map.md` line 47) | Directly verified | Thinnest payload in the module (no domain fields beyond identity/actor) but that's correct for a Create event — nothing else exists yet to report |
| AssignInvestigator | Y | `AccidentInvestigatorAssignedEvent` | Y — adds `InvestigatorId (UserId)`, `AssignedAtUtc` | **N** | Directly verified | |
| RecordRootCause | Y | `AccidentRootCauseRecordedEvent` | Y — adds `RootCause (RootCauseCategory)`, `RecordedAtUtc` | **N** | Directly verified | |
| CreateCorrectiveAction | Y | `AccidentCorrectiveActionCreatedEvent` | Y — adds `CorrectiveActionId, Category, ResponsiblePersonId, DueAtUtc, CreatedAtUtc` — one of the most complete payloads in the whole solution (owner + due date + category, everything a CAPA-tracking consumer would need) | **N** | Directly verified | |
| CompleteCorrectiveAction | Y | `AccidentCorrectiveActionCompletedEvent` | Y — adds `CorrectiveActionId, CompletedAtUtc` — **F15 caveat: does not restate the CAPA's due date or whether it was completed late**, a consumer would need to re-fetch the CAPA to know if this was an on-time or overdue completion | **N** | Directly verified | Command-handler-map already flags this transition doesn't re-check parent `Accident.Status` — the event payload gap compounds that: nothing in the event tells a consumer this CAPA might be late either |
| FileRegulatoryReport | Y | `AccidentRegulatoryFilingCompletedEvent` | Y — adds `FiledBy (UserId), RegulatoryAgency (string), ReferenceNumber (string), FiledAtUtc` — fully complete, exactly the fields a compliance-tracking consumer needs | **N** | Directly verified | Best payload in the module |
| CloseInvestigation | Y | `AccidentInvestigationClosedEvent` | Y — adds `ClosedBy (UserId), ClosedAtUtc` — **F15 caveat: does not carry whether `RegulatoryFilingRequired` was satisfied**, even though the domain method's own guard (per command-handler-map, "best-guarded transition in the solution") enforces exactly that precondition before allowing Close — a consumer would have to trust the guard ran rather than see the fact asserted in the event | **N** | Directly verified | The guard is real; the event just doesn't surface what the guard checked |
| Archive (Closed→Archived) | **N — domain method exists but is never invoked from any command/handler** (command-handler-map: "the most concrete fully dead transition") | N/A | N/A — even if reached, `Accident.Archive` never calls `AddDomainEvent` (silent-archive pattern, inherited F3) | N/A | Directly verified (inherited) | Doubly dead: unreachable, and would be silent even if reached |
| Delete / Cancel | Missing entirely (no domain method) | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** `Accident.cs` has exactly 7 `AddDomainEvent` call sites for its 7
guarded methods — 1:1, no method raises more than one event.

**Module-level summary:** 6 of 8 non-Create-missing transitions publish (75% of the reachable
lifecycle, or 6/7 if Archive's unreachability is set aside); every published event's payload is
complete for its own new-state fields and actor attribution (F14) but subject to the universal F15
"no old-value" gap; zero events reach any consumer (F9/[EVT-C], `consumption-map.md` line 47 — not
even the unregistered stub the 22 Created events elsewhere in the solution got). This is the
best-designed event set in the solution and also one of the most completely undelivered.
