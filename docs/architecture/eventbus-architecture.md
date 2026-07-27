# Cross-Module Event Bus (Shared.EventBus)

**Date:** 2026-07-27
**Author:** Eventing & Messaging Agent

This document covers the cross-module event bus built in this pass: what real
domain events exist in `PlantSafety.Domain` today, which of them now have a
versioned cross-module contract, how publishing/subscribing/retry/dead-letter
work, and the concrete PlantSafety -> Reporting flow wired up. It does not
duplicate `docs/architecture/audit-logging.md` (a different concern — the
`dbo.AuditLog` who-changed-what trail) or `plantsafety-architecture.md`'s
description of PlantSafety's own intra-module Outbox -> MassTransit/RabbitMq
pipeline, which is untouched by this work.

Canonical source of truth for the code itself:
`C:\Users\dross\source\Shared\EventBus\` (`Shared.DomainEvents.Abstractions`,
`Shared.EventBus.Abstractions`, `Shared.EventBus.Infrastructure`) and
`PlantSafety.Infrastructure\Messaging\CrossModule\` /
`Reporting.Infrastructure\EventBus\`.

## 1. Domain event inventory (what was actually found)

`PlantSafety.Domain` declares a module-specific `I<Module>DomainEvent` marker
interface (ultimately implementing the canonical
`PlantSafety.Domain.Common.Abstractions.IDomainEvent`) for essentially every
bounded context — **~140 marker interface files**. Concrete event *records*
implementing those markers exist under `**/Events/*.cs` (excluding the marker
files themselves): **~200 concrete domain event classes** across roughly 80
aggregates, and **300 confirmed `AddDomainEvent(new ...)` call sites across 81
aggregate files** (`grep -rn "AddDomainEvent(new " PlantSafety.Domain` for the
exact count at any point in time) — i.e. the overwhelming majority of these
events are genuinely raised from aggregate methods today, not dead code.

This pass's cross-module contract work is deliberately scoped to a curated
subset, not all ~200:

| Module | Domain events with a cross-module DTO |
|---|---|
| `SafetyEvents.IncidentManagement` | `IncidentManagementCreatedEvent`, `IncidentManagementUpdatedEvent`, `IncidentManagementArchivedEvent` (new in Phase 2 — there is **no** `IncidentManagementClosedEvent`; no code path closes an incident) |
| `ContinuousImprovement.CAPA` | `CorrectiveActionCreatedEvent`, `CorrectiveActionStartedEvent`, `CorrectiveActionCompletedEvent`, `CorrectiveActionClosedEvent`, `CorrectiveActionMarkedOverdueEvent` (new in Phase 2) |
| `AccidentManagement` | `AccidentCreatedEvent`, `AccidentArchivedEvent` (pre-existing) |
| `RiskManagement` | `RiskCreatedEvent`, `RiskArchivedEvent` (pre-existing) |

**12 domain events, 4 modules.** This is the checklist's own explicit focus
(IncidentManagement + CAPA from Phase 2, plus the two named pre-existing
examples, `AccidentArchivedEvent`/`RiskArchivedEvent`) and matches the
"Safety Events"/"Compliance" rows of the integration event matrix in
`project_plantsafety_integration_events.md`. Building versioned DTOs,
mappers, and consumer-side handlers for all ~200 domain events in one pass
would produce an unreviewable diff and duplicate work that belongs to each
event's *own* future cross-module need — **assumption, flagged per the
agent's "prefer the more consumer-hardened option, note the assumption"
rule**: extending coverage is additive (one new DTO + one new
`CrossModuleDomainEventMapper` case arm per event) and is the natural next
step, not a redesign.

Other domain events with an `Archived`/`Closed`-style terminal transition
that were checked and found to have **no** dedicated Shared DTO in this pass
(candidates for a future pass, not fabricated here): `PermitWorkflowClosedEvent`,
`PermitWorkflowArchivedEvent`, `PermitTemplateArchivedEvent`,
`DashboardArchivedEvent`, `RiskClosedEvent`, `AccidentInvestigationClosedEvent`,
`WorkOrderPriorityClosedEvent`. No `Hazard`-specific domain event class exists
anywhere in `PlantSafety.Domain` today — the closest matches are
`HazardCommunicationTrainingCreatedEvent`,
`HazardCommunicationRightToKnowCreatedEvent`, and
`HazardIsolationSequencingCreatedEvent` (each a `*CreatedEvent` on an
unrelated aggregate whose name happens to contain "Hazard"), none of which
were in scope for this pass.

## 2. Contracts (`Shared.DomainEvents.Abstractions`)

Plain, immutable `record` DTOs — no EF, no ORM coupling, no reference to
PlantSafety.Domain value objects (`StrongId`/`UtcTimestamp`/`PerformedByActor`
etc. are unwrapped to `Guid`/`DateTime`/`string` at the mapping boundary), so
a consuming module never takes a compile-time dependency on PlantSafety's
internal domain types. Every DTO implements `IIntegrationEventContract`
(`static abstract string EventType`, `static abstract int SchemaVersion`).

Naming follows `project_plantsafety_patterns.md` §4/§14 exactly:
`<Module>.<EventName>` (e.g. `Incidents.IncidentManagementCreated`,
`CAPA.CorrectiveActionClosed`, `Accidents.AccidentArchived`,
`Risk.RiskArchived`) — past-tense facts, no verbs-as-intentions, version
tracked in `SchemaVersion`/`EventEnvelope.SchemaVersion`, never in the type
name.

## 3. Event bus abstraction (`Shared.EventBus.Abstractions`)

- **`EventEnvelope`** — canonical envelope: `EventId`, `EventType`,
  `SchemaVersion`, `PayloadType`, `TenantId`, `CorrelationId`, `CausationId`,
  `OccurredAtUtc`, `SourceModule`, `Payload` (JSON), `EnqueuedAtUtc`. Mirrors
  the field set of PlantSafety's own intra-module canonical envelope
  (`IntegrationEventEnvelope` / patterns.md §4) but is transport-agnostic.
- **`IEventPublisher`** — `PublishAsync(EventEnvelope, ...)` plus a typed
  convenience overload. Publishing durably enqueues; it does not guarantee
  synchronous delivery.
- **`IEventHandler<TPayload>`** — one idempotent side effect for one payload
  type within one named `ConsumerGroup` (module).
- **`IEventSubscriber`** — pulls and dispatches due events for one consumer
  group (`ProcessDueEventsAsync`).

## 4. Infrastructure (`Shared.EventBus.Infrastructure`)

### Transport choice

No message broker is configured anywhere in this solution **for cross-module
use**. `PlantSafety.Infrastructure`'s own `ServiceBusEventPublisher` +
`InfrastructureServiceRegistration.cs` *does* wire a real broker
(`MassTransit.UsingRabbitMq`, config keys `RabbitMq:Host`/`Username`/
`Password` in `PlantSafety.Api/Appsettings.json`) — but that pipeline is
scoped to PlantSafety-internal outbox publishing (`IIntegrationEventPublisher`,
one message type per `Publish<T>` call, no envelope/consumer-group routing),
and Reporting has zero messaging infrastructure today. Reusing RabbitMq
cross-module would mean adding a new runtime dependency to Reporting and
redesigning MassTransit's routing for envelope-based, multi-consumer-group
dispatch — a materially larger, riskier change than this checklist scope.
Per checklist step 21's own explicit fallback ("a SQL queue table ... or an
in-memory implementation for tests"), `Shared.EventBus` uses a **SQL queue
table** (`dbo.EventQueue` on `PlantSafety.IntegrationDB` — one of the 8
existing module databases per `project_plantsafety_integration_events.md`'s
inventory and already used for `ArchivalOptions`/`ConfigurationOptions` in
`PlantSafety.Infrastructure.Operations`) as the durable cross-module
transport, and an `InMemoryEventBus` for tests/local dev.

### Tables (`Shared\EventBus\sql\Phase0-Tables\`)

| Table | Purpose |
|---|---|
| `dbo.EventQueue` | Durable envelope queue. One row per published event. |
| `dbo.ProcessedEvents` | Idempotency ledger, keyed `(ConsumerGroup, EventId)` — patterns.md §4/§12. |
| `dbo.EventRetryState` | Bounded-retry-with-backoff bookkeeping, keyed `(ConsumerGroup, EventId)`: `AttemptCount`, `NextAttemptAtUtc`, `LastError`. |
| `dbo.EventDeadLetterQueue` | Poison/unresolvable messages, full diagnostic context, never auto-deleted. |

All four scripts are additive-only, idempotent (`OBJECT_ID` guarded), no
`ALTER`/`DROP`.

### Publisher — `SqlEventPublisher`

`INSERT INTO dbo.EventQueue` via Dapper (raw parameterised SQL for a
single-table insert, following `Shared.AuditLogging.Infrastructure`'s own
precedent for cross-module shared tables rather than a stored procedure).

### Subscriber/dispatcher — `SqlEventSubscriber`

Implements every rule in patterns.md §12 ("Consumer Hardening Rules"):

1. **Idempotency** — `ProcessedEvents` keyed by `(ConsumerGroup, EventId)`,
   checked before dispatch.
2. **Retry with exponential backoff** — `EventBusOptions.ComputeBackoff`
   (`BackoffBaseDelay * 2^(attempt-1)`, capped at `BackoffMaxDelay`); due
   events are those with no retry-state row, or whose `NextAttemptAtUtc` has
   elapsed.
3. **Dead-letter queue** — once `AttemptCount >= MaxDeliveryAttempts`
   (default 5), the event moves to `EventDeadLetterQueue` with full context
   (`FailureReason`, `AttemptCount`, payload) and its retry-state row is
   removed.
4. **Poison-message detection** — if the failure reason is byte-identical
   across consecutive attempts *and* `AttemptCount >= PoisonMessageRepeatThreshold`
   (default 3) at the moment of dead-lettering, `IsPoisonMessage = 1`.
5. **Structured logging** — every outcome (`Success`/`Failed`/`DeadLettered`)
   is logged with `EventId`, `CorrelationId`, `ConsumerGroup`, `EventType`.

An event with no registered handler for a given consumer group is marked
processed as "not applicable" rather than retried forever — a consumer group
only ever sees the payload types it explicitly registered handlers for.

### In-memory bus (`InMemoryEventBus`) — tests / local dev

Same publish/subscribe/idempotency/dead-letter *semantics*, backed by
`ConcurrentQueue`/`ConcurrentDictionary` instead of SQL. Not durable, not for
production. Exercised by all 25 tests in `Shared.EventBus.Tests`.

## 5. PlantSafety publisher wiring

`PlantSafety.Infrastructure\Messaging\CrossModule\`:

- **`CrossModuleDomainEventMapper.TryMap`** — pattern-matches the 12 curated
  domain event record types onto their `Shared.DomainEvents.Abstractions` DTO
  + `EventEnvelope`. Returns `false` (no-op) for the other ~190 domain event
  types, which is the expected common case, not an error.
- **`CrossModuleEventRelay`** (`ICrossModuleEventRelay`) — buffers mapped
  envelopes in-memory, mirroring `IOutboxWriter`'s own buffer/flush/clear
  shape, and publishes them via `IEventPublisher` on `FlushAsync`.
- **`UnitOfWork.EnqueueEvent`** now calls **both** `IOutboxWriter.Enqueue`
  (existing, unchanged — intra-module Outbox -> RabbitMq) **and**
  `ICrossModuleEventRelay.Enqueue` (new). **`UnitOfWork.CommitAsync`** flushes
  the outbox and commits the SQL transaction exactly as before, then — only
  *after* `Transaction.Commit()` returns — flushes the cross-module relay.
  This is genuine Publish-After-Commit for the Shared.EventBus sink (whereas
  the pre-existing `OutboxWriter.FlushAsync` call happens *before*
  `Transaction.Commit()`, on its own separate connection — a pre-existing
  characteristic of that pipeline, unchanged and out of this pass's scope).
  `RollbackAsync` calls `ICrossModuleEventRelay.Clear()` alongside the
  existing `Transaction.Rollback()`.
- Deliberately **separate from `Shared.AuditLogging`** — audit trail
  (who-changed-what) and cross-module integration events (what-happened,
  for other modules to react to) are different concerns with different
  consumers, retry semantics, and lifecycles; no code path merges them.

Registered in `PlantSafety.Infrastructure\DependencyInjection\
InfrastructureServiceRegistration.cs` via `services.AddSharedEventBus(configuration)`
+ `services.AddScoped<ICrossModuleEventRelay, CrossModuleEventRelay>()`,
binding the `Shared:EventBus` section of `PlantSafety.Api/Appsettings.json`
(points at `PlantSafety.IntegrationDB`).

## 6. Reporting subscriber wiring

Reporting has no SQL migrations/tables of its own today — it is a pure
read/query layer over `Shared.AuditLogging`/`Shared.SafetyData` at
report-generation time (`Reporting.Infrastructure\DataSources\*`). Rather
than inventing a new, unreviewed durable schema for this checklist item, the
concrete "update read models on relevant events" deliverable is:

- **`ISafetyEventActivityFeedStore`** (`InMemorySafetyEventActivityFeedStore`)
  — a thread-safe, bounded (500 entries/event type), `EventId`-idempotent,
  in-process read model: "recent cross-module safety events," queryable by
  tenant and event-type prefix. Promoting this to a durable SQL-backed table
  (once Reporting has its own schema) is the natural next step, out of scope
  here.
- **12 `IEventHandler<TPayload>` implementations** (`Reporting.Infrastructure\
  EventBus\Handlers\`), one per curated DTO, `ConsumerGroup = "Reporting"`,
  each recording one `SafetyEventActivityEntry` into the store.
- **`EventBusSubscriberHostedService`** — a `BackgroundService` that polls
  `IEventSubscriberFactory.Create("Reporting").ProcessDueEventsAsync(...)`
  every `Shared:EventBus:ReportingSubscriber:PollInterval` (default 15s,
  batch size 50). A single failed pass is caught/logged and never crashes the
  poller.
- Registered via `AddReportingEventBusSubscribers(configuration)`, called
  from `InfrastructureServiceCollectionExtensions.AddReportingInfrastructure`
  (the same place `AddSharedAuditLogging`/`AddSharedSafetyData` are wired).

### Modules checked and skipped for step 23

Per the checklist's own instruction ("if a target module doesn't exist,
skip it and say so"): `C:\Users\dross\source\` contains exactly four
module repos — `PlantSafety`, `Reporting`, `Shared\AuditLogging`,
`Shared\Authorization` (now `Shared\EventBus`, `Shared\SafetyData` also
exist as prior Shared extractions). **No Scheduling, Maintenance, or
Compliance module/project exists anywhere on disk** (`PlantSafety.Domain`
has `Scheduling.*` and `Compliance.*` *bounded contexts inside* the single
PlantSafety solution/deployable, not separate modules/repos with their own
API/Infrastructure to hook a subscriber into) — so no separate subscriber
project was created for them, per the "don't fabricate a subscriber for a
module that doesn't exist" constraint.

## 7. Cross-module flow (concrete example)

1. A command handler mutates the `IncidentManagement` aggregate; the
   aggregate raises `IncidentManagementCreatedEvent` (`AddDomainEvent`).
2. The Unit of Work calls `EnqueueEvent(...)`: the existing `OutboxWriter`
   buffers it for `OutboxMessages` (unchanged), and the new
   `CrossModuleEventRelay` maps it to `IncidentManagementCreatedIntegrationEvent`
   and buffers an `EventEnvelope`.
3. `CommitAsync()`: outbox flush + SQL commit (unchanged) — then, only after
   the commit returns, the relay publishes the envelope via `SqlEventPublisher`
   -> `INSERT INTO PlantSafety.IntegrationDB.dbo.EventQueue`.
4. `Reporting.Api`'s `EventBusSubscriberHostedService` polls, finds the row
   due, resolves `IncidentManagementCreatedActivityFeedHandler` from DI,
   deserializes the payload, records it into
   `InMemorySafetyEventActivityFeedStore`, marks `(Reporting, EventId)`
   processed.
5. Any transient failure at step 4 leaves a `dbo.EventRetryState` row with a
   computed backoff; repeated failure past `MaxDeliveryAttempts` moves the
   event to `dbo.EventDeadLetterQueue` for operator review.

## 8. Testing

`Shared.EventBus.Tests` (25 tests, xUnit v3, all passing):

- **Contract tests** (`Contracts\IntegrationEventContractTests.cs`) — every
  curated DTO's `EventType` matches the `<Module>.<EventName>` convention,
  `SchemaVersion >= 1`, and payloads round-trip losslessly through
  `EventEnvelope` JSON serialization.
- **Emission tests** (`Publishing\InMemoryEventPublisherTests.cs`) — publish
  records to `PublishedEvents`, canonical `EventType` construction, publish
  order preserved.
- **Consumption tests** (`Dispatch\InMemoryEventConsumptionTests.cs`) —
  handler invocation for matching payload type, idempotency (second pass does
  not re-invoke), unrelated-consumer-group skip, and the full retry -> dead-letter
  -> poison-flag pipeline.

`SqlEventPublisher`/`SqlEventSubscriber` themselves require a live SQL Server
instance and are exercised by `PlantSafety.IntegrationTests`/
`DataIntegrationTests` conventions, not unit tests here (consistent with how
`AuditLogDapperRepository` is treated per `audit-logging.md`).

## 9. Files touched

- `Shared\EventBus\` — new solution: `Shared.DomainEvents.Abstractions`,
  `Shared.EventBus.Abstractions`, `Shared.EventBus.Infrastructure`,
  `Shared.EventBus.Tests`, `sql\Phase0-Tables\*.sql`.
- `PlantSafety.Infrastructure\Messaging\CrossModule\` — new (mapper, relay).
- `PlantSafety.Infrastructure\Persistence\UnitOfWork\UnitOfWork.cs` — extended
  (optional `ICrossModuleEventRelay` dependency; `IUnitOfWork.cs` unchanged).
- `PlantSafety.Infrastructure\DependencyInjection\InfrastructureServiceRegistration.cs`,
  `PlantSafety.Infrastructure.csproj`, `PlantSafety.Api\Appsettings.json` —
  new registrations/config.
- `Reporting.Infrastructure\EventBus\` — new (read model, handlers, hosted
  service, DI extensions).
- `Reporting.Infrastructure\DependencyInjection\InfrastructureServiceCollectionExtensions.cs`,
  `Reporting.Infrastructure.csproj`, `Reporting.Api\appsettings.json` — new
  registrations/config.
