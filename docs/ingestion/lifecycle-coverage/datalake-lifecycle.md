# Lifecycle Coverage — DataLake

Phase 6 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/datalake.md`, `gaps/orphaned-ingress.md`, `gaps/missing-events.md` (Phase 3-4),
plus direct reads this phase (marked **[NEW]**) of `DataLake.Application`'s command handlers and
validators for Archival, Governance, and MetadataCatalog — the three sub-areas with any
non-Create-shaped command at all.

## Architectural note carried from Phase 3, load-bearing for this document

DataLake has **no C# domain aggregate layer**. Every "lifecycle transition" is a MediatR command
handler that calls a Dapper repository method directly against a SQL row — there is no `Status`
enum, no `EnsureStatusIsOneOf`-style guard, and structurally **cannot** be a state-machine violation
in the way PlantSafety's aggregates can, because there is no in-memory state to protect. This
changes what "Domain-Rule-Enforced?" means for this module: the only guard mechanism available is
FluentValidation on the command's *input shape* (format, non-empty, allow-listed enum strings) —
there is no cross-field or state-transition business rule of the kind found throughout PlantSafety
(e.g. "can't close without regulatory filing"). This is a structural limitation of the module, not a
per-command oversight, and is noted once here rather than repeated in every row.

| Aggregate/Concept | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| IngestionJob | Create (Ingest Raw Data) | Yes | Input-validation only (validator not re-read this phase; Phase 3 confirmed the handler itself has no business-rule branches) | **No** — `RawDataIngestedEvent` class exists, never instantiated (Phase 3, unchanged) | Unverified — no AuditLog-equivalent mechanism found in DataLake this phase (DataLake has no cross-DB AuditLog table pattern like PlantSafety's) | Yes — `DataLakeControllerBase.TenantId` required, non-nullable accessor | Directly verified (Phase 3 carried forward) | No Update/Delete/Cancel concept for a raw-zone ingestion job exists anywhere — a job, once created, is immutable in this module's data model |
| ProcessingJob (Raw→Cleansed, Cleansed→Curated) | Create (Process) | Yes | Input-validation only | **No** — `RawToCleansedCompletedEvent`/`CleansedToCuratedCompletedEvent` classes exist, never published (Phase 3, unchanged) | Unverified | Yes | Directly verified (Phase 3 carried forward) | Same immutable-job shape as Ingestion — no follow-on transition (e.g. "mark processing job failed/retried") found anywhere in `Processing.Application` |
| IntegrationEvent record | Create (Publish Integration Event) | Yes | Input-validation only | Yes — `IntegrationEventPublishedEvent`, the module's one genuine `IPublisher.Publish` call (Phase 3, unchanged) | Unverified | Yes | Directly verified (Phase 3 carried forward) | Publishes correctly but zero subscribers (Phase 4) |
| Dataset (Archival) | **[NEW]** ArchivePartitionAsync (`ArchiveDatasetCommand`) | Yes | Input-validation only — `ArchiveDatasetCommandValidator` (full read this phase) checks `TenantId`/`CorrelationId`/`ApiVersionToken` shape, `PartitionKey` non-empty ≤128 chars, `Zone` from an allow-list, `TargetStorageTier` from `{hot, cool, archive}`, `ArchivalReason` non-empty ≤512 — no check for "is this partition already archived" or any other state precondition, because no state is tracked | **No** — the handler (full read this phase) calls the repository and returns; no `IPublisher`/event type referenced anywhere in `ArchiveDatasetCommandHandler.cs` | Unverified | Yes — `TenantId` validated via `MustBeValidTenantId()` | Directly verified **[NEW]** | A dataset can be "archived" repeatedly with no idempotency or precondition check at the domain layer (there is none) — whether the underlying stored procedure itself has an idempotency guard was not checked this phase |
| Access Request (Governance) | **[NEW]** Approve (`ApproveDataAccessCommand`) | Yes | Input-validation only (validator not re-read this phase) — no domain check that the access request is in a pending/approvable state | Yes — **[NEW]** `DataAccessApprovedEvent` is genuinely published via `IPublisher.Publish` (full handler read this phase, confirms the same "does the right thing, still dead-ends" shape as `IntegrationEventPublishedEvent`) | **[NEW] Zero consumers** — grepped the whole `DataLake` tree for `INotificationHandler<DataAccessApprovedEvent>`: zero results. This is a **previously undocumented instance** of the "published, zero subscriber" pattern Phase 4 found once (`IntegrationEventPublishedEvent`) — this phase confirms it recurs in Governance too | Yes — `request.TenantId` flows onto the event | Directly verified **[NEW]** | A second confirmed "publishes correctly into the void" case for `gaps/missing-events.md` §2b to append |
| Dataset ownership/documentation | **[NEW]** UpdateDatasetDocumentationCommand / UpdateDatasetOwnershipCommand (MetadataCatalog) | Yes | Input-validation only (handlers/validators not opened line-by-line this phase — existence and naming confirmed by file listing only) | Unverified this phase | Unverified | Presumed yes (same controller-base pattern) | Pattern-inferred (file existence only) | Flagged for a deeper follow-up pass — not read this phase given time budget; included here so it isn't silently dropped from the lifecycle inventory |
| Any DataLake concept | Delete / hard-delete | **No** — no `Delete*Command` found anywhere in `DataLake.Application` (grepped) | N/A | N/A | N/A | N/A | Directly verified **[NEW]** | Consistent with an append-only data-lake philosophy — plausibly intentional, not necessarily a gap |
| Outbox (cross-cutting) | N/A | Infrastructure exists (`OutboxProcessor`/`OutboxRepository`/`OutboxCleanupService`, tenant-scoped) | N/A | N/A — nothing ever writes to it (Phase 4, unchanged) | N/A | Yes (schema-level) | Directly verified (Phase 4 carried forward) | Dormant, not broken — no command in this module ever calls it |

## Summary for this document

DataLake's lifecycle coverage is **uniformly shallow rather than asymmetric**: unlike PlantSafety
(where some aggregates are richly guarded and others are Create-only, producing a visible
asymmetry), every DataLake command examined — Ingestion, Processing, Integration, Archival,
Governance — has the same shape: real input-shape validation, no state-transition concept, and
either no event published at all or an event published into a confirmed void. There is no "here is
the positive example, here is the dangerous gap" contrast to draw within this module the way there
is in PlantSafety, because there is no state machine anywhere to compare against. The module's
single most notable finding this phase is the **[NEW]** `DataAccessApprovedEvent` case: a second
confirmed instance of "does the right MediatR-publish thing, still has zero subscribers," which
should be added to `gaps/missing-events.md` §2b for Phase 7.
