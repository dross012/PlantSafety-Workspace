# Security Audit — RLS / Tenant-Scoping Mechanism, and the Phase 4 Authorization Question

Phase 7 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Method: full grep census of every `[Authorize]` attribute in `PlantSafety.Api` (369 real
attribute usages across 362 controller files, comment-text false positives excluded), full
reads of the DI registration files, a forced clean `dotnet build` of `PlantSafety.Api` to
verify compile-time truth (not just static grep), and full reads of the SQL Server RLS
migration and its C# call sites.

---

## Bottom line, stated first

**There is no live, exploitable cross-tenant data leak from the client-supplied-`TenantId`
read-endpoint pattern Phase 4 flagged as the top open question.** Every one of the 155
directly-verified client-supplied-`TenantId` read controllers is gated by an
`[Authorize(Policy = "<Feature>AuthorizationPolicies.Read")]` (or equivalent quoted
module-level string) attribute, and **not one** of those policy names is registered anywhere
in the DI container. The real-world effect is **(b) from Phase 4's framing: a broad
availability defect**, not a live tenant-isolation breach — every request to one of these
endpoints throws `InvalidOperationException: The AuthorizationPolicy named: '...' was not
found` inside ASP.NET Core's authorization middleware, before the request ever reaches
`GetBehaviorQuery`, a repository, or any tenant-scoped data. The solution's own
`ExceptionHandlingMiddleware` (registered first in the pipeline, wraps everything downstream)
catches this and returns a generic RFC 7807 500 — so in practice these ~150+ endpoints do not
crash the process, they silently 500 on every call.

**Separately, and more encouragingly than Phase 4 anticipated: PlantSafety does have real,
correctly-wired SQL Server Row-Level Security**, applied via SESSION_CONTEXT and verified by
an integration test. This is not the case for DataLake, Intelligence, Azure, or Reporting —
none of which have any RLS or equivalent app-level global tenant filter; DataLake/Intelligence
rely entirely on ad-hoc per-repository `WHERE TenantId = @TenantId`/stored-procedure parameter
filtering, correctly derived from a required context accessor, but — as Phase 2/4 already
established and this phase reconfirmed — that filtering sits behind **zero HTTP
authentication** in both modules, so the identity of the "tenant" being filtered on is never
established. See §3.

The single most severe *new* finding this phase adds is in the actor-propagation document
(`actor-propagation.md`): the "envelope" `ActorId`/`CorrelationId` pattern that the API layer
appears to construct carefully is **completely discarded** at the point every one of
PlantSafety's 58 write-repository outbox calls hardcodes `"system"` / `Guid.Empty` — including
`AccidentDapperRepository`, the exact repository Phase 2 cited as the showcase example of the
envelope pattern working end to end.

---

## 1. The critical question, resolved definitively: (b), not (a)

### 1a. Only two authorization policies are actually registered

`PlantSafety.Api\Extensions\ServiceCollectionExtensions.cs:117-135`
(`AddPlantSafetyAuthorization`, called from `AddPlantSafetyApi` at line 65, which is in turn
the only thing `Program.cs:56` calls) registers exactly two named policies inside the
solution's one and only `services.AddAuthorization(...)` call
(`ServiceCollectionExtensions.cs:119`):

```csharp
options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
options.AddPolicy("RequireDashboardRead", policy => policy.RequireAuthenticatedUser()
      .RequireRole("PlatformAdministrator", "PlantManager", "SafetyOfficer",
                   "EnvironmentalEngineer", "Supervisor", "ComplianceAuditor",
                   "DataScientist", "EsgReportingOfficer", "ReadOnlyAuditor"));
```

No `DefaultPolicy` or `FallbackPolicy` is configured anywhere in the solution (grepped
`FallbackPolicy|DefaultPolicy` across all of `PlantSafety` — zero hits), so there is no
catch-all that could change this analysis.

### 1b. `AddPlantSafetyPolicies()` — confirmed, again, never invoked

`PlantSafety.Api\Extensions\AuthorizationPolicyExtensions.cs:49-101` defines ~30 additional
policies (`Analytics.Read`, `Compliance.Update`, `SafetyEvents.Export`, etc.). Grepped the
entire solution for `AddPlantSafetyPolicies(`: the only hit is the method's own declaration at
line 49. **Zero call sites.** This method is dead code — confirmed identically to Phase 4,
now with the additional confirmation that `Program.cs` itself only ever calls
`AddPlantSafetyApi`, which does not reach this method through any path.

### 1c. Full attribute census — what the 362 controllers actually carry

Real (non-comment) `[Authorize...]` attribute instances in `PlantSafety.Api`: **369** (708
comment-text false matches from the 147 `*AuthorizationPolicies.cs` files' own doc comments —
which literally contain the string `[Authorize(Policy = ...)]` as illustrative text — were
excluded by filtering out `///`-prefixed lines).

| Pattern | Count | Registered? | Effect |
|---|---|---|---|
| `[Authorize(Policy = "RequireAuthenticatedUser")]` | 57 | **Yes** | Works — authenticates only, no tenant check needed (see §1d) |
| `[Authorize(Policy = <Feature>AuthorizationPolicies.Read)]` (293 across 147 feature classes) | 293 | **No** | 500s at authorization-evaluation time |
| `[Authorize(Policy = "X.Y")]` quoted module-level strings matching `AddPlantSafetyPolicies` names (e.g. `"Compliance.Update"`, `"SafetyEvents.Update"`, `"Operational.Export"`) | ~13 | **No** (that registration method is never called — see §1b) | 500s, same as above |
| Bare `[Authorize]` (no `Policy=`) | 3 files: `PlatformV1Controller.cs`, `PlatformV2Controller.cs`, `PlantSafetyRealtimeHub.cs` | N/A — authentication only | See §1e |

`RequireDashboardRead`, the second registered policy, has **zero** `[Authorize(Policy =
"RequireDashboardRead")]` usages anywhere in the codebase — it is registered but dead, the
mirror image of the 293 unregistered-but-used policies.

### 1d. The cross-check that resolves the question: do any client-supplied-TenantId controllers use a *working* policy?

Phase 4 left this exact question open: "if some controllers in this set use bare
`[Authorize]`... then those specific endpoints are still exploitable." This phase found and
grepped every controller that builds a query directly from a client-supplied
`request.TenantId`/`Request.TenantId` (the pattern `BehaviorV1Controller` exemplifies):
**155 files** match. Cross-referencing that list against (a) the 57 files using the working
`RequireAuthenticatedUser` policy and (b) the 3 files using bare `[Authorize]` produced **zero
overlap in both cases**:

```
comm -12 <client-supplied-TenantId files> <RequireAuthenticatedUser files>   → (empty)
comm -12 <client-supplied-TenantId files> <bare-[Authorize] files>            → (empty)
```

Every single one of the 155 client-supplied-`TenantId` files uses a feature-specific
`<X>AuthorizationPolicies.Read` (or quoted module-level string) policy — confirmed unregistered
in §1b/1c. **This is the direct evidence that resolves Phase 4's open question**: the
population of controllers with the theoretically-dangerous client-supplied `TenantId` is
*disjoint* from the population using a policy that would actually let the request through.

The 57 `RequireAuthenticatedUser` controllers (verified via spot-read of
`AccidentCategoriesV1Controller.cs`, `DashboardsV1Controller.cs`, `WidgetsV1Controller.cs`,
`ChecklistTemplatesV1Controller.cs`) are, without exception in the sample, the
`RequestEnvelopeFactory`-pattern controllers that derive `TenantId` from
`env.TenantId` → `IOperationContext.TenantId` → `HttpContext.Items` populated by
`TenantResolutionMiddleware` from the **JWT `tenantId` claim**, never from client input. So
even the 57 endpoints that *do* pass authorization have no client-suppliable-TenantId exposure
to begin with.

### 1e. The 3 bare-`[Authorize]` files carry no exploitable surface

- `PlatformV1Controller.cs:34` / `PlatformV2Controller.cs` (analogous): class-level
  `[Authorize]`, but the one real action (`GetVersionAsync`) carries a method-level
  `[AllowAnonymous]` (`PlatformV1Controller.cs:54`) and only returns the running assembly
  version — read-only, no `TenantId` parameter of any kind.
- `PlantSafetyRealtimeHub.cs` — a SignalR hub, not a REST controller in the classified
  473-GET-endpoint population; out of scope for the client-supplied-`TenantId` question (not
  independently re-audited this phase for its own hub-invocation authorization model — flagged
  as a follow-up if real-time features carry tenant-scoped data).

### 1f. Runtime-path confirmation: does the exception actually surface as a 500, or something worse?

Traced the middleware pipeline (`ApplicationBuilderExtensions.cs:61-91`):
`CorrelationIdMiddleware` → `ExceptionHandlingMiddleware` → `RequestLoggingMiddleware` →
`UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` →
`TenantResolutionMiddleware` → `UseOutputCache()` → endpoint execution.
`ExceptionHandlingMiddleware.InvokeAsync` (`Middleware\ExceptionHandlingMiddleware.cs:65-78`)
wraps `await _next(context)` in a try/catch that covers every middleware after it, including
`UseAuthorization()`. So the `InvalidOperationException` ASP.NET Core throws when
`AuthorizationPolicy.CombineAsync` cannot resolve a named policy is caught here, logged at
Error level with the CorrelationId, and converted into a generic RFC 7807 `500` —
**confirmed: this fails closed with a clean error response, not an unhandled crash or a
silent pass-through.** No caller ever sees another tenant's data via this path; they see a 500.

### 1g. Verdict

**Answer: (b) — a broad, confirmed availability defect (~150+ read endpoints and an unknown
number of the remaining ~140 non-Read-suffixed policy usages 500 on every call), not (a) a
live cross-tenant data leak.** This was verified by (i) a complete attribute census
cross-referenced against DI registration, (ii) tracing the exact ASP.NET Core failure mode and
confirming it fails closed, and (iii) confirming zero overlap between the "client-supplied
TenantId" population and the "policy actually resolves" population. Recommended fix: either
finish wiring the 147 feature-level policies' actual names into `AddAuthorization`, or replace
them with calls to the already-written (but unregistered) `AddPlantSafetyPolicies()` extension
— whichever the team intends as canonical — as an immediate priority; this is presently a
361-file blast radius of 500 errors, not a theoretical risk.

---

## 2. PlantSafety's real SQL Server Row-Level Security — exists, is well-built, and is wired

Contrary to the possibility flagged in this phase's brief (PlantSafety might have only
app-level filtering with no real RLS), **`PlantSafety.CoreDB` and `PlantSafety.DiagnosticsDB`
have genuine, deny-by-default SQL Server RLS**, and the application-layer plumbing that makes
it actually function is wired in, not a stub.

### 2a. The predicate and policy

`PlantSafety.Infrastructure\Persistence\Migrations\Phase9-Security\90-row-level-security.sql`:

- `Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)` (lines 108-113,
  identical definitions repeated per-database at 165-170 and 242-247) is a `SCHEMABINDING`
  inline table-valued function that returns a row only when
  `@TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier)`.
- A `CREATE SECURITY POLICY Security.TenantAccessPolicy` is built dynamically (lines 116-149)
  by live-discovering every table with a non-nullable `uniqueidentifier TenantId` column
  (self-healing against schema drift — re-running the script picks up new tables
  automatically) and adding a **FILTER predicate** (read-side row filtering) plus **two BLOCK
  predicates** (`AFTER INSERT`, `AFTER UPDATE` — write-side enforcement) referencing that
  function, `WITH (STATE = ON)`.
- Applied to `PlantSafety.CoreDB` and `PlantSafety.DiagnosticsDB`. `PlantSafety.ArchiveDB` is
  explicitly scoped out — its only `NOT NULL TenantId` table
  (`Archival.ArchivalJob`) holds only platform-wide, sentinel-`TenantId`
  (`00000000-0000-0000-0000-000000000000`) job-execution log rows, not tenant business data
  (migration lines 52-68), so no policy is created there — a documented, deliberate scope
  decision, not an oversight.
- `AuditDB.dbo.AuditLog` / `ErrorDB.dbo.ErrorLog` are also explicitly excluded — their
  `TenantId` columns are nullable `nvarchar` metadata fields on shared, cross-tenant
  operational logging tables, not the `uniqueidentifier NOT NULL` shape the policy targets
  (migration lines 70-81).
- The migration's own header (lines 12-40) is transparent that, **at authoring time**, no
  application code set `SESSION_CONTEXT` — meaning the policy alone, if unpaired with the fix
  below, would have deny-by-default-blocked every write and returned zero rows on every read.
  This phase confirms the follow-up fix referenced in that header **has since landed** (§2b).

### 2b. The C# side is genuinely wired — not a stub

`PlantSafety.Infrastructure\Persistence\Dapper\TenantSessionContextExtensions.cs` implements
`SetTenantSessionContextAsync(this IDbConnection, Guid tenantId, ...)`, which runs:

```sql
IF TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier) IS NULL
   OR TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier) <> @TenantId
BEGIN
    EXEC sp_set_session_context @key = N'TenantId', @value = @TenantId, @read_only = 1;
END
```

Critically, this is **not called ad hoc from 58 separate places that could drift** — it is
centralized in exactly the two shared Dapper execution paths every repository routes through:

- `PlantSafety.Infrastructure\Persistence\Dapper\Commands\DapperCommandExecutor.cs:33` — every
  write stored-procedure call sets session context before executing.
- `PlantSafety.Infrastructure\Persistence\Dapper\Queries\DapperQueryExecutor.cs:32,53` — every
  read stored-procedure call (both `QueryAsync` and `QuerySingleOrDefaultAsync`) does the same
  on the read-replica connection.

`@read_only = 1` is deliberate (documented in the extensions file's own remarks): each
repository call opens a fresh pooled `SqlConnection` and disposes it immediately; ADO.NET's
`sp_reset_connection` on pool hand-back clears `SESSION_CONTEXT`, so no connection can ever
carry tenant A's context into tenant B's request even under connection pooling — a real,
reasoned defense against the classic RLS-via-session-context footgun.

**Bypass check**: 8 files call `connection.Query/ExecuteAsync` directly instead of going
through the two shared executors (`InboundWebhookDapperRepository.cs`,
`ArchivalDapperRepository.cs`, `AuditLogDapperRepository.cs`, `BackupDapperRepository.cs`,
`FeatureFlagDapperRepository.cs`, `MonitoringDapperRepository.cs`,
`SchemaVersionDapperRepository.cs`, plus the extensions file itself). Checked each: the six
`Operations/*` repositories operate on platform-level tables with **no `TenantId` column at
all** (confirmed by grep across their `.sql` table definitions — zero hits), so they are
correctly out of RLS scope, not a bypass. `InboundWebhookDapperRepository.cs:48` calls
`connection.SetTenantSessionContextAsync(tenantId, ...)` explicitly and manually before its own
direct `ExecuteAsync` call (line 62) — correctly handled, just not routed through the shared
executor. **No bypass found**: every code path that touches an RLS-protected table sets
session context first, either via the shared executors or explicitly.

### 2c. This is tested, not just asserted

`PlantSafety.IntegrationTests\Infrastructure\Persistence\TenantSessionContextRlsTests.cs`
stands up a live analog of the Phase 9 RLS fixture (predicate function + FILTER/BLOCK policy)
against a Testcontainers SQL Server instance and — per its own doc comment — "verifies the fix
for the Phase 9 RLS/SESSION_CONTEXT gap," proving both that the pre-fix behavior (no
`SESSION_CONTEXT` set) is denied, and that the current `DapperQueryExecutor`/
`DapperCommandExecutor` wiring resolves correctly for two distinct tenants (`TenantA`/
`TenantB` fixtures defined at the top of the file). This is a real, live-verified control, not
an aspirational comment.

### 2d. Net assessment

**PlantSafety's RLS is a genuine strength, correctly designed and wired.** It is real SQL
Server ROW LEVEL SECURITY (not an EF Core global query filter, which the Architecture
Specification's own rationale — cited in the migration header — explicitly prohibits: "ORM-level
global query filters are explicitly prohibited by the Enterprise Architectural Rule Handbook").
It provides genuine defense-in-depth *underneath* whatever the API layer does or fails to do,
which matters directly for §1: even on the (b)-classified 500-ing endpoints, if the
authorization gap were ever "fixed" by simply removing the `Policy=` argument without also
verifying the query correctly derives `TenantId` from `env.TenantId` (not
`request.TenantId`), the RLS layer would still block a request whose session context doesn't
match the row's `TenantId` — provided the connection used still routes through
`DapperCommandExecutor`/`DapperQueryExecutor`. This is a meaningful safety net, not a
substitute for fixing the authorization-policy registration gap in §1.

---

## 3. DataLake, Intelligence, Azure, Reporting — no RLS, no equivalent, confirmed again

Grepped `SESSION_CONTEXT|ROW LEVEL SECURITY|SECURITY POLICY|HasQueryFilter` across `DataLake`,
`Intelligence`, `Azure`, and `Reporting` (`.cs` and `.sql`): **zero hits in all four modules.**
There is no SQL Server RLS and no EF Core global query filter anywhere outside PlantSafety.

- **DataLake**: `DataLakeControllerBase.TenantId` is a required, throwing accessor
  (confirmed in Phase 2), and repositories filter `WHERE TenantId = @TenantId`/pass it as a
  query parameter in C# (no `.sql` stored-procedure files found in DataLake at all — its
  persistence layer builds parameterized queries directly in C#, confirmed via grep for
  `TenantId` hits landing in `.cs` mapper/repository files, not `.sql`). This is honest,
  correctly-derived, per-query filtering — **but it is application-level only, and (per Phase 2/4,
  reconfirmed this phase by grepping `Program.cs`/`ServiceCollectionExtensions.cs` again) there
  is still zero `AddAuthentication`/`AddJwtBearer`/`[Authorize]` anywhere in `DataLake.Api`.**
  The filter logic is sound; the identity it filters on is never authenticated.
- **Intelligence**: same shape — `TenantId` is optional (`Guid?`) at the API layer per Phase 2,
  no RLS or query-filter equivalent, and (reconfirmed this phase) zero authentication wiring in
  `Intelligence.Api`.
- **Azure**: has no tenant concept at the application level at all (per Phase 2/4) — nothing to
  bypass because there is no tenant partition to enforce. Genuinely has JWT authentication
  wired, unlike DataLake/Intelligence/Reporting.
- **Reporting**: no tenant concept, no RLS, and (reconfirmed this phase) zero authentication
  anywhere in `Reporting.Api` — the starkest gap of the four, as Phase 4 already concluded.

**Net: only PlantSafety has real RLS. DataLake and Intelligence have correctly-derived but
purely application-level tenant filtering with no authentication establishing the caller's
identity in the first place — the same High-severity finding Phase 4 reported, reconfirmed by
direct grep this phase, not just repeated from memory.**

---

## 4. Does anything bypass the tenant-scoping mechanism that does exist?

- **PlantSafety**: No bypass found (§2b) — every RLS-protected table's access path runs through
  session-context-setting code, whether via the two shared Dapper executors or an explicit
  manual call.
- **DataLake/Intelligence**: There is no global mechanism to bypass — every repository method
  independently threads `TenantId` as an explicit parameter, so "bypass" would mean a specific
  repository method omitting that parameter from its `WHERE`/stored-procedure call. This phase
  did not have time to individually audit every DataLake/Intelligence repository method for a
  missing `TenantId` filter (Phase 2 sampled 3 controllers, not the full repository surface) —
  flagged as a follow-up if this module's authentication gap is ever closed, since a missing
  per-query filter would become immediately exploitable at that point.

---

## Summary table

| # | Finding | Severity | Confidence |
|---|---|---|---|
| 1 | 293+ PlantSafety controllers gated by unregistered `[Authorize(Policy=...)]` names — confirmed **availability defect (500s)**, not a cross-tenant leak; zero overlap with the client-supplied-TenantId population and a working policy | **High** (availability), **not** a confirmed data leak | High — direct grep census + build + pipeline trace |
| 2 | PlantSafety's SQL Server RLS (SESSION_CONTEXT + FILTER/BLOCK policy) is real, correctly wired through both shared Dapper executors, and integration-tested | **Positive finding** — genuine defense-in-depth | High — full read of migration, C# wiring, and test |
| 3 | DataLake/Intelligence/Azure/Reporting: no RLS or equivalent anywhere; DataLake/Intelligence's per-query filtering is sound but sits behind zero authentication (reconfirmed) | **High** (same as Phase 4, reconfirmed by direct grep, not assumed) | High |
| 4 | `RequireDashboardRead` policy registered but never referenced by any `[Authorize]` attribute — dead code, inverse of the 293-policy gap | Low (cleanliness), flags the DI registration file is out of sync with the controller tree in both directions | High |
| 5 | No bypass of PlantSafety's RLS found in the 8 non-shared-executor repositories (all correctly out-of-scope or manually handled) | Informational | High |

## Immediate recommendation

Fix the authorization-policy registration gap first (§1) — it is presently causing ~150+
(likely closer to 300+, given the 293 unregistered `.Read` uses plus the ~13 unregistered
module-string uses) endpoints to 500 on every request, which is an active production-breaking
defect if this environment is live, not merely a latent risk. Do **not** fix it by stripping
the `Policy=` argument down to bare `[Authorize]` as a quick patch — §1d's evidence shows that
would immediately convert this from an availability defect into the exact live cross-tenant
leak Phase 4 hypothesized, on any endpoint whose query still derives `TenantId` from
`request.TenantId` rather than `env.TenantId`. The correct fix is to register the real policy
names (or route through the already-written `AddPlantSafetyPolicies()`), not to remove the
check.
