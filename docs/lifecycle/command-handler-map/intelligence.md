# Command-Handler Map — Intelligence

2 aggregates: `TrainingJob` (AITraining), `Investigation` (AIInvestigations). Inherited from
`docs/ingestion/lifecycle-coverage/intelligence-lifecycle.md` and `events/publishing-map.md`/
`consumption-map.md` (Intelligence sections) — full reads of both aggregates and their handlers were
completed by the prior discovery; not independently re-verified this phase. **This module's headline
pattern is unique in the solution**: the domain guard and the SQL/handler path both exist and are
both reachable, but the handler bypasses the aggregate entirely, so the guard never executes.

| Aggregate | Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| TrainingJob | Submit (Create) | `SubmitTrainingJobCommand` (pattern-inferred) | Handler calls `_repository.CreateAsync` directly — **never constructs a `TrainingJob` instance** | Direct Dapper call to `usp_TrainingJob_Create` (name only, body unverifiable) | **N — domain method never invoked, so `AddDomainEvent` never runs** | **N** | N/A — Intelligence has no PlantSafety-style SQL AuditLog block | Directly verified (inherited) | Domain guards (fully written, never executed) |
| TrainingJob | RegisterDataset | Domain method exists | **No handler found calling it** | N/A | N/A | N/A | N/A | Directly verified (aggregate)/pattern-inferred (no caller) | Possible additional fully-dead domain method |
| TrainingJob | Start | Domain method exists, bypassed | Handler bypasses domain (same pattern) | Direct Dapper call | **N** | **N** | N/A | Directly verified (inherited) | |
| TrainingJob | Complete | Domain method exists | Not confirmed whether any handler calls it | Unconfirmed | Unconfirmed | Unconfirmed | N/A | Pattern-inferred (inherited) | |
| TrainingJob | Cancel | `CancelTrainingJobCommand` | `CancelTrainingJobCommandHandler` — calls `_repository.CancelAsync` directly | Direct Dapper call to `usp_TrainingJob_Cancel` | **N — guard (terminal-state check, non-empty-reason) never executes** | **N** | **N** | Directly verified (inherited) | **The clearest "looks guarded, isn't" case in the solution** — a fully written, fully correct domain guard that cannot ever run because the handler never touches it |
| TrainingJob | RecordEvaluation | Command exists | `EvaluateTrainingJobCommandHandler` — calls `_repository.RecordEvaluationAsync` directly | Direct Dapper call | **N** | **N** | N/A | Directly verified (inherited) | |
| TrainingJob | Fail | Domain method exists | No handler found calling it | N/A | N/A | N/A | N/A | Pattern-inferred (inherited) | |
| TrainingJob | Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| Investigation | Open (Create) | Command exists | `RunInvestigationCommandHandler` — calls the repository's create method directly | Direct Dapper call | **N — no event of any kind confirmed even in the bypassed path** | **N** | **N** | Directly verified (inherited) | |
| Investigation | AddFinding | Domain method exists (guarded, 2 throw sites) | No handler confirmed calling it | Unconfirmed | Unconfirmed | Unconfirmed | N/A | Pattern-inferred (inherited) | |
| Investigation | SubmitHypothesis | Domain method exists (guarded) | No handler confirmed calling it | Unconfirmed | Unconfirmed | Unconfirmed | N/A | Pattern-inferred (inherited) | |
| Investigation | Cancel | Command exists | `CancelInvestigationCommandHandler` — calls `_repository.CancelAsync` directly | Direct Dapper call | **N** | **N** | **N** | Directly verified (inherited) | Same bypass pattern as `TrainingJob.Cancel` |
| Investigation | Complete | Domain method exists (guarded) | No handler confirmed calling it | Unconfirmed | Unconfirmed | Unconfirmed | N/A | Pattern-inferred (inherited) | |
| Investigation | Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Outbox reality — the one event that IS raised correctly still never delivers:**
`DataLakeTransformationCompletedIntegrationEvent`, published via `IIntelligenceEventPublisher.PublishAsync`
in `TransformDataCommandHandler` (the one handler in this module that does not bypass its domain
concept), is written to a correctly-hosted Outbox — but the only registered `IOutboxPublisher` is
`NullOutboxPublisher`, which **logs a warning and marks the row `Processed=true` without delivering
anything** (`publishing-map.md` line 113, `consumption-map.md` line 64). This is the closest thing to
an end-to-end-attempted delivery anywhere in the solution, and it actively misreports success.

**Audit column note:** Intelligence has no SQL AuditLog table/insert pattern analogous to
PlantSafety's F8 — "Audit Entry?" is **N** across the board for this module, not because of a
coverage gap in the same sense as PlantSafety's 70 uncovered aggregates, but because the mechanism
doesn't exist here at all.
