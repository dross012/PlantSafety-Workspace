# Command-Handler Map — DataLake

No C# aggregate layer exists (structural, inherited). All "transitions" below are MediatR command
handlers writing directly via Dapper. Inherited from `docs/ingestion/lifecycle-coverage/datalake-lifecycle.md`
and `events/publishing-map.md`/`consumption-map.md` (DataLake sections); not independently
re-verified this phase.

| Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| IngestionJob.Create (Ingest Raw Data) | `IngestRawDataCommand` (pattern-inferred) | Handler behind `IngestionController.cs` | Direct Dapper call, no `.sql` file to inspect | `RawDataIngestedEvent` — **never instantiated anywhere** (confirmed, `publishing-map.md` line 98) | **N** | **N** — DataLake has no equivalent of PlantSafety's SQL AuditLog block; not applicable here | Directly verified (inherited) | Immutable once created — no Update/Delete/Cancel concept |
| ProcessingJob.Create (Raw→Cleansed, Cleansed→Curated) | Command per stage (pattern-inferred) | Handler per stage | Direct Dapper call | `RawToCleansedCompletedEvent`/`CleansedToCuratedCompletedEvent` — **never instantiated** (confirmed) | **N** | **N** | Directly verified (inherited) | No "mark failed/retried" transition found |
| IntegrationEvent.Create (Publish Integration Event) | `PublishIntegrationEventCommand` | `PublishIntegrationEventCommandHandler` | `IPublisher.Publish` (in-process MediatR, not Outbox) | `IntegrationEventPublishedEvent` — **Y, published correctly** | N/A — bypasses DataLake's own dormant Outbox entirely, goes straight to in-process MediatR | **N** | Directly verified (inherited) | **Zero `INotificationHandler<IntegrationEventPublishedEvent>` exists anywhere** (`consumption-map.md` line 55) — published into a void |
| Dataset (Archival).ArchivePartitionAsync | `ArchivePartitionCommand` (pattern-inferred) | matching handler | Direct Dapper call | **None published** | **N** | **N** | Directly verified (inherited) | No idempotency/precondition check — repeatable archival with no guard, and no event even if it were meant to notify anyone |
| Access Request (Governance).Approve | `ApproveDataAccessCommand` | `ApproveDataAccessCommandHandler` | Direct Dapper call | `DataAccessApprovedEvent` — **Y, published correctly** | N/A — same in-process MediatR path as IntegrationEvent | **N** | Directly verified (inherited) | **Zero `INotificationHandler<DataAccessApprovedEvent>` exists** (`consumption-map.md` line 56) — a 2nd "does the right thing, still dead-ends" case |
| Dataset ownership/documentation updates | `UpdateDatasetDocumentationCommand`/`UpdateDatasetOwnershipCommand` | Unconfirmed handler bodies (file existence only) | Unconfirmed | Unconfirmed | Unconfirmed | **N** | Pattern-inferred (inherited) | Still an open follow-up, not opened by this phase either |
| Any DataLake concept | Delete / hard-delete | Missing entirely | N/A | N/A | N/A | N/A | Directly verified (inherited) | Consistent with an append-only data-lake philosophy |

**Outbox infrastructure (cross-cutting, not a transition):** `OutboxProcessor`/`OutboxCleanupService`
are both hosted (`AddHostedService<>` confirmed, `publishing-map.md` line 38) and poll continuously,
but **no Application-layer command ever writes to the DataLake Outbox table** — confirmed dormant,
not broken. This is architecturally distinct from PlantSafety's Outbox (F7), which is fed correctly
but whose consumers are unregistered/wrong-typed — here the feed itself never happens.
