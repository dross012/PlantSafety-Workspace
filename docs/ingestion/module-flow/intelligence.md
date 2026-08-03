# Module Flow — Intelligence

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Scope and method

Intelligence has 37 API endpoints across 6 controllers (AIInvestigations, AITraining, DataLake,
FeatureEngineering, ModelRegistry, PredictiveSafety), all inheriting `IntelligenceControllerBase`
(optional `Guid? TenantId`, required `RequestingUserId`). Three flows were **directly traced
end-to-end**: `RunInvestigationCommand` (AIInvestigations), `IngestDataCommand` and
`TransformDataCommand` (Intelligence's own `DataLake` subsystem — note this is Intelligence's
*own* ingestion surface, distinct from the top-level DataLake module). The `TransformDataCommand`
flow was chosen specifically because it is the **only** Intelligence command confirmed to publish
an event, making it the most important trace in this module.

## Traced flow 1 — AIInvestigations: Run Investigation

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/aiinvestigations` | `AIInvestigationsController.RunInvestigationAsync` → `RunInvestigationCommandHandler` | Investigation record (SQL, `intelligence.usp_Investigation_Create`) | Creates investigation in "Pending" status | Dapper (`IInvestigationRepository` → `InvestigationRepository`), stored procedures under `Sp.Create`/`Sp.Cancel`/`Sp.AddHypothesis` etc. — no EF/DbContext | **None.** No `IPublisher`/`IIntelligenceEventPublisher` reference anywhere in `RunInvestigationCommandHandler` | N/A | Directly verified (full read) |

## Traced flow 2 — Intelligence.DataLake: Ingest Data

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/datalake/ingest` | `DataLakeController.IngestDataAsync` → `IngestDataCommandHandler` | `DataLakeRecord` (SQL row) | Creates ingestion record | Dapper (`IDataLakeRepository`) — SQL-backed, not EF | **None.** `DataLakeIngestionCompletedIntegrationEvent` class exists (`Intelligence.Application.DataLake.Events`) but `IngestDataCommandHandler` never references `IPublisher` or `IIntelligenceEventPublisher` — confirmed by full read | N/A | Directly verified |

## Traced flow 3 — Intelligence.DataLake: Transform Data (the one event Intelligence actually publishes)

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/datalake/transform` | `DataLakeController.TransformDataAsync` → `TransformDataCommandHandler` | No local aggregate — delegates to `IPipelineOrchestrationService.ExecuteTransformationAsync` | Pipeline execution outcome (`TargetZone`, `OutputPath`, `RecordsWritten`) is folded into the event payload; no local status entity | Not a repository call at all — an orchestration service call. No DB write observed in this handler. | **`DataLakeTransformationCompletedIntegrationEvent`**, published via `IIntelligenceEventPublisher.PublishAsync` | See below — **dead-ends at a no-op publisher, confirmed by direct read of the DI registration** | Directly verified (handler, publisher implementation, and DI registration all read in full) |

### The full trace of where `TransformDataCommand`'s event actually goes

1. `IIntelligenceEventPublisher` is implemented by
   `Intelligence.Infrastructure.Messaging.IntelligenceEventPublisher`, which serializes the event
   to JSON and calls `OutboxRepository.InsertStandaloneAsync` — i.e. it writes to Intelligence's
   own Outbox (`Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage`), **not** a
   cross-module call. This part works as designed (durable write-then-relay).
2. Intelligence's `OutboxProcessor` (`BackgroundService`) polls this table and calls
   `IOutboxPublisher.PublishAsync(message, ct)` per unprocessed row.
3. **`IOutboxPublisher` has exactly one implementation registered anywhere in the solution:
   `NullOutboxPublisher`** (`Intelligence.Infrastructure.Persistence.Outbox.NullOutboxPublisher`).
   Its own header comment states it is *"the default registration when no real event bus
   implementation has been registered"* and instructs whoever wires production to
   `services.AddSingleton<IOutboxPublisher, YourServiceBusPublisher>()` — **this registration was
   never added.** Confirmed by grepping the entire `Intelligence` tree for `IOutboxPublisher`: the
   only concrete class is `NullOutboxPublisher`, and `Intelligence.Infrastructure.DependencyInjection.cs`
   registers no alternative.
4. `NullOutboxPublisher.PublishAsync` logs a warning (*"NullPublisherRegistered — no event bus
   implementation has been wired up"*) and returns `Task.CompletedTask`. The `OutboxProcessor` then
   marks the message `Processed = true` — **the event is durably recorded as "delivered" while
   never having gone anywhere.**

**This is the single most concrete "dead-end" found in the entire discovery**: not merely
"nothing consumes this event" but a literal `Null`-pattern implementation standing in for
production wiring that was never completed, silently marking undelivered messages as processed.

## AITraining — confirmed same no-publish pattern

`StartTrainingJobCommandHandler`, `CancelTrainingJobCommandHandler`, and
`EvaluateTrainingJobCommandHandler` were confirmed (via grep for `IPublisher`/`_eventPublisher`
across `Intelligence.Application.AITraining`) to **never reference a publisher at all**, despite
`TrainingJobStartedIntegrationEvent` and `TrainingJobCancelledIntegrationEvent` classes existing
under `AITraining\Events`. Same dead-end shape as flows 1 and 2 (event contract defined, never
raised). Not individually read line-by-line this phase — pattern-inferred from the grep result.

## Summary for Phase 4

- Intelligence's 37 endpoints show the **same two dead-end shapes as DataLake** (event class never
  raised — AIInvestigations, DataLake.Ingest, AITraining) **plus a third, worse shape unique to
  Intelligence**: the one flow that *does* correctly publish through its Outbox
  (`TransformDataCommand`) is silently swallowed by a `NullOutboxPublisher` that marks messages
  processed without delivering them. Phase 4 should treat this as higher severity than a simple
  "unconsumed event" — it is an **active, working pipeline whose terminal step was deliberately
  stubbed and never replaced**, and the "Processed = true" bookkeeping actively hides that fact
  from monitoring.
- No Intelligence flow examined reaches a real downstream module or a real external system.
