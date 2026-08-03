# Gaps — DataLake

No C# domain aggregate layer; 7 command-shaped transitions across Ingestion/Processing/Integration/
Archival/Governance/MetadataCatalog. See `_legend.md`.

| Concept | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| IngestionJob | No Update/Delete/Cancel concept exists — immutable once created, structurally intentional for a data-lake philosophy `[Inv]` | N/A — no domain guard layer exists to bypass (command handler writes directly via Dapper) `[Inv]` | N/A (inherent) | N/A — no aggregate concept | **N/A — different infra**, no `OutboxWriter.Enqueue`/`IUnitOfWork.EnqueueEvent` call site exists here at all `[Actor§3]` | N/A — no `.sql` files under source control for this module; Dapper infers parameter size from value length, so no fixed-cap truncation risk exists `[SQL-G3]` | N/A — no RLS/SESSION_CONTEXT hits found `[RLS§5]` |
| ProcessingJob | No "mark failed/retried" transition found `[Inv]` | N/A | N/A (inherent) | N/A | **N/A — different infra** `[Actor§3]` | N/A — same Dapper-parameter-inference note | N/A |
| IntegrationEvent | N/A — publishes correctly | N/A | **No** — publishes `IntegrationEventPublishedEvent` correctly, but zero subscribers exist (same EVT-B-shaped dead-end as PlantSafety) `[CH-F9]` | N/A | **N/A — different infra**; `ActorId` never a first-class outbox column at all in this module — strictly worse than PlantSafety's "column exists but hardcoded" `[Actor§3]` | N/A | N/A |
| Dataset (Archival) | No idempotency/precondition check — `ArchivePartitionAsync` can be called repeatedly on the same dataset `[Inv]` | **Yes** — no event published for this transition, no guard against re-archiving `[Inv]` | **Yes** — no event published at all for this transition `[Inv]` | N/A | **N/A — different infra** `[Actor§3]` | N/A | N/A |
| Access Request (Governance) | N/A | N/A | **No** — publishes `DataAccessApprovedEvent` correctly, zero consumers (2nd confirmed "does the right thing, still dead-ends" case) `[CH-F9]` | N/A | **N/A — different infra** `[Actor§3]` | N/A | N/A |
| Dataset ownership/documentation commands | Not opened/confirmed by any phase — still an open follow-up `[Inv]` | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed |
| Any DataLake concept | **Delete missing entirely** — consistent with an append-only philosophy, plausibly intentional `[Inv]` | — | — | — | — | — | — |

**Net for this module**: DataLake's per-repository `WHERE TenantId = @TenantId` filtering is real but is the **only** control (app-layer only, unauthenticated-caller-trusting) — worse than PlantSafety's RLS-exists-but-doesn't-engage story, because there is no second layer here at all `[RLS§5]`. `Dataset.ArchivePartitionAsync`'s missing idempotency guard is this module's closest analog to PlantSafety's rule-enforcement gaps.
