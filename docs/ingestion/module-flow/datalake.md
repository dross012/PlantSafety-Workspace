# Module Flow — DataLake

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Scope and method

DataLake has 52 API endpoints across 10 controllers (Archival, Consumption, DataContracts,
DataQuality, Governance, Ingestion, Integration, MetadataCatalog, Processing, Schema), all
inheriting `DataLakeControllerBase` (required, throwing `TenantId`/`RequestingUserId` accessors).
Three flows were **directly traced end-to-end by reading every file in the chain**:
`IngestRawDataCommand` (Ingestion), `ProcessRawToCleansedCommand` (Processing), and
`PublishIntegrationEventCommand` (Integration) — the last chosen deliberately because its name and
route (`POST api/v1/integration/events`) most strongly suggested a genuine cross-module publish
point. All three read patterns (Archival, Consumption, DataQuality, Governance, MetadataCatalog,
Schema) are pattern-inferred from the identical `DataLakeControllerBase` inheritance and identical
handler-file shape (Dapper repository + stored procedure + structured log, no event) confirmed via
grep across the whole `DataLake.Application` tree.

## Traced flow 1 — Ingestion: Ingest Raw Data

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/ingestion` | `IngestionController.IngestRawDataAsync` → `IngestRawDataCommandHandler` | `IngestionJob` (SQL row; no in-process C# aggregate/entity class — the "aggregate" is the row itself) | Creates a new `IngestionJob` record (implicitly "Received"/Raw Zone written) | Dapper (`IIngestionRepository` → `IngestionRepository`), stored procedure `DataLake.CreateIngestionJob`, no EF/DbContext | **None.** A `RawDataIngestedEvent` class exists (`DataLake.Application.Ingestion.Events`, full canonical envelope with `AggregateId`, `TenantId`, `RawZonePartitionPath`) but is **never instantiated or published anywhere in the solution** — confirmed by grep, zero references outside its own definition file | N/A — never published | Directly verified (handler read in full; event-class grep confirms zero publish sites) |

**Dead-end — defined-but-never-raised event.** This is a distinct failure mode from PlantSafety's
"raised but nobody listens" pattern: here the event contract was designed (full audit envelope,
schema-versioned) but the command handler that should raise it does not — it only logs and
returns. Same shape confirmed for `RawToCleansedCompletedEvent` and
`CleansedToCuratedCompletedEvent` in flow 2 below.

## Traced flow 2 — Processing: Raw-to-Cleansed Promotion

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/processing/raw-to-cleansed` | `ProcessingController.ProcessRawToCleansedAsync` → `ProcessRawToCleansedCommandHandler` | `ProcessingJob` (SQL row) | Creates a `ProcessingJob` record with `SourceZone="raw"`, `TargetZone="cleansed"` | Dapper (`IProcessingRepository`), stored procedure (name not printed inline but same `CreateProcessingJobAsync` shape as Ingestion) | **None.** `RawToCleansedCompletedEvent` class exists (`DataLake.Application.Processing.Events`) but is never referenced outside its own definition file (confirmed by grep) | N/A | Directly verified (handler read in full; grep confirms zero publish sites for both `RawToCleansedCompletedEvent` and `CleansedToCuratedCompletedEvent`) |

**Dead-end**, same failure mode as flow 1.

## Traced flow 3 — Integration: Publish Integration Event (the one exception)

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `POST api/v1/integration/events` | `IntegrationController.PublishIntegrationEventAsync` → `PublishIntegrationEventCommandHandler` | An integration-event record (SQL row, via `IIntegrationRepository.RecordIntegrationEventAsync`) | Records the event; no zone/status lifecycle beyond the record itself | Dapper (`IIntegrationRepository`) | **`IntegrationEventPublishedEvent`** (`DataLake.Application.Integration.Events`) — this is the **one** place in all of DataLake.Application where an `IPublisher.Publish(...)` call for a domain/integration event actually executes | **None.** Grepped the whole `DataLake` tree for `INotificationHandler<IntegrationEventPublishedEvent>` — zero results. The event fires into MediatR's in-process notification pipeline with no registered handler subscribing to it. | Directly verified (handler read in full; grep confirms zero subscribers) |

**Dead-end, different flavor**: this is the module's only genuinely *published* event, and it
still goes nowhere — published into the void rather than never raised.

## The DataLake Outbox is never fed by any traced command

DataLake has an `OutboxMessage`/`OutboxProcessor`/`OutboxRepository`/`OutboxCleanupService` set of
classes (`DataLake.Infrastructure.Outbox`), confirmed properly tenant-scoped
(`OutboxMessage.TenantId`) by Phase 2. However, grepping all of `DataLake.Application` for any
reference to inserting an `OutboxMessage` or calling an `IOutboxRepository`/outbox-write API
returned **zero results**. None of the three traced command handlers — nor any other command
handler in the module, by the same grep — ever writes to the outbox. The `OutboxProcessor`
`BackgroundService` therefore polls a table that nothing in this module populates; it is fully
wired infrastructure with no producer.

## Summary for Phase 4

- **Every DataLake lifecycle-triggering flow examined is a dead-end**, and they dead-end in three
  distinct ways worth keeping separate for the gap analysis:
  1. Event class defined, never instantiated/published (`RawDataIngestedEvent`,
     `RawToCleansedCompletedEvent`, `CleansedToCuratedCompletedEvent`, and by the same grep
     pattern very likely every other `*CompletedEvent`/`*Event` class under
     `DataLake.Application.*.Events` — not individually re-verified per controller, flagged
     pattern-inferred).
  2. Event published via in-process MediatR, zero subscribers (`IntegrationEventPublishedEvent`).
  3. Outbox infrastructure fully built (processor, cleanup service, tenant-scoped message schema)
     but never written to by any Application-layer command — a dormant pipeline, not merely an
     unconsumed one.
- This is a stronger and more specific finding than Phase 1/2's "no evidence of cross-module
  wiring" — on direct read, DataLake's ingestion/processing/integration commands don't even
  attempt to raise the events their own contracts describe.
- Read-only endpoints (Archival eligibility, Consumption exports, DataQuality reports, Governance
  policies, MetadataCatalog search, Schema compatibility) were not individually traced — they are
  pure Dapper reads with no lifecycle or event dimension by construction (pattern-inferred from
  Phase 2's controller-base classification).
