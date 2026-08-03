# Modernization Recommendations — DataLake

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
DataLake has excellent-looking tenant/actor plumbing at the code level (`DataLakeControllerBase`'s
`TenantId`/`RequestingUserId` are non-nullable, throwing accessors) and real FluentValidation
coverage — but sits behind zero authentication, and its entire eventing/outbox layer is either
never fed or published into a void.

---

## Top 5 fixes, in priority order

1. **Wire authentication middleware (JWT bearer + `[Authorize]`) into `DataLake.Api`.** Currently
   zero `AddAuthentication`/`AddJwtBearer`/`[Authorize]` exist anywhere in the module — the
   "required" `TenantId`/`RequestingUserId` values are read from a context nothing trustworthy
   populates. Effort: M. Risk of not fixing: **Security** (confirmed, not theoretical — an
   unauthenticated caller can supply any TenantId/RequestingUserId value today).
2. **Feed the Outbox from at least the Ingestion/Processing/Integration command handlers**, or
   remove the Outbox infrastructure if it's not actually needed — as built it's fully wired
   (processor, cleanup service, tenant-scoped schema) but zero Application-layer command ever
   writes to it. Effort: M. Risk of not fixing: **None-just-tech-debt** (dormant, not broken) but
   real engineering effort sitting unused.
3. **Register `INotificationHandler<T>` consumers for `IntegrationEventPublishedEvent` and
   `DataAccessApprovedEvent`**, the two events DataLake *does* correctly publish via
   `IPublisher.Publish` — both currently have zero subscribers. Effort: S-M depending on what the
   handler should actually do. Risk of not fixing: **Availability/tech-debt** — the module does the
   right thing and it still goes nowhere.
4. **Instantiate/publish the defined-but-never-raised events** (`RawDataIngestedEvent`,
   `RawToCleansedCompletedEvent`, `CleansedToCuratedCompletedEvent`, and likely more under
   `DataLake.Application.*.Events`) from their respective command handlers, or delete the unused
   event classes if the design has moved on from needing them. Effort: M. Risk of not fixing:
   **None-just-tech-debt** today, but blocks any future consumer from ever having something to
   subscribe to.
5. **Locate or reconstruct the missing SQL DDL** for DataLake's tables/stored procedures (zero
   `.sql` files exist anywhere in this module, unlike PlantSafety's 772) — bring it under source
   control. Effort: M-L (may require reverse-engineering from a live database). Risk of not fixing:
   **Data integrity / governance** — column types, nullability, truncation risk, and CHECK
   constraints cannot be verified from source at all today; every shape question in this module
   stops at "Dapper record shape, SQL shape unverifiable."

---

## Security / Availability

### 1. Zero authentication middleware wired
- **Problem**: `DataLakeControllerBase.TenantId`/`RequestingUserId` are non-nullable, throwing
  accessors — genuinely strong-looking tenant/actor design. But grepping the entire DataLake tree
  found zero `AddAuthentication`/`AddJwtBearer` calls and zero `[Authorize]` attributes anywhere,
  and `Program.cs` never wires authentication middleware. There is nothing establishing *who*
  populates `HttpContext.Items` before those "required" values are trusted.
  (`classification-matrix.md` Top-5 #1, `security/rls-audit.md` §3.)
- **Fix**: Add `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` in
  `DataLake.Api`'s DI setup (mirroring Azure's already-correct pattern in
  `Compute.Api`/`Data.Api`), and `[Authorize]` every controller (or a global
  `RequireAuthorizedUser` fallback policy). Confirm via integration test that an unauthenticated
  request is rejected before reaching a handler.
- **Effort**: M.
- **Risk of not fixing**: **Security** — confirmed, high-confidence gap; per-query tenant
  filtering is sound but the identity it filters on is never authenticated, meaning an
  unauthenticated caller can supply any `TenantId` value.

### 2. No SQL Server RLS or equivalent tenant-scoping at the DB layer
- **Problem**: Unlike PlantSafety's real, tested Row-Level Security, DataLake relies entirely on
  application-level `WHERE TenantId = @TenantId` filtering built per-repository in C# — correctly
  derived, but with no defense-in-depth if a future repository method omits the filter.
  (`security/rls-audit.md` §3-4.)
- **Fix**: Not urgent to build full SQL RLS immediately (that's a larger investment), but at
  minimum audit every repository method for a missing `TenantId` filter now that authentication is
  being added (item #1) — a missing filter becomes immediately exploitable the moment
  authentication closes the "who is the caller" gap. Consider PlantSafety's `SESSION_CONTEXT` +
  `SECURITY POLICY` approach as a template if/when DataLake's SQL is brought under source control
  (see Top-5 #5).
- **Effort**: M (audit) / L (full RLS build-out, longer-term).
- **Risk of not fixing**: **Security** (defense-in-depth gap; becomes live risk as soon as
  authentication is fixed and a missing filter exists in any repository).

---

## Data Integrity

### 3. Outbox infrastructure fully built, never fed
- **Problem**: `OutboxProcessor`/`OutboxRepository`/`OutboxCleanupService`
  (`DataLake.Infrastructure\Outbox\*.cs`) are fully wired, hosted (`AddHostedService<OutboxProcessor>()`,
  `AddHostedService<OutboxCleanupService>()`), and correctly tenant-scoped (`OutboxMessage.TenantId`)
  — but grepping all of `DataLake.Application` for any write to `OutboxMessage`/`IOutboxRepository`
  returns zero hits. Both hosted services run continuously, finding and doing nothing.
  (`module-flow/datalake.md` §Outbox, `gaps/orphaned-ingress.md` #5.)
- **Fix**: Either wire the Ingestion/Processing/Integration/Governance command handlers to write
  to the outbox as part of their transaction (the natural next step once events are actually being
  raised — see Event Wiring #4/#5 below), or remove the outbox infrastructure entirely if the
  module's eventing needs are fully served by the in-process MediatR `IPublisher.Publish` pattern
  it already uses elsewhere. Don't leave a fully-built, hosted, polling service running against a
  table nothing writes to indefinitely — it's a resource cost and a maintenance liability with zero
  payoff.
- **Effort**: M (wire it up) or S (remove it, if a product decision confirms it's not needed).
- **Risk of not fixing**: **None-just-tech-debt** — dormant, not broken, but real engineering
  effort with zero return until either wired or removed.

### 4. `PayloadJson` on `IngestRawDataCommand` has no `MaximumLength` validator
- **Problem**: `PayloadJson`'s FluentValidation rule is `NotEmpty()` only — no length cap — on an
  arbitrary producer payload flowing to an unverifiable SQL column (see Top-5 #5). If the real
  column is not `NVARCHAR(MAX)`, this is the highest-risk field in the module for either silent
  data loss or an in-flight insert failure. (`shape-audit/datalake-shape.md` finding #4.)
- **Fix**: Confirm the real column type once the missing SQL is located/reconstructed (Top-5 #5);
  add an explicit `MaximumLength` rule matching it, or confirm `NVARCHAR(MAX)` is intentional and
  document that assumption in the validator.
- **Effort**: S (once the SQL is known).
- **Risk of not fixing**: **Data integrity** (unverified truncation/failure risk on arbitrary
  payload data) — severity depends on the actual unseen column size, currently unknown.

---

## Event Wiring

### 5. `IntegrationEventPublishedEvent` and `DataAccessApprovedEvent` — published correctly, zero subscribers
- **Problem**: `PublishIntegrationEventCommandHandler` and `ApproveDataAccessCommandHandler`
  (Governance) both correctly call `IPublisher.Publish(...)` — this is the module doing the right
  thing — but grepping for `INotificationHandler<IntegrationEventPublishedEvent>` and
  `INotificationHandler<DataAccessApprovedEvent>` returns zero results in both cases.
  (`gaps/missing-events.md` §2b, `lifecycle-coverage/datalake-lifecycle.md` §Governance.)
- **Fix**: Add `INotificationHandler<IntegrationEventPublishedEvent>` and
  `INotificationHandler<DataAccessApprovedEvent>` classes performing whatever real downstream
  action these events are meant to trigger (cross-module notification, audit-log write, whatever
  the product intent behind "publish an integration event" and "approve a data access request"
  actually is) — MediatR auto-registers `INotificationHandler<T>` via assembly scan, so no DI
  wiring beyond creating the class is needed.
- **Effort**: S-M depending on what the handler needs to do.
- **Risk of not fixing**: **Availability/tech-debt** — no data loss, but the one place DataLake's
  Application layer does eventing "correctly" still produces zero observable effect.

### 6. `RawDataIngestedEvent`/`RawToCleansedCompletedEvent`/`CleansedToCuratedCompletedEvent` never instantiated
- **Problem**: Full audit-envelope event classes exist (`AggregateId`, `TenantId`,
  `RawZonePartitionPath`, etc.) but the command handlers that should raise them
  (`IngestRawDataCommandHandler`, `ProcessRawToCleansedCommandHandler`, and by the same pattern the
  cleansed-to-curated handler) only log and return — the event contract was designed, the handler
  was never updated to raise it. Likely recurs across the other 7 read-only controller groups'
  `*Event` classes (pattern-inferred, not individually re-verified). (`module-flow/datalake.md`
  Flows 1-2, `gaps/missing-events.md` §2a.)
- **Fix**: Add the `IPublisher.Publish(new RawDataIngestedEvent(...))` (and siblings) call to each
  handler, populating the event from data already available in the command/repository result.
  Combine with item #3 (feed the outbox) if the intent is durable, at-least-once delivery rather
  than in-process MediatR notification.
- **Effort**: M (3+ handlers, plus the pattern-inferred rest if confirmed).
- **Risk of not fixing**: **None-just-tech-debt** today (no consumer exists yet to receive these
  anyway) but blocks the entire ingestion/processing pipeline's observability story.

---

## Lifecycle / Data Governance

### 7. Zero source-controlled SQL for the entire module
- **Problem**: `find DataLake -iname "*.sql"` returns zero results. Every repository calls a
  stored procedure by string name only (`"DataLake.CreateIngestionJob"`, etc.) with no
  corresponding `CREATE PROCEDURE`/`CREATE TABLE` script tracked in this repository. This caps
  every shape-audit conclusion in this module at "Dapper record shape confirmed, SQL shape
  unverifiable" — column types, nullability, truncation risk, and CHECK constraints cannot be
  confirmed, denied, or diffed against a migration history from source alone.
  (`shape-audit/datalake-shape.md` finding #1.)
- **Fix**: Bring the actual DDL under source control in this repository, following PlantSafety's
  `*.Infrastructure\<Domain>\Sql\{Tables,Procedures}\` convention — either by exporting the live
  schema from whatever database currently backs this module, or, if no such database exists yet
  (module may be pre-production), by writing the DDL as part of finishing the module's build-out.
- **Effort**: M-L (depends entirely on whether a live schema already exists to export).
- **Risk of not fixing**: **Data integrity / governance** — this is a fundamental blocker to any
  future shape-drift audit, migration review, or column-level security review for this module; not
  itself a live bug, but it makes several other findings in this document (item #4 especially)
  permanently unverifiable without it.

---

## Deliberately not recommended

- **Not recommending building full SQL Server RLS immediately** (item #2) — that's a larger
  investment appropriate once the SQL itself is under source control (item #7) and authentication
  is closed (item #1); sequencing matters here, and jumping straight to RLS before those two land
  would be premature.
