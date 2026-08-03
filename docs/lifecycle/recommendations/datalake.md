# Recommendations — DataLake

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. DataLake has no
C# domain aggregate layer and none of PlantSafety's cross-cutting fixes (XC-1 through XC-6) apply to
it directly — it has its own, independently-worse security/eventing gaps already fully documented in
`docs/ingestion/recommendations/datalake-recommendations.md`. This file cross-references that
document's Top-5 rather than duplicating it, adding only the lifecycle-audit-specific framing this
phase contributes.

1. **Wire authentication middleware into `DataLake.Api`** — already this module's own Top-5 #1 in
   `docs/ingestion/recommendations/datalake-recommendations.md`. Zero `AddAuthentication`/
   `AddJwtBearer`/`[Authorize]` exist anywhere; the "required" `TenantId`/`RequestingUserId` values
   are read from a context nothing trustworthy populates. This lifecycle audit reconfirms DataLake's
   per-repository `WHERE TenantId = @TenantId` filtering is real but is the **only** control — worse
   than PlantSafety's "RLS exists but doesn't engage" story, because there is no second layer here at
   all. Cite: `security/rls-audit.md` §5, `gaps/_severity-ranking.md` (DataLake context),
   `gaps/datalake.md`. Effort: M. Risk: **Security** — confirmed, live. Sequencing: **independent of
   PlantSafety's XC-1/XC-3 chain** — DataLake has its own authentication gap, not the same
   tenancy/RLS defect, so it does not need to wait for PlantSafety's sequencing to resolve.

2. **Fix `Dataset.ArchivePartitionAsync`'s missing idempotency/precondition check** — can be called
   repeatedly on the same dataset with no guard against re-archiving and no event published for the
   transition at all (not merely silent — genuinely absent). This is DataLake's closest analog to
   PlantSafety's rule-enforcement gaps and its own confirmed, not pattern-inferred, defect. Fix: add
   a status/precondition check before archiving, and publish the missing event once the check is in
   place. Cite: `gaps/datalake.md`. Effort: S-M. Risk: **Data integrity** — repeated archival calls
   have undefined/unguarded behavior today. Sequencing: none.

3. **Feed the Outbox or remove it** — fully wired (processor, cleanup service, tenant-scoped schema)
   but zero Application-layer command ever writes to it; already this module's own Top-5 #2 in
   `datalake-recommendations.md`. Cite: `gaps/datalake.md`. Effort: M (wire) or S (remove, if a
   product decision confirms it's unneeded). Risk: None-just-tech-debt (dormant, not broken) but real
   ongoing resource cost. Sequencing: none, but do before or alongside item 4 (register consumers) —
   no point registering consumers for events that never get raised.

4. **Register consumers for `IntegrationEventPublishedEvent`/`DataAccessApprovedEvent`** — the two
   events DataLake correctly publishes via `IPublisher.Publish`, both with zero subscribers; already
   this module's own Top-5 #3. Cite: `command-handler-map/_shared-findings.md` F9 (the same
   zero-consumer pattern solution-wide), `gaps/datalake.md`. Effort: S-M. Risk:
   Availability/tech-debt — the module does the right thing and it still goes nowhere. Sequencing:
   none.

5. **Bring the module's SQL DDL under source control** — zero `.sql` files exist anywhere in
   DataLake, capping every shape-audit conclusion at "Dapper record shape confirmed, SQL shape
   unverifiable." Already this module's own Top-5 #5. Cite: `gaps/datalake.md`,
   `sql-shape/_shared-findings.md` (methodology note — DataLake explicitly excluded from the SQL-shape
   audit's confirmed findings for this reason). Effort: M-L. Risk: **Data integrity/governance** —
   blocks any future shape-drift audit or column-level security review for this entire module.
   Sequencing: do before or alongside item 1 — once authentication exists, a missing per-repository
   tenant filter becomes immediately exploitable, and confirming the real column types/constraints
   requires this item first.

See `docs/ingestion/recommendations/datalake-recommendations.md` for full detail on items 1, 3, 4, 5.
DataLake does not participate in PlantSafety's XC-1/XC-2/XC-3 sequencing chain (`_cross-cutting.md`)
— its authentication and outbox gaps are independent defects on independent infrastructure.
