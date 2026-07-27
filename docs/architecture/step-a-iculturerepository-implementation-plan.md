# Step A — ICultureRepository Implementation Plan

**Status note:** a background session (task `task_644ecd93`) was already
dispatched to implement this fix and, as of this writing, has live
uncommitted changes to `CultureRepository.cs`,
`CreateCultureCommandHandler.cs`, and
`AnalyticsLegacyRepositoryRegistration.cs`. This document was written by
inspecting those in-progress files as read-only reference material — it
does not modify them — and is intended to serve as an independent
specification to validate that work against, not a green-field proposal
written in ignorance of it.

## 1. The actual problem, precisely stated

There are **two unrelated interfaces named `ICultureRepository`** in this
codebase:

| Interface | Namespace | Members | Consumers |
|---|---|---|---|
| Thin write-side contract | `PlantSafety.Application.Common.Interfaces.Persistence` | `AddAsync(Culture, CancellationToken)` only | `CreateCultureCommandHandler` (confirmed live consumer — this is the one whose missing implementation broke the whole `WebApplicationFactory` test host) |
| Full DDD repository contract | `PlantSafety.Domain.Analytics.Culture.Interfaces` | `GetByIdAsync`, `ListAsync`, `AddAsync`, `UpdateAsync` | **None.** Confirmed via repo-wide grep: the only reference anywhere is a `global using` declaration in `PlantSafety.Domain/GlobalUsing.cs` — no class implements it, no handler injects it. |

`PlantSafety.Infrastructure/Analytics/Culture/Persistence/CultureRepository.cs`
existed with real, working methods (`GetByIdAsync(Guid,Guid,...)`,
`GetAllAsync(Guid,...)`) but implemented **neither** interface — it was a
bare class with no interface declaration at all, which is why DI had
nothing to bind to `ICultureRepository` when
`CreateCultureCommandHandler` asked for it.

## 2. Recommended contract: the thin interface is correct, the fat one is dead

**Implement `PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository`
(the thin one).** Reasoning:

- It's the one actually consumed by production code (`CreateCultureCommandHandler`).
- Its single method, `AddAsync`, matches the CQRS write-side pattern used
  by every other repository fixed in `AnalyticsLegacyRepositoryRegistration.cs`
  (`IBehaviorRepository`, `ICultureIndexRepository`, etc. — all thin,
  single- or few-method contracts, not full DDD repositories with
  `ListAsync`/specification objects).
- `CultureRepository.cs`'s pre-existing `GetByIdAsync`/`GetAllAsync`
  methods don't need to be forced into either interface — they can remain
  plain public methods on the concrete class, consumed directly by query
  handlers exactly as `IBehaviorRepository`'s siblings do elsewhere in
  this module. Nothing in this plan requires them to be part of an
  interface at all unless a query handler needs to be unit-testable
  against a fake, in which case a separate, narrow query interface
  (`ICultureReadRepository`, mirroring `IIncidentReadRepository`'s split
  from `IIncidentRepository`) is the idiomatic pattern already
  established in `SafetyEvents.IncidentManagement`.

**The Domain-layer `ICultureRepository` is dead code.** It has zero
consumers. Per this codebase's own established precedent (see
`RequireDashboardRead` policy removal, `IncidentManagement`'s deleted
factory/service/policy scaffold in the Phase 2 aggregate redesign),
genuinely unreferenced interfaces should be deleted, not implemented for
their own sake. **Recommendation: delete
`PlantSafety.Domain/Analytics/Culture/Interfaces/ICultureRepository.cs`
and its `GlobalUsing.cs` reference** in a follow-up cleanup pass — out of
scope for the immediate unblock, called out here so it isn't mistaken for
still-needed scaffolding.

## 3. Expected behaviors

For `AddAsync(Culture culture, CancellationToken)`:

- Opens a transaction (`IsolationLevel.ReadCommitted`, matching every
  other write-side repository in this codebase — `IncidentDapperRepository`,
  `AccidentDapperRepository`, etc.).
- Maps the aggregate's validated state to a stored-procedure parameter
  set via `StoredProcedureBuilder` (the same fluent builder used
  everywhere else — no bespoke parameter-building logic).
- Executes the insert stored procedure inside the transaction, commits.
- Enqueues the aggregate's pending domain events to the outbox
  (`IOutboxWriter`) **after** the commit, mirroring the
  post-commit-then-enqueue ordering established in
  `IncidentDapperRepository.CreateAsync`/`AccidentDapperRepository.AddAsync`
  — never publish before the write is durable.
- Clears the aggregate's domain events after enqueueing.

This is exactly what the in-progress `CultureRepository.AddAsync`
implementation already does (as read during this plan's research) — no
behavioral change is being proposed beyond what's already shaped that way.

## 4. Data sources — the blocking gap this plan surfaces

**`usp_Culture_Insert`, `usp_Culture_GetById`, and `usp_Culture_GetAll` do
not exist anywhere in the SQL tree.** Confirmed via a repo-wide search for
each procedure name across every `.sql` file — zero matches outside the
C# code that calls them. This means:

- The DI/interface fix alone is **necessary but not sufficient**. Once
  `ICultureRepository` resolves correctly, the `WebApplicationFactory`
  host will build — but the very first `CreateCultureCommand` sent through
  it, or any Culture read query, will fail at the database with an
  "could not find stored procedure" error, because these procedures were
  never created.
- **This must be added to whatever implementation work is happening now.**
  Three new stored procedures are needed under
  `PlantSafety.Infrastructure/Analytics/Culture/Sql/Procedures/` (a new
  folder — no `Sql/` subtree currently exists under
  `Analytics/Culture/`), following the canonical FY2027 pattern already
  used by every other procedure in this codebase: `TRY/CATCH`,
  centralized `AuditLog`/`ErrorLog` writes via the `[PlantSafety.AuditDB]`/
  `[PlantSafety.ErrorDB]` cross-database inserts (see
  `usp_Incident_Create.sql` as the closest recent reference — same shape,
  same era), soft-delete-aware `WHERE` filters if `Culture` rows are ever
  soft-deleted, and canonical header parameters
  (`@TenantId`, `@CorrelationId`, `@ActorId`, `@NowUtc`).
- A `[Analytics].[Culture]` table must also exist for these procedures to
  target. Whether it already exists was not verified as part of this
  plan (out of scope — a table-existence check belongs in the
  implementation work itself, not this document) but should be the very
  first thing the implementer confirms before writing procedure bodies.

## 5. Integration points

- **DI registration**: `AnalyticsLegacyRepositoryRegistration.AddAnalyticsLegacyRepositories()`
  is the single, already-established call site for this entire class of
  fix (21 of 22 originally-scaffolded repositories were already wired
  there; this is the 23rd/last one). No new registration file or call
  site is needed — just one more `services.AddScoped<...>()` line
  alongside its siblings, which the in-progress work has already added.
- **Consumer**: `CreateCultureCommandHandler` — no changes to its own
  code should be needed once the interface has a real implementation,
  since it already depends on the correct (thin) interface.
- **Test host**: `PlantSafety.IntegrationTests`' `IntegrationTestWebAppFactory`
  builds the exact same `PlantSafety.Api` DI container as production
  (via `WebApplicationFactory<Program>`), so fixing this in
  `PlantSafety.Infrastructure` fixes both the real API and every
  integration test in one change — no test-only registration path exists
  or should be created.

## 6. Migration-safe rollout plan

1. **Confirm the `[Analytics].[Culture]` table exists** (or create it if
   not, following `Incident.sql`'s conventions — tenant-scoped,
   `CreatedAtUtc`/`CreatedBy` audit columns, and a concurrency column if
   `UpdateAsync` is ever added later).
2. **Write the three missing stored procedures** (`usp_Culture_Insert`,
   `usp_Culture_GetById`, `usp_Culture_GetAll`) under a new
   `PlantSafety.Infrastructure/Analytics/Culture/Sql/Procedures/` folder,
   applying the same `RowVersion`→`NVARCHAR` conversion pattern fix
   documented in Step B **if** a `RowVersion`/`timestamp` column is ever
   projected by these procedures (none of the three obviously need one
   today, since `Culture` has no ETag-based concurrency check in the
   current interface — flag this explicitly rather than assume).
3. **Implement/confirm `CultureRepository : ICultureRepository`** (the
   thin Application-layer interface) — already substantially done in the
   in-progress background session per this plan's research; verify it
   matches section 3's behavior contract exactly, especially the
   post-commit outbox-enqueue ordering.
4. **Register in `AnalyticsLegacyRepositoryRegistration.cs`** — already
   done per this plan's research; verify the line is present and correctly
   scoped (`AddScoped`, matching every sibling registration).
5. **Delete the dead Domain-layer `ICultureRepository`** (section 2) in a
   clearly-separated commit from the fix itself, so the fix's diff stays
   reviewable and isn't conflated with unrelated cleanup.
6. **Verify**: build `PlantSafety.Api` and `PlantSafety.IntegrationTests`
   cleanly, then run the integration test suite (see the Step C
   restoration plan) — the `WebApplicationFactory` host must build for
   every module, not just Culture-touching tests, since this was a
   whole-container failure, not a scoped one.
7. **No breaking change risk to other modules**: this fix is strictly
   additive (one new DI registration, new stored procedures, an interface
   implementation on a previously-orphaned class) — nothing existing is
   being renamed, removed, or given new required parameters, so no other
   module's behavior can regress from this change alone.

## 7. Explicitly out of scope for this plan

- Whether `Culture` needs an `UpdateAsync`/`ListAsync` path at all (the
  Domain-layer interface implies future need, but no consumer exists yet
  — build it when a real consumer appears, not speculatively).
- Any change to the `Culture` aggregate itself (`PlantSafety.Domain.Analytics.Culture.Aggregates.Culture`)
  — this plan only concerns the repository/persistence gap.
