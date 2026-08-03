# Event Map — DataLake

No C# aggregate layer (structural, inherited) — "transitions" are MediatR command handlers writing
directly via Dapper. Inherited from `docs/ingestion/events/publishing-map.md`/`consumption-map.md`
(DataLake sections) and `command-handler-map/datalake.md`; not independently re-verified this phase
at the source-file level, but re-organized here per-transition per this phase's schema.

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| IngestionJob.Create (Ingest Raw Data) | **N — never instantiated anywhere** (confirmed, `publishing-map.md` line 98) | `RawDataIngestedEvent` (definition only) | N/A — no instance ever constructed, so there is no payload to assess | N/A | Directly verified (inherited) | The DataLake equivalent of the emerging-createonly cluster's "defined but dead" pattern |
| ProcessingJob.Create (Raw→Cleansed, Cleansed→Curated) | **N — never instantiated** | `RawToCleansedCompletedEvent`/`CleansedToCuratedCompletedEvent` (definition only) | N/A | N/A | Directly verified (inherited) | No "mark failed/retried" transition found either |
| IntegrationEvent.Create (Publish Integration Event) | **Y — published correctly** via `IPublisher.Publish` (in-process MediatR) | `IntegrationEventPublishedEvent` | Unconfirmed — event body not opened this phase; this is the one DataLake event that is genuinely constructed and worth a payload check in a future pass | **N — zero `INotificationHandler<IntegrationEventPublishedEvent>` anywhere** (`consumption-map.md` line 55) | Directly verified (publish path, inherited); payload not re-opened | Bypasses DataLake's own dormant Outbox entirely — published into a void regardless of payload quality |
| Dataset (Archival).ArchivePartitionAsync | **N — none published** | N/A | N/A | N/A | Directly verified (inherited) | No idempotency/precondition check either — repeatable archival with no guard and no event |
| Access Request (Governance).Approve | **Y — published correctly** via `IPublisher.Publish` | `DataAccessApprovedEvent` | Unconfirmed — event body not opened this phase | **N — zero `INotificationHandler<DataAccessApprovedEvent>`** (`consumption-map.md` line 56) | Directly verified (publish path, inherited); payload not re-opened | A 2nd "does the right thing, still dead-ends" case — an access-approval event is exactly the kind of thing a real audit/compliance consumer would want, and none exists |
| Dataset ownership/documentation updates | Unconfirmed | Unconfirmed | Unconfirmed | **N** | Pattern-inferred (inherited) | Open follow-up, not opened by this phase either |
| Any DataLake concept | Delete / hard-delete | Missing entirely | N/A | N/A | Directly verified (inherited) | Consistent with an append-only data-lake philosophy |

**Outbox infrastructure note (cross-cutting, not a transition):** DataLake's `OutboxProcessor`/
`OutboxCleanupService` are both hosted but no Application-layer command ever writes to the DataLake
Outbox table (confirmed dormant, not broken, per `publishing-map.md`). This is architecturally
distinct from PlantSafety's Outbox (F7-universal-but-consumers-unregistered) — here the feed itself
never happens, so there is no payload to evaluate for the Outbox path specifically; only the 2
in-process MediatR publishes (`IntegrationEventPublishedEvent`, `DataAccessApprovedEvent`) are worth
opening in a future phase.

**Module-level summary:** of DataLake's ~7 transitions, only 2 actually publish (both via in-process
MediatR, bypassing the dormant Outbox entirely) — the lowest publish rate of any module reviewed
this phase. Both of the 2 that do publish have zero subscribers. No duplicate-publish risk exists
(no consumer means no reaction, per F17's reasoning) and no event payload was directly re-opened
this phase — flagged as the top follow-up for a future DataLake-focused pass.
