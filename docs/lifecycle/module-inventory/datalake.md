# Module Inventory — DataLake

No C# domain aggregate layer exists in this module (structural, confirmed by the prior discovery and
unchanged this phase) — every "transition" is a MediatR command handler writing directly via Dapper.
All rows below are reformatted from `docs/ingestion/lifecycle-coverage/datalake-lifecycle.md`
(directly verified/pattern-inferred labels inherited unchanged); not independently re-verified this
phase. DataLake also has **no SQL under source control in this repository** (confirmed by the prior
discovery) — so the "Exists in SQL?" column here means "does a Dapper call to a named SQL object
exist in the C# source," not "was a `.sql` file found," unlike every PlantSafety table in this
document set.

| Aggregate/Concept | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| IngestionJob | Create (Ingest Raw Data) | Yes | Referenced via Dapper call, no `.sql` file to inspect | Live (Create only) | Directly verified (inherited) | No Update/Delete/Cancel concept exists — immutable once created |
| ProcessingJob | Create (Process, Raw→Cleansed, Cleansed→Curated) | Yes | Same as above | Live (Create only) | Directly verified (inherited) | No "mark failed/retried" transition found |
| IntegrationEvent | Create (Publish Integration Event) | Yes | Same as above | Live | Directly verified (inherited) | Publishes correctly (`IntegrationEventPublishedEvent`) but zero subscribers |
| Dataset (Archival) | ArchivePartitionAsync | Yes | Referenced via Dapper call | Live | Directly verified (inherited) | No event published; no idempotency/precondition check — a dataset can be "archived" repeatedly |
| Access Request (Governance) | Approve | Yes | Referenced via Dapper call | Live | Directly verified (inherited) | Publishes `DataAccessApprovedEvent` correctly — zero consumers, a 2nd confirmed "does the right thing, still dead-ends" case |
| Dataset ownership/documentation | UpdateDatasetDocumentationCommand / UpdateDatasetOwnershipCommand | Yes (file existence only) | Unconfirmed | Unconfirmed | Pattern-inferred (inherited) | Not opened by the prior discovery either — still a follow-up candidate |
| Any DataLake concept | Delete / hard-delete | **No** — missing entirely | N/A | N/A | Directly verified (inherited) | Consistent with an append-only data-lake philosophy — plausibly intentional |
| Outbox (cross-cutting) | N/A | Infrastructure exists, never written to | N/A | N/A | Directly verified (inherited) | Dormant, not broken |
