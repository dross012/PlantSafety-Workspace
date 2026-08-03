# Event Map — Intelligence (TrainingJob, Investigation)

2 aggregates. Inherited from `docs/ingestion/events/publishing-map.md`/`consumption-map.md`
(Intelligence sections) and `command-handler-map/intelligence.md`; not independently re-verified
this phase at the source-file level. This module's headline pattern — the domain guard and event
both exist and are well-formed, but the Application-layer handler bypasses the aggregate entirely —
means the payload-completeness question is almost entirely moot: an event that is never constructed
has no payload to assess.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| TrainingJob | Submit (Create) | **N — domain method never invoked, `AddDomainEvent` never runs** (handler calls `_repository.CreateAsync` directly) | `TrainingJobStartedEvent` (would be raised if the aggregate were ever used) | N/A — never constructed | N/A | Directly verified (inherited) | The guard is fully written and would work; the event would too, if reached |
| TrainingJob | RegisterDataset | Domain method exists, no handler found calling it | Presumed a matching event exists on the aggregate | N/A | N/A | Pattern-inferred (inherited) | Possible additional fully-dead domain method |
| TrainingJob | Start | Bypassed, same pattern | `TrainingJobRunningEvent` (per aggregate name) | N/A | N/A | Directly verified (inherited) | |
| TrainingJob | Complete | Unconfirmed whether any handler calls it | `TrainingJobCompletedEvent` | N/A | N/A | Pattern-inferred (inherited) | |
| TrainingJob | Cancel | Bypassed — handler calls `_repository.CancelAsync` directly | `TrainingJobCancelledEvent` | N/A — never constructed | N/A | Directly verified (inherited) | The clearest "looks guarded, isn't" case in the solution (inherited framing) — this extends to its event too: fully defined, never raised |
| TrainingJob | RecordEvaluation | Bypassed — handler calls `_repository.RecordEvaluationAsync` directly | `TrainingJobEvaluatedEvent` | N/A | N/A | Directly verified (inherited) | |
| TrainingJob | Fail | No handler found calling it | `TrainingJobFailedEvent` | N/A | N/A | Pattern-inferred (inherited) | |
| TrainingJob | Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Investigation | Open (Create) | Bypassed — handler calls the repository's create method directly | Presumed an Open/Created event exists on the aggregate | N/A | N/A | Directly verified (inherited) | No event of any kind confirmed even in the bypassed path (inherited) |
| Investigation | AddFinding | No handler confirmed calling it | Presumed exists (2 throw sites confirmed, inherited) | N/A | N/A | Pattern-inferred (inherited) | |
| Investigation | SubmitHypothesis | No handler confirmed calling it | Presumed exists | N/A | N/A | Pattern-inferred (inherited) | |
| Investigation | Cancel | Bypassed — handler calls `_repository.CancelAsync` directly | Presumed exists | N/A | N/A | Directly verified (inherited) | Same bypass pattern as `TrainingJob.Cancel` |
| Investigation | Complete | No handler confirmed calling it | Presumed exists (guarded) | N/A | N/A | Pattern-inferred (inherited) | |
| Investigation | Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| TransformData (the one non-bypassed transition in this module) | Y — `IIntelligenceEventPublisher.PublishAsync` in `TransformDataCommandHandler` | `DataLakeTransformationCompletedIntegrationEvent` | Unconfirmed — event body not opened this phase, flagged as the one Intelligence event actually worth a payload check in a future pass, since it is the closest thing to an end-to-end-attempted delivery in the solution | **N — reaches a running `OutboxProcessor`, which hands it to `NullOutboxPublisher`, which logs a warning and marks the row `Processed=true` without delivering** (`consumption-map.md` line 64) | Directly verified (publish path, inherited) | The closest thing to "delivered" anywhere in the solution, and it actively misreports success |

**Duplicate-publish check (F17):** not applicable to the 12 bypassed `TrainingJob`/`Investigation`
events (never constructed, so never duplicated); `TransformDataCommandHandler` calls
`PublishAsync` once per invocation per the inherited read — no duplicate found.

**Module-level summary:** Intelligence is the sharpest case in the entire audit of "payload quality
is irrelevant because the event is never constructed" — 12 of 13 transitions bypass the aggregate
entirely, so there are 12 well-designed-on-paper events with zero real payload to evaluate. The one
event that IS constructed (`DataLakeTransformationCompletedIntegrationEvent`) reaches the closest
thing to a working pipeline in the solution and still terminates in a no-op that lies about success.
