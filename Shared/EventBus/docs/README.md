# Shared.EventBus

**Date:** 2026-07-27

> **Status (2026-09-07): re-wired into PlantSafety.** PlantSafety previously
> depended on this module; its eventing layer was deliberately disabled after
> a 2026-08-29 build-repair pass found this module's source unrecoverable at
> the time. This module was reconstructed from scratch and fully tested
> (41/41) in isolation, and on 2026-09-07 that reconstruction was verified
> against PlantSafety's own `CrossModuleDomainEventMapper[V2]`/
> `CrossModuleEventRelay[V2]` and their previously-disabled tests: the
> `PlantSafety.Infrastructure.csproj` / `PlantSafety.Tests.csproj` Compile
> exclusions and the commented-out `AddSharedEventBus` DI registration were
> all removed, and the full solution builds and the full test suite passes
> (3861/3861, including those tests) with the two projects compiled together
> for the first time since the rebuild — confirming the reconstructed
> contracts genuinely match what PlantSafety expects, not just in isolation.

Cross-module event bus for the PlantSafety solution family. Lets one module
(currently: PlantSafety) publish versioned integration event contracts that
other modules (currently: Reporting) can subscribe to idempotently, with
bounded retry/backoff and dead-letter handling — without any module taking a
compile-time dependency on another module's domain types.

**This module does not duplicate the full design rationale.** For the
grounded domain-event inventory, the concrete PlantSafety publisher wiring,
the Reporting subscriber wiring, and the end-to-end flow walkthrough, see the
canonical doc:
`C:\Users\dross\source\PlantSafety\docs\architecture\eventbus-architecture.md`.

## Layout

| Project | Purpose |
|---|---|
| `Shared.DomainEvents.Abstractions` | Versioned, plain-DTO cross-module event contracts (no EF, no domain-type coupling). Currently: 12 DTOs across `PlantSafety.IncidentManagement`, `PlantSafety.CAPA`, `PlantSafety.AccidentManagement`, `PlantSafety.RiskManagement`. |
| `Shared.EventBus.Abstractions` | Transport-agnostic bus contracts: `EventEnvelope`, `IEventPublisher`, `IEventHandler<TPayload>`, `IEventSubscriber`. |
| `Shared.EventBus.Infrastructure` | SQL-queue-backed implementation (`dbo.EventQueue`/`ProcessedEvents`/`EventRetryState`/`EventDeadLetterQueue`) + an `InMemoryEventBus` for tests/local dev. DI: `AddSharedEventBus`, `AddInMemoryEventBus`, `AddEventHandler<TPayload, THandler>`. |
| `Shared.EventBus.Tests` | 25 xUnit v3 tests: contract, emission, consumption (idempotency/retry/dead-letter/poison-detection), all against `InMemoryEventBus`. |
| `sql\Phase0-Tables\` | Idempotent, additive-only `CREATE TABLE` scripts for the four tables above. |

## Consumers

- **PlantSafety** (publisher) — `PlantSafety.Infrastructure\Messaging\CrossModule\` maps a curated
  set of real domain events onto `Shared.DomainEvents.Abstractions` DTOs and publishes them via
  `ICrossModuleEventRelay`, flushed from `UnitOfWork.CommitAsync` *after* the SQL transaction commits.
- **Reporting** (subscriber) — `Reporting.Infrastructure\EventBus\` registers a handler per DTO
  (consumer group `"Reporting"`) that updates an in-memory "safety event activity feed" read model,
  driven by a polling `BackgroundService`.

## Configuration

Binds `Shared:EventBus` (matching the `Shared:AuditLogging`/`Shared:SafetyData`
section-naming convention):

```json
"Shared": {
  "EventBus": {
    "ConnectionString": "Server=...;Database=PlantSafety.IntegrationDB;...",
    "MaxDeliveryAttempts": 5,
    "BackoffBaseDelay": "00:00:30",
    "BackoffMaxDelay": "00:15:00",
    "PoisonMessageRepeatThreshold": 3,
    "CommandTimeoutSeconds": 30,
    "ReportingSubscriber": { "PollInterval": "00:00:15", "MaxBatchSize": 50 }
  }
}
```

## Safety invariants

- **No EF, no ORM** — Dapper + raw parameterised SQL only (single-table
  operations; no stored procedure was introduced, consistent with
  `Shared.AuditLogging.Infrastructure`'s precedent for shared cross-module
  tables).
- **Idempotent consumers** — `ProcessedEvents` keyed by `(ConsumerGroup,
  EventId)`; handlers themselves should also be internally idempotent where
  practical (Reporting's activity-feed store additionally dedupes on
  `EventId`).
- **No message broker introduced** — PlantSafety already has a real broker
  (MassTransit + RabbitMq) wired for its own intra-module Outbox publishing;
  this module intentionally does not extend that wiring cross-module (see
  `eventbus-architecture.md` §4 for the full reasoning) and instead uses the
  checklist's own explicit SQL-queue fallback.
- **Additive-only SQL** — every script under `sql\Phase0-Tables\` is
  `OBJECT_ID`-guarded; no `ALTER`/`DROP` against any existing object.
- **Dead-letter is manual re-drive** — `EventDeadLetterQueue` rows are never
  auto-deleted or auto-retried; operator/backoffice tooling to re-drive them
  is out of scope for this pass.
