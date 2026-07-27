# Triage 1 — Culture Subsystem

## 1. Full failure chain

Three independent defects compound into a completely non-functional
write/read path for `Analytics.Culture`:

1. **Interface duplication.** Two unrelated `ICultureRepository`
   interfaces exist:
   - `PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository`
     — thin, one method (`AddAsync`), the one `CreateCultureCommandHandler`
     actually depends on.
   - `PlantSafety.Domain.Analytics.Culture.Interfaces.ICultureRepository`
     — a full DDD repository contract (`GetByIdAsync`, `ListAsync`,
     `AddAsync`, `UpdateAsync`). Confirmed to have **zero consumers**
     anywhere in the codebase (the only reference is a `global using` in
     `PlantSafety.Domain/GlobalUsing.cs`) — dead code, not an alternate
     live path.
2. **Missing implementation → missing DI registration.**
   `PlantSafety.Infrastructure/Analytics/Culture/Persistence/CultureRepository.cs`
   pre-existed with working `GetByIdAsync`/`GetAllAsync` methods but
   implemented **neither** interface, so nothing could be registered for
   `CreateCultureCommandHandler` to resolve. This is the defect that
   broke the entire `WebApplicationFactory` test host for the whole
   `PlantSafety.IntegrationTests` project (`ValidateOnBuild = true`
   validates the full container, not just Culture's slice of it) — a
   single-module gap with a whole-project blast radius, purely because of
   *when* in the DI graph construction order the failure was hit.
3. **Missing stored procedures.** Independent of (1) and (2):
   `usp_Culture_Insert`, `usp_Culture_GetById`, and `usp_Culture_GetAll`
   — the three procedures `CultureRepository` calls — **do not exist
   anywhere in the SQL tree**, confirmed via repo-wide search. Even a
   fully correct C#/DI fix cannot make Culture work end-to-end without
   these being written, and it is not yet confirmed whether the
   `[Analytics].[Culture]` table itself exists.

**Status as of this document**: a background session was dispatched to
fix (1) and (2); as of this writing it has live, uncommitted changes to
`CultureRepository.cs`, `CreateCultureCommandHandler.cs`, and
`AnalyticsLegacyRepositoryRegistration.cs` addressing the interface/DI
side. **Defect (3) — the missing stored procedures — has not been
addressed by that work**, confirmed by a fresh search at the time of
writing this document (zero `usp_Culture_*.sql` files exist anywhere).

## 2. Repair, replace, or retire?

**Repair.** Reasoning:

- The gap is narrow and mechanical, not a sign of a wrong design. The
  `Culture` domain aggregate, its `CreateCultureCommandHandler`, and the
  `CultureRepository`'s read methods are all real, working code that
  simply never got its write-side persistence finished. This is
  materially different from, say, the Domain-layer `ICultureRepository`
  (genuinely dead, zero consumers, correctly recommended for deletion in
  Step A) or a module that was actively designed one way and needs
  rethinking.
- There is a live UI consumer: `PlantSafety.UI/Modules/Culture/ApiClients/CultureApiClient.cs`
  and `CultureDashboardPage.razor` already exist and presumably expect
  this to work. Retiring the module would mean removing or disabling
  existing UI surface, which is a larger, more visible decision than
  finishing three stored procedures.
- No evidence was found during Steps A–G of a competing/duplicate module
  that already covers Culture's responsibility (survey/engagement/culture
  index metrics) — "replace" would imply an alternative exists to migrate
  to, which isn't the case here.

**Explicitly not recommended**: "replace" (rewrite from scratch) — the
existing `Culture` aggregate, command handler, and repository read
methods show no signs of being wrong in shape, only incomplete. A rewrite
would re-derive work that already exists and works (`GetByIdAsync`/
`GetAllAsync`), for no benefit over finishing the missing third.

## 3. Safe modernization plan

1. **Confirm `[Analytics].[Culture]` table existence** — the first,
   cheapest verification step, blocking everything after it. If it
   doesn't exist, it must be created first, following `Incident.sql`'s
   conventions (tenant-scoped, audit columns, and — per the RowVersion
   coverage plan (Triage 2) — a considered, not default, choice of
   concurrency column type).
2. **Write the three missing stored procedures** under a new
   `PlantSafety.Infrastructure/Analytics/Culture/Sql/Procedures/`
   folder, following the canonical pattern (TRY/CATCH,
   AuditDB/ErrorDB logging — see Triage 4 for why this matters more than
   it might for other modules given Culture's fresh start) and the
   canonical header-parameter contract.
3. **Land the in-progress interface/DI fix** (already substantially
   complete per direct inspection) as its own reviewable commit, separate
   from the stored-procedure addition, so each change can be verified
   independently — a DI fix that silently unblocks compilation but still
   fails at the database is a worse failure mode to debug than either
   issue in isolation.
4. **Delete the dead Domain-layer `ICultureRepository`** in a third,
   separate commit — genuinely unrelated cleanup, shouldn't be bundled
   with the functional fix.
5. **Verify** via the integration test host restoration (Step C) once
   that's unblocked — a `CreateCultureCommand` sent through the real
   `WebApplicationFactory` host, hitting a real Testcontainers SQL Server,
   is the only verification that actually proves all three defects are
   resolved together; verifying each in isolation (interface compiles,
   procedure syntax parses) leaves the integration between them unproven.
6. **Re-audit `Modules/Culture`'s UI** (`CultureApiClient.cs`,
   `CultureDashboardPage.razor`) against the finished backend before
   declaring the UI module ready — per the UI readiness matrix (Triage
   5), Culture is currently the clearest example of UI existing ahead of
   a working backend.

## 4. Risk if left unrepaired

Every `CreateCultureCommand` sent to the real API — from the UI, from any
future integration, from any test — will fail either at DI resolution
(pre-fix) or at the database with "could not find stored procedure"
(post-fix, pre-Triage-3-of-this-document). Given a UI page already exists
that presumably drives this path, this is a live, user-facing gap, not
purely a backend/test concern.
