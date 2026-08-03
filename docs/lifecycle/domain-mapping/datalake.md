# Domain Mapping — DataLake

See `_shared-findings.md` §F1 for context — DataLake has no C# aggregate layer, so the tenancy
question here is about command-handler validation, not aggregate-method guards. Not independently
re-verified this phase; inherited from Phase 1 (itself inherited from the prior Ingestion Discovery).

| Concept | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| IngestionJob | Create (Ingest Raw Data) | Immutable once created — no Update/Delete/Cancel concept | N/A (by design) | Not independently confirmed | None | Unconfirmed — not independently checked this phase | Directly verified (inherited) | |
| ProcessingJob | Create (Raw→Cleansed, Cleansed→Curated) | No "mark failed/retried" transition exists (Phase 1 finding) | **N** — confirmed structural gap: a failed processing run has no formal failure-state transition | Not independently confirmed | Should plausibly check `IngestionJob` completed before processing — not independently confirmed this phase | Unconfirmed | Directly verified (inherited) | |
| IntegrationEvent | Create (Publish) | Publishes correctly (`IntegrationEventPublishedEvent`) but zero subscribers (Phase 1 finding) | Y for publish mechanics; **N** in the sense that nothing downstream ever consumes it — a "does the right thing, dead-ends" case | — | None confirmed | Unconfirmed | Directly verified (inherited) | |
| Dataset (Archival) | ArchivePartitionAsync | No event published; no idempotency/precondition check — a dataset can be "archived" repeatedly (Phase 1 finding) | **N** — confirmed gap | None confirmed | None | Unconfirmed | Directly verified (inherited) | Distinct from PlantSafety's silent-archive pattern (F3) — this one lacks even the guard, not just the event |
| Access Request (Governance) | Approve | Publishes `DataAccessApprovedEvent` correctly — zero consumers (Phase 1 finding) | Y for publish; same "does the right thing, dead-ends" shape as IntegrationEvent | Not independently confirmed | Should plausibly notify Compliance/Security modules — no confirmed consumer | Unconfirmed | Directly verified (inherited) | |
| Dataset ownership/documentation | UpdateDatasetDocumentationCommand/UpdateDatasetOwnershipCommand | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed | Pattern-inferred (inherited) | File existence only, not opened by either phase |
| Any DataLake concept | Delete | N/A — missing entirely | N/A | N/A | N/A | N/A | Directly verified (inherited) | Append-only philosophy, plausibly intentional |

**Summary:** `Dataset.ArchivePartitionAsync`'s lack of any idempotency check (repeatable archival
with no guard) is this module's clearest "stated invariant not enforced" candidate, though it was
not re-opened this phase to confirm the stated invariant explicitly (inherited finding).
