# Lifecycle Security Audit — Does PlantSafety's RLS Actually Cover the F1 Gap?

Phase 7 of 10, Lifecycle Coverage Audit. Root: `C:\Users\dross\source`. Date: 2026-07-26.
Builds on `rule-enforcement/_shared-findings.md` F1 (no transition checks the caller's tenant
against the aggregate's TenantId; repositories load by aggregate ID alone — confirmed across
every module) and the completed Ingestion Pipeline Discovery's `docs/ingestion/security/
rls-audit.md` (which characterized PlantSafety's SQL Server RLS as "a genuine strength,
correctly designed and wired," based on spot-reads of write-repository `AddAsync` methods,
list-by-tenant reads, and a synthetic integration test).

---

## Bottom line, stated first — and it is bluntly worse than the prior phase's framing

**RLS does not close the F1 gap. For the exact call shape F1 already flagged — single-aggregate
load-by-ID and the paired update-by-ID that every lifecycle transition (Close, Approve, Escalate,
Archive, etc.) is built from — SQL Server's SESSION_CONTEXT('TenantId') is never populated at
all, because the mechanism that populates it (`TenantSessionContextExtensions.
SetTenantSessionContextAsync`, called from `DapperQueryExecutor`/`DapperCommandExecutor`) reads
the tenant id back out of the very same `DynamicParameters` object the repository method itself
builds — and the classic-pattern `GetByIdAsync(id)` / `UpdateAsync(aggregate)` methods used by
the majority of PlantSafety's 48 rich aggregates never put a `TenantId` key into that parameter
set in the first place.**

This is not a "SESSION_CONTEXT might be wrong or spoofable" risk (question 2's framing). It is
sharper and worse: **for this call shape, SESSION_CONTEXT is not wrong — it is never set at all,
by construction, on every single call.** The prior phase's "genuine strength" conclusion was built
entirely from spot-reads of shapes that *do* carry a `TenantId` parameter (`AddAsync`/`Insert`,
list-by-tenant reads, and a synthetic `RlsTest.Widgets` fixture that was seeded with `TenantId` as
an explicit parameter on every call). None of those spot-reads happened to be the specific
load-by-ID-alone shape F1 is about. This phase read that exact shape directly and it does not
engage RLS.

**Real-world consequence, two possible states, and this phase cannot distinguish which is live
without a running SQL Server instance (static analysis only):**

- **If migration `90-row-level-security.sql` is actually applied and enabled against the live
  `PlantSafety.CoreDB`/`DiagnosticsDB`**, then every `GetByIdAsync`/`UpdateAsync` call built the
  way `AccidentDapperRepository`, `RiskDapperRepository`, and (by the same code-generation
  template) the bulk of the other 46 rich aggregates are built, opens a **fresh** connection with
  **no** SESSION_CONTEXT set, and the FILTER predicate (`@TenantId = TRY_CAST(SESSION_CONTEXT(...)
  AS uniqueidentifier)`, false when the right side is `NULL`) silently matches **zero rows** —
  meaning `GetByIdAsync` always returns `null` and `UpdateAsync` always affects 0 rows, for every
  tenant, unconditionally. That is a solution-breaking availability bug, not a tenant-isolation
  control — and neither the repository code nor the calling command handler checks the affected
  row count, so it would fail **silently** (no exception, no log line beyond a debug-level
  `LogDebug`), which is precisely why it would not have been noticed by casual testing.
- **If the migration has been authored but not actually deployed/enabled against whatever
  database the running application currently points at** (plausible: the migration's own header
  comment says it shipped with "0 live rows... safe to ship now" and frames SESSION_CONTEXT wiring
  as a required, separately-landed follow-up — this phase confirms the follow-up code exists but
  cannot confirm from source alone that the *migration itself* is currently applied) — then there
  is **no SQL Server enforcement running at all** on these tables today, and F1 stands completely
  undiminished: the aggregate-ID-alone load/update path has **zero** defense-in-depth from either
  layer, exactly as F1 already said, with RLS contributing nothing because it was never wired to
  engage for this shape regardless of migration state.

**Either way, the answer to the question this phase was asked to resolve is: RLS does NOT provide
real protection for the F1-flagged operations.** It only engages for the call shapes that happen
to carry an explicit `TenantId` parameter (creates, list-by-tenant reads, and a second,
newer-generation repository pattern under `Features/*` — see §4) — which is a different,
non-overlapping population from the one F1 is about.

---

## 1. How SESSION_CONTEXT population actually works — traced to the exact mechanism

`PlantSafety.Infrastructure\Persistence\Dapper\TenantSessionContextExtensions.cs:99-104`:

```csharp
public static Guid GetTenantId(this DynamicParameters parameters)
{
    return parameters.ParameterNames.Contains(TenantIdParameterKey)
        ? parameters.Get<Guid>(TenantIdParameterKey)
        : Guid.Empty;
}
```

and lines 75-91:

```csharp
public static async Task SetTenantSessionContextAsync(this IDbConnection connection, Guid tenantId, ...)
{
    if (tenantId == Guid.Empty) { return; }   // no-op
    await connection.ExecuteAsync(new CommandDefinition(SetSessionContextSql, new { TenantId = tenantId }, ...));
}
```

`DapperCommandExecutor.ExecuteAsync` (`Persistence\Dapper\Commands\DapperCommandExecutor.cs:37`)
and both `DapperQueryExecutor` methods (`Persistence\Dapper\Queries\DapperQueryExecutor.cs:32,55`)
call `connection.SetTenantSessionContextAsync(parameters.GetTenantId(), ...)` **before** executing
the stored procedure — confirming the Ingestion Discovery's finding that this wiring is real and
centralized in exactly two chokepoints, not ad hoc. What that prior audit's spot-checks did not
surface is that **`GetTenantId()`'s only source of truth is whether the calling repository method
happened to add a `"TenantId"` key to its own `DynamicParameters`** — there is no ambient
`ICurrentTenantProvider`/`ITenantContextAccessor` in the Infrastructure layer (the file's own doc
comment says so explicitly, `TenantSessionContextExtensions.cs:23-25`). If the repository method
doesn't add that key, `GetTenantId()` returns `Guid.Empty`, and `SetTenantSessionContextAsync`
silently no-ops — **on a brand-new connection that starts with SESSION_CONTEXT unset.**

## 2. The exact F1 call shape, confirmed directly, in the exact aggregates G2 already named

`PlantSafety.Infrastructure\AccidentManagement\Persistence\AccidentDapperRepository.cs:41-54`
(`GetByIdAsync`):

```csharp
var parameters = new StoredProcedureBuilder().AddGuid("AccidentId", id.Value).Build();
var row = await queryExecutor.QuerySingleOrDefaultAsync<AccidentRow>(
    "[AccidentManagement].[usp_Accident_GetById]", parameters, cancellationToken);
```

No `TenantId` parameter anywhere in this call. `usp_Accident_GetById.sql:15-28` confirms the
proc's own signature takes only `@AccidentId` and selects `WHERE AccidentId = @AccidentId AND
IsDeleted = 0` — the proc body has no tenant filter of its own either (consistent with F1); it was
relying entirely on RLS to be the tenant boundary, and RLS never engages here.

The same repository's `UpdateAsync` (lines 130-154) builds its parameter set from `AccidentId`,
`Status`, `Category`, ... `UpdatedBy` — again **no `TenantId` key**. The one place `TenantId` does
appear in this file is the nested `CorrectiveAction` upsert loop (line 162), and the `AddAsync`
(Create) path (line 88) — both of which are shapes RLS *does* engage for, because they're
`INSERT`/child-entity paths that need `TenantId` for their own column values anyway, not because
of any deliberate RLS-awareness.

Re-confirmed identically in `RiskManagement\Persistence\RiskDapperRepository.cs:70-72`
(`GetByIdAsync` → `new StoredProcedureBuilder().AddGuid("RiskId", id.Value).Build()`, no
`TenantId`) and `Auditing\AuditFinding\Persistence\AuditFindingDapperRepository.cs:47-48` (same
shape, despite this repository's `UpdateAsync` correctly using `.WithCanonicalHeaders(aggregate.
TenantId, ...)` for the write side at line 85/124 — the read-by-id path is the one that's blind).

A full grep for `GetByIdAsync`/`FindAsync` across every `*DapperRepository.cs` file found **140
occurrences across 81 files**. Cross-referencing against `WithCanonicalHeaders` usage (60
occurrences across 37 files, and — critically — `WithCanonicalHeaders` is only ever chained onto
**write** calls like `AddAsync`/`UpdateAsync` in every file checked, never onto a `GetByIdAsync`)
confirms this is the dominant, not exceptional, shape: **the "classic" repository pattern used by
Accident, Risk, Scorecard, SafetyPolicy, all four PermitToWork sub-aggregates, all five PPE
sub-aggregates, all five Observability sub-aggregates, all five Scheduling sub-aggregates, and
AuditFinding/AuditEngagement (i.e. the exact 23-aggregate G2 population plus more) loads and
updates by aggregate ID alone, with no `TenantId` parameter on either call — meaning RLS's own
session-context wiring never fires for any of them.**

## 3. Not every repository is built this way — a second, newer pattern exists that DOES pass TenantId

A second repository generation, concentrated under `PlantSafety.Infrastructure\Features\*`
(`Observation`, `NearMiss`, `CorrectiveAction`, `Permit` (Compliance), `ManagementOfChange`,
`EnterpriseFabric`, `DigitalTwin`, `AiGovernance`, `Sustainability`, and ~15 others — 25 files
confirmed by direct read), takes `tenantId` as an **explicit method parameter** on `GetByIdAsync`
and threads it into the stored-procedure call, e.g.
`Features\SafetyEvents\Observation\Repositories\ObservationDapperRepository.cs:98-108`:

```csharp
public async Task<ObservationReadModel?> GetByIdAsync(Guid tenantId, Guid observationId, ...)
{
    var parameters = new StoredProcedureBuilder()
        .AddGuid("TenantId", tenantId).AddGuid("ObservationId", observationId).Build();
    ...
}
```

For this shape, RLS's session-context mechanism *does* engage correctly. **But this is a hollow
positive**: per `sql-shape/_shared-findings.md` G1, several of the exact aggregates using this
pattern (`Observation`, `EmergencyManagement`, `IncidentManagement` in SafetyEvents;
`Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`, `Procedures`,
`RegulatoryCalendar` in Compliance) are **confirmed zero-implementation** for their actual domain
write-repository interface — meaning the class exhibiting correct RLS behavior here may not even
be the one wired into the application's DI container as the real repository (see G2's
"naming-collision trap" — a same-named class implementing an unrelated read-model interface).
Where that's the case, correct RLS engagement on a class nobody calls is not a mitigating control;
it's a coincidence.

**Net for §3: RLS engagement correctness correlates with which repository-generation template an
aggregate happened to be scaffolded from, not with any deliberate security review — and the newer
template that gets it right disproportionately overlaps with the aggregates that don't have a
working write path at all.**

## 4. Is the RLS predicate's source client-influenceable? (Question 2, direct answer)

**No — SESSION_CONTEXT is set server-side, from a value the repository method already has in
hand (either `aggregate.TenantId` read back off the domain object being persisted, or an explicit
`tenantId` parameter passed down from the command handler); a client cannot write directly into
SESSION_CONTEXT, and `SetTenantSessionContextAsync`'s SQL is parameterized (`@TenantId`), not
string-built, so there is no injection vector into the predicate function itself.** The real risk
is not "a client can forge SESSION_CONTEXT" — it's the §1/§2 finding: **for a large, specifically-
identified population of calls, nothing ever sets it at all**, which is a strictly different (and,
per §1's two branches, either equally or more severe) failure mode than spoofing. Separately, per
`docs/ingestion/security/rls-audit.md` §1d, the population of PlantSafety controllers that accept
a **client-supplied** `TenantId` (155 files) is disjoint from the population using a working
authorization policy today — but if that authorization gap is ever fixed naively, any of those 155
controllers whose repository call *does* carry a TenantId parameter would feed the client-supplied
value straight into both the app-layer filter and SESSION_CONTEXT identically, meaning RLS would
faithfully enforce whatever tenant the client claims to be — not a bypass of RLS, but RLS
correctly protecting the wrong boundary, because the value it trusts was never authenticated. This
reconfirms (does not re-litigate) the ingestion phase's own recommendation not to fix the
authorization gap by stripping the `Policy=` argument.

## 5. DataLake, Intelligence, Azure, Reporting — reconfirmed, no new finding

Reconfirmed by grep this phase (`SESSION_CONTEXT|ROW LEVEL SECURITY|SECURITY POLICY|
HasQueryFilter` across `DataLake/`, `Intelligence/`, `Azure/`, `Reporting/`): zero hits, consistent
with the ingestion discovery. No base repository class, EF query filter, or other compensating
control was found in place of RLS in any of these four modules — DataLake/Intelligence's
per-repository `WHERE TenantId = @TenantId` filtering is real (confirmed by the Ingestion
Discovery, not re-derived here) but is the **only** control in these modules, app-layer only, and
— per F1/G1 — not itself verified against the authenticated caller's identity, because none of
these three modules (Reporting has no aggregates at all) have any authentication wired in the
first place. There is nothing here for this phase to add beyond citing it: **wide open at both
layers, exactly as F1 and the ingestion discovery already concluded.**

## 6. Does fixing the ~300-endpoint auth-policy-500 bug close the tenant-isolation gap? (Question 4)

**No — fully orthogonal, and this phase's findings make the two problems' independence sharper,
not weaker, than the ingestion discovery already stated.** Three distinct layers each have to be
separately correct for tenant isolation to actually hold, and registering the missing
authorization policies only touches the first:

1. **Authentication/authorization** (the 500 bug): confirms the caller is *a* valid, authenticated
   user of *some* tenant. Fixing this only lets requests reach the handler; it says nothing about
   which tenant's data the handler then touches.
2. **App-layer tenant scoping**: per F1, unenforced for aggregate-ID-alone load/update transitions
   across every module examined, including the ones with a real repository implementation.
3. **RLS**: per this document's §1-§2, never engages for that exact same population, regardless of
   whether layer 1 or 2 is fixed.

Naively "fixing" layer 1 alone (registering the policies as currently named) would let previously-
500ing requests through to handlers that still have neither of layers 2 or 3 protecting them for
GetById/Update-shaped transitions — meaning a same-tenant-authenticated caller who supplies another
tenant's aggregate ID (reachable today only by luck/effort, since the request currently 500s before
reaching that code) would, post-fix, successfully read or mutate it: no F1 check, and (per this
document) no RLS engagement either. **Fixing the availability bug does not merely fail to close
the tenant-isolation gap — for this specific transition shape, it is the step that makes the gap
newly reachable**, since today's 500 response is, incidentally, also a (accidental, availability-
motivated, not designed) tenant-isolation control for these 293+ endpoints.

## Summary table

| # | Finding | Severity | Confidence |
|---|---|---|---|
| 1 | SESSION_CONTEXT population depends entirely on the calling repository method including a `"TenantId"` DynamicParameters key; the classic `GetByIdAsync(id)`/`UpdateAsync(aggregate)` shape used by ~23+ of PlantSafety's richest aggregates (Accident, Risk, SafetyPolicy, Scorecard, Permit\*, PPE\*, Observability\*, Scheduling\*, AuditFinding) never includes that key | **Critical** — either a silent, solution-wide availability bug (if RLS is live) or a fully unmitigated F1 (if it isn't) | High — direct read of repository code, proc SQL, and the session-context extension's own source |
| 2 | A second, newer repository template (`Features/*`) does pass TenantId correctly on GetById, but disproportionately overlaps with aggregates already confirmed to have zero real domain-repository implementation (sql-shape G1) | Medium — a correct control on possibly-dead code is not a working control | High |
| 3 | SESSION_CONTEXT is not client-spoofable directly (parameterized, server-set) — the risk is omission, not forgery | Informational, corrects question 2's framing | High |
| 4 | DataLake/Intelligence/Azure/Reporting: no RLS or compensating control, reconfirmed, no change from ingestion discovery | High (unchanged) | High |
| 5 | The auth-policy-500 fix and the tenant-isolation gap are orthogonal; naively fixing the former makes the F1/RLS gap newly *reachable* for the previously-500ing 293+ endpoints, rather than closing it | **Critical** — sequencing risk for whoever fixes #1 in `docs/ingestion/security/rls-audit.md` | High |

## Recommendation

Before registering the missing authorization policies (the ingestion discovery's "immediate
priority"), audit and fix the GetById/Update call shape to carry `TenantId` through to both the
app-layer check (F1) and the stored-procedure parameter set (so RLS can actually engage) for at
least the 23 audit-logged aggregates named in G2 and, ideally, the full 46+ classic-pattern rich
aggregates identified in §2 — otherwise fixing the availability bug first will convert an
inconvenience into the live cross-tenant exposure everyone has been correctly worried was already
there.

## 7. Addendum (2026-07-26) — concrete first step, and a new regression test

This document's §1 already establishes the two possible states (RLS deployed vs. not) and that
static analysis alone cannot distinguish them. That ambiguity is cheap to resolve and does not
require writing new test infrastructure: **run the verification query that already exists at the
tail of the migration itself**, `PlantSafety.Infrastructure\Persistence\Migrations\Phase9-Security\
90-row-level-security.sql:289-297`, against the actual target database:

```sql
SELECT 'PlantSafety.CoreDB' AS DatabaseName, sp.name AS PolicyName, sp.is_enabled,
    (SELECT COUNT(*) FROM [PlantSafety.CoreDB].sys.security_predicates) AS PredicateCount
FROM [PlantSafety.CoreDB].sys.security_policies sp;
```

If `Security.TenantAccessPolicy` is present and `is_enabled = 1`, the codebase is in **Scenario
A**: `GetByIdAsync`-shaped calls against RLS-covered tables are returning zero rows / silently
blocking writes for every tenant right now — a live availability incident, not a dormant risk. If
no such policy exists, the codebase is in **Scenario B**: no RLS enforcement is running at all
today, and F1 stands with zero defense-in-depth from either layer. Either result determines the
correct next move; guessing does not.

**New regression coverage added this session**: `PlantSafety.IntegrationTests\Infrastructure\
Persistence\TenantSessionContextRlsTests.cs` now includes
`GetByIdShape_NoTenantIdParameter_ReturnsNoRows_EvenForTheOwningTenant`, which reproduces
`AccidentDapperRepository.GetByIdAsync`'s exact parameter shape (id-only, no `TenantId`) against
the existing `RlsTest.Widgets`/`fn_TenantAccessPredicate_RlsTest` fixture. It seeds a row under its
correct owning tenant via the working insert path, then proves that a GetById-shaped read of that
same row — with no `TenantId` parameter — returns `null`, i.e. **even the legitimate owning tenant
cannot retrieve its own row through this call shape once RLS is active**. This is the concrete,
CI-enforced proof that §1's "silent availability bug, not a tenant-isolation control" framing is
correct, and it will fail (catching a regression) if this exact gap is ever accidentally
reintroduced after the underlying repositories are fixed to carry `TenantId` through.

The root fix remains: update `AccidentDapperRepository.GetByIdAsync` and the ~46 other
classic-pattern repositories named in §2 to accept and thread `TenantId` through to both the
app-layer check and the stored-procedure parameter set, then re-run this test suite to confirm
correct scoping, no cross-tenant access, and no silent outage under RLS — only after which
registering the ~293 missing authorization policies becomes safe.
