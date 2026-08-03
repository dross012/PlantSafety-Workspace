# Cross-Cutting Recommendations — Phase 9 of 10 (Lifecycle Coverage Audit)

Root: `C:\Users\dross\source`. Date: 2026-07-26. This document covers the fixes that are genuinely
solution-wide/architectural — a single engineering task that happens to touch dozens or hundreds of
aggregates identically, as opposed to a per-aggregate defect. Every `recommendations/<module>.md`
file cross-references these items by ID (XC-1 … XC-6) instead of restating them.

**Read this document's "Required sequencing" section before touching any of XC-1/XC-2/XC-3.** It is
the single most consequential ordering decision in the entire recommendations set: doing these in
the wrong order converts a availability bug into a live cross-tenant data exposure.

---

## Required sequencing among the four security/attribution cross-cutting items

The audit surfaced four items that look, superficially, like four independent backlog tickets a
team could pick up in any order. **They cannot be done in any order.** Three of them have a hard
dependency chain; the fourth is independent and can run in parallel with any of them.

```
XC-1 (Tenancy check + RLS engagement)  ──must land and be verified before──▶  XC-3 (Auth-policy registration)

XC-2 (Actor/correlation propagation)  ──independent, no ordering dependency on XC-1/XC-3, run any time / in parallel──
```

**Recommended fix order: XC-1 first (and verified), then XC-3. XC-2 can run before, during, or
after either — schedule it in parallel to use engineering capacity, not because it's lower
priority.**

### Why this order, explicitly

Today, ~293+ PlantSafety endpoints 500 on every call because `AddPlantSafetyAuthorization()` never
registers the 147 feature-level authorization policies referenced by `[Authorize(Policy=...)]`
attributes (`security/rls-audit.md` §6, inherited from the Ingestion Discovery's own
`docs/ingestion/security/rls-audit.md`). That 500 is a bug — but it is **incidentally also today's
only tenant-isolation control** for the GetById/Update transition shape, because:

- The app layer never checks the caller's TenantId against the loaded aggregate's TenantId for any
  transition (`gaps/_severity-ranking.md` #5, `domain-mapping/_shared-findings.md` F1).
- SQL Server RLS's `SESSION_CONTEXT('TenantId')` is never populated for the same call shape, because
  `TenantSessionContextExtensions.GetTenantId()` only finds a `TenantId` key in the
  `DynamicParameters` set the classic `GetByIdAsync(id)`/`UpdateAsync(aggregate)` repository methods
  never add — confirmed directly in `AccidentDapperRepository`, `RiskDapperRepository`,
  `AuditFindingDapperRepository`, `PermitToWorkDapperRepository`, and pattern-inferred across the
  remaining ~40+ classic-pattern aggregates (`security/rls-audit.md` §1-2).

If the ~293-endpoint 500 bug is fixed **first** (naive fix: just register the policies as currently
named), previously-blocked requests reach these handlers with **zero** tenant isolation of any kind
— a same-tenant-authenticated caller who supplies another tenant's aggregate GUID would now
successfully read or mutate it, where today they get a 500 instead. **This is the single finding in
the whole audit where the "obviously correct" fix makes the security posture strictly worse if done
out of order** (`security/rls-audit.md` §6, `gaps/_severity-ranking.md` #1).

XC-2 (actor/correlation propagation) does not interact with this chain at all — it is a forensic/
compliance defect (nobody can answer "who did this"), not a tenant-isolation defect. Fixing it
before, during, or after XC-1/XC-3 has no effect on the tenant-isolation risk in either direction,
so it should be scheduled based on engineering capacity, not blocked on the other three.

---

## XC-1. App-layer tenancy check + RLS engagement (combined fix — same root cause)

**Category**: Security (cross-tenant data exposure) + Availability (bimodal, see below).
**Cites**: `gaps/_severity-ranking.md` #2 (Tier 0) and #5 (Tier 1); `domain-mapping/_shared-findings.md`
F1; `security/rls-audit.md` §1-2; `rule-enforcement/_shared-findings.md` G1.
**Scope**: every mutating transition built on the "classic" `GetByIdAsync(id)`/`UpdateAsync(aggregate)`
repository shape — confirmed directly in 5 aggregates (Accident, Risk, MachineGuarding,
PermitWorkflow/`PermitToWorkDapperRepository`, `AuditFindingDapperRepository`'s read path),
pattern-inferred for the remaining ~40+ of PlantSafety's 46+ classic-pattern rich aggregates
(all 23 audit-logged aggregates plus the ~35-aggregate Bug#1-only cluster's write-repositories).
Does **not** apply to the newer `Features/*` repository generation (already passes `tenantId`
explicitly — though see XC-4, several of those interfaces have no live implementing class anyway),
Create-only aggregates (their `AddAsync`/Create path already carries TenantId), or the 13
zero-implementation aggregates (nothing to fix until XC-4 lands for them).

**Problem, restated precisely**: two independent enforcement layers are both blind to the identical
call shape for a different mechanical reason each:
1. **App layer**: no command handler checks `caller.TenantId == loadedAggregate.TenantId` before
   mutating. Isolation today depends entirely on `AggregateId` GUIDs being unguessable.
2. **SQL layer**: `TenantSessionContextExtensions.SetTenantSessionContextAsync` only fires when the
   calling method's own `DynamicParameters` includes a `"TenantId"` key. The classic repository
   shape's `GetByIdAsync`/`UpdateAsync` methods build their parameter set from the aggregate's own ID
   only — never a `TenantId` key — so RLS's SESSION_CONTEXT is never set on this call shape,
   regardless of whether the RLS migration (`90-row-level-security.sql`) is live.

**Bimodal severity note carried over from the audit**: if the RLS migration is live against the
running database, this defect is *also* a silent, solution-wide availability bug (every
`GetByIdAsync` returns null, every `UpdateAsync` affects 0 rows, for every tenant, unconditionally,
with no exception and no error-level log). If the migration is not live, it is "only" a completely
unmitigated F1 tenancy gap. **Determine which state is live against the actual running database as
the first step of this fix** — this cannot be resolved from source alone.

**Concrete fix**:
1. Add an explicit `tenantId` parameter to every classic-pattern repository's `GetByIdAsync`/
   `UpdateAsync` method signature (mirroring the `Features/*` generation's already-correct pattern,
   e.g. `ObservationDapperRepository.GetByIdAsync(Guid tenantId, Guid id, ...)`).
2. Thread the caller's authenticated TenantId (from the command envelope, never client input) into
   that parameter at every call site — command handler → repository call.
3. Add the `TenantId` key to the `StoredProcedureBuilder`/`DynamicParameters` set for these calls
   so `SetTenantSessionContextAsync` actually populates SESSION_CONTEXT (closes the RLS half).
4. In the same handler, add an explicit `if (aggregate.TenantId != callerTenantId) throw
   ForbiddenException` check immediately after load, before any mutation (closes the app-layer half;
   do not rely on RLS alone as defense-in-depth per the audit's own two-layer framing).
5. Update the corresponding stored procedures to accept `@TenantId` and filter
   `WHERE AggregateId = @Id AND TenantId = @TenantId` explicitly in the proc body too — do not rely
   on RLS as the sole SQL-layer filter, since RLS's live/applied status is itself unconfirmed.
6. Add an integration test per aggregate (or a shared parameterized test run against all
   classic-pattern repositories) asserting: same-tenant load succeeds, cross-tenant load returns
   not-found/forbidden, not silent-zero-rows.

**Effort**: L (touches ~40+ repository classes' method signatures, their command handlers, and their
stored procedures — mechanical per-aggregate but not small in aggregate; recommend prioritizing the
23 audit-logged aggregates first since they're both Tier-0-cited and independently the audit
population, then the remaining ~35).

**Risk of not fixing**: **Critical** — either a live cross-tenant data exposure (if XC-3 is fixed
first) or a silent solution-wide availability failure (if the RLS migration is live) or a fully
unmitigated tenancy gap (if it isn't) — every one of the three possible states is severe.

**Sequencing**: **Must complete and be verified before XC-3** (see "Required sequencing" above).
No dependency on XC-2. Is itself the prerequisite most per-module `gaps/*.md` "Violates Tenancy" /
"Violates RLS" rows point back to — do not schedule per-module tenancy fixes independently of this
item; almost none of the 25 module files carry a module-specific tenancy fix, because this is the
one place the fix belongs.

---

## XC-2. Actor/CorrelationId propagation (C#-outbox rewire + SQL AuditLog binding)

**Category**: Compliance + Security (forensic/audit failure).
**Cites**: `gaps/_severity-ranking.md` #3 (Tier 0); `security/actor-propagation.md` §1-5;
`rule-enforcement/_shared-findings.md` G2. Also see the Ingestion Discovery's
`docs/ingestion/recommendations/plantsafety-recommendations.md` Top-5 #3, which already recommends
this exact rewire for the same 58 call sites — this item does not duplicate that analysis, it
reconfirms it now spans the full ~344-transition lifecycle inventory, not just the ingestion-visible
subset.

**Problem**: two independent bugs land on an overlapping population:
- **Bug #1 (C#/outbox)**: all 58 confirmed `outboxWriter.Enqueue(domainEvent,
  aggregate.TenantId.ToString(), "system", Guid.Empty.ToString())` call sites hardcode the actor
  and correlation away, discarding the real values the command handler already received via
  `CommandEnvelope`. The correct, already-built, already-registered alternative —
  `IUnitOfWork.EnqueueEvent(domainEvent, tenantId, actorId, correlationId)` — has zero callers.
- **Bug #2 (SQL/AuditLog)**: the T-SQL `AuditLog` INSERT embedded in each of the 23 audit-logged
  aggregates' `usp_*_Insert`/`_Update` stored procedures binds `ChangedBy = SYSTEM_USER` — a shared
  service-account identity, never the application-level actor. Fixing Bug #1 does not touch this.

**Concrete fix**:
1. Change all 58 `outboxWriter.Enqueue(...)` call sites to `unitOfWork.EnqueueEvent(domainEvent,
   aggregate.TenantId.ToString(), actorId, correlationId)`, threading the real `ActorId`/
   `CorrelationId` already available on `CommandEnvelope` through the handler → repository call
   chain. Mechanical, but verify each repository method signature actually accepts and threads the
   two new parameters — not a pure find/replace.
2. For the 23 audit-logged aggregates' stored procedures, add an `@ActorId`/`@ChangedBy` parameter
   to each `usp_*_Insert`/`_Update` proc, bind it from the C# caller (same `CommandEnvelope.ActorId`
   used in step 1), and change the `AuditLog` INSERT to use it instead of `SYSTEM_USER`.
3. Add `TenantId`/`ActorId` fields to the ~22 domain event contracts confirmed missing them
   (`CultureCreatedEvent` and siblings) in the same pass, since it's the same root cause and the
   same commit is the natural place to close both.
4. Do **not** attempt this for the 13 zero-implementation aggregates (XC-4) — there is no outbox
   call site to rewire until a repository exists. Do not attempt this for DataLake/Intelligence/
   Azure using this exact mechanism — each has an independently-different actor-loss shape (no
   `ActorId` column at all in DataLake/Azure's outbox; `Investigation.RequestedByActorId` narrowed
   to `string` in Intelligence) requiring its own fix, cross-referenced in those modules' own
   ingestion-recommendation docs (`docs/ingestion/recommendations/datalake-recommendations.md`,
   `intelligence-recommendations.md`, `azure-recommendations.md`).

**Effort**: M (58 call sites + ~23 stored procedure changes; mechanical pattern once the first
instance is done as a template, but must be verified per call site, not blind find/replace).

**Risk of not fixing**: **Critical** — "who did this" is unanswerable for every mutating action in
the solution; a hard blocker for SOC 2 / incident-investigation / regulatory audit-trail
requirements regardless of whether any individual transition is otherwise correct.

**Sequencing**: **No dependency on XC-1 or XC-3** — run in parallel with either, at any point in the
remediation timeline. Recommend doing step 1 (the C# rewire) early since it's cheap, mechanical, and
already-built plumbing; step 2 (SQL) can follow on its own cadence since it requires per-procedure
review.

---

## XC-3. Register the missing ~293+ authorization policies

**Category**: Availability (confirmed, live, production-breaking) — **but see the sequencing
warning above; this is not simply "the top priority fix" despite being the most visible defect.**
**Cites**: `gaps/_severity-ranking.md` #1 (Tier 0, the sequencing-trap gap itself);
`security/rls-audit.md` §6; inherited from `docs/ingestion/recommendations/plantsafety-recommendations.md`
Top-5 #1 (same defect, same fix, this document adds the sequencing constraint the ingestion
discovery's own Phase 9 could not fully resolve without this lifecycle audit's RLS findings).

**Problem**: `AddPlantSafetyAuthorization()` registers exactly 2 policies; 293+ controllers
reference 147 feature-level policy classes and ~13 more reference module-level string policies from
`AddPlantSafetyPolicies()`, which is never invoked. Every one of these endpoints throws
`InvalidOperationException` inside ASP.NET Core's authorization middleware, converted to a generic
500 by `ExceptionHandlingMiddleware`.

**Concrete fix**: pick one canonical registration path (either wire `AddPlantSafetyPolicies()` into
`AddPlantSafetyAuthorization()` and reconcile its ~30 module-level names against the 147
feature-level class names, or register the 147 feature-level names directly) and finish wiring it.
Do **not** fix this by stripping the `Policy=` argument down to bare `[Authorize]` — that converts
the bug into an immediate cross-tenant leak on any endpoint whose query derives TenantId from
client-supplied `request.TenantId` rather than the authenticated envelope. Verify with an
integration test hitting a sample of the 147 policy names, asserting 200/403, never 500.

**Effort**: S-M (registration itself is small; reconciling 147 names against ~30 is the real work).

**Risk of not fixing**: **Availability** if left alone (current state); **becomes a live
cross-tenant security exposure if fixed before XC-1** (this is the entire point of the sequencing
section above — repeated here because this is the item most likely to be picked up first by a team
scanning for "biggest bang for buck," which is exactly the trap).

**Sequencing**: **Must not start before XC-1 is complete and verified.** No dependency on XC-2.

---

## XC-4. 13+ aggregates' write-repository has zero implementing class

**Category**: Availability (guaranteed runtime failure — `Create` fails at DI-container resolution).
**Cites**: `gaps/_severity-ranking.md` #4 (Tier 1); `sql-shape/_shared-findings.md` G1/G5;
`command-handler-map/_shared-findings.md` F13.
**Scope**: confirmed — `RegulatoryCalendar`, `Certification`, `Compliance`, `ManagementOfChange`,
`Permits`, `Policy`, `Procedures` (Compliance module, 7); `Culture`, `ScoreCards` (Analytics, 2);
`EmergencyManagement`, `IncidentManagement`, `Observation` (SafetyEvents, 3) — 12 directly confirmed,
plus `AiGovernance`/`AiSafetyIntelligence` and most of `emerging-createonly.md`'s 9 other markers
(`ContractorVisitor`, `Governance`, `HumanFactors`, `OperationalSafety`, `ProcessDefinition`,
`Sustainability`, `WorkerWellBeing`) pattern-inferred/confirmed to have **no SQL of any kind**,
an even more basic absence than G1's "class missing but SQL exists" framing.

**Problem**: calling `Create` on any of these aggregates fails at DI-container resolution before a
single SQL statement runs — worse than "the stored procedure doesn't exist," because the C# class
that would call any procedure doesn't exist either. Several of these (`Procedures`, `Culture`) have
a same-named class that implements an *unrelated* interface — a naming-collision trap that could
mislead a future engineer into thinking the repository exists (`sql-shape/_shared-findings.md` G2).

**Concrete fix**: for each of the 12+ confirmed aggregates, (a) write the missing
`I<Aggregate>Repository` implementation following the `Features/*` generation's pattern (explicit
`tenantId` parameter — get this right the first time rather than adding it retroactively as XC-1
requires elsewhere), (b) write the missing stored procedures where SQL doesn't exist at all
(`RegulatoryCalendar` and most of `emerging-createonly.md`'s cluster), (c) register the
implementation in DI, (d) rename or delete the naming-collision classes (`Procedures`Repository,
`Culture`Repository) so they can't be mistaken for the real write path in a future search.
**Prioritize by product need, not audit order** — this document does not recommend building all 13
speculatively if some of these aggregates (especially the 9 `emerging-createonly.md` markers with
zero SQL of any kind) are aspirational/unwired features not yet needed. Confirm scope with product
owners before committing engineering time to all 13.

**Effort**: L (13 repository implementations, 12+ stored procedure sets, from scratch).

**Risk of not fixing**: **Availability** — ranked by Phase 6 as "the single worst — and most
solution-wide — shape-alignment finding of the entire audit"; not recoverable by fixing a validator
or widening a column.

**Sequencing**: independent of XC-1/XC-2/XC-3 — these aggregates never reach the repository layer,
so none of the other three fixes' code paths apply to them until this lands. Do XC-1's tenancy
pattern correctly (`tenantId`-first signatures) when building these new repositories, so they don't
need to be retrofitted later.

---

## XC-5. AuditLog coverage (22-23/93 aggregates, best-effort, anonymous where it exists)

**Category**: Compliance (forensic gap, distinct from XC-2's actor-identity bug).
**Cites**: `gaps/_severity-ranking.md` #7 (Tier 1); `command-handler-map/_shared-findings.md` F8.

**Problem**: 70 of 93 PlantSafety aggregates get zero `AuditLog` row for any transition. Of the 23
that do get one: the INSERT is wrapped in an empty-`CATCH` (write failures silently swallowed), and
`Module` is bound to the stored-procedure name, which can't distinguish which of several transitions
sharing one generic `_Update` proc actually wrote the row.

**Concrete fix**: (a) decide, with compliance/product stakeholders, which of the 70 uncovered
aggregates actually need an audit trail (not all 93 necessarily do — treat as a scoping exercise,
not a blanket mandate); (b) for aggregates that need it, add the same `AuditLog` INSERT pattern the
23 already have, corrected per XC-2 (`ChangedBy` from the real actor, not `SYSTEM_USER`); (c) replace
the empty-`CATCH` swallow with a logged failure (audit-write failures should be visible, even if
they don't block the primary transaction); (d) bind `Module`/`Action` from the actual transition
name passed by the caller, not the generic proc name, so the same `_Update` proc can distinguish
which transition wrote a given row.

**Effort**: L (broad scope; recommend phasing by aggregate value, starting with the highest-severity
domains — EmployeeSafety, MachineSafety, SafetyProcedures, ContractorSafety currently have zero
coverage despite carrying real safety-incident data).

**Risk of not fixing**: **Compliance** — no record exists that certain actions ever happened, for
75% of the aggregate population; distinct from XC-2 (which is about *who*, not *whether*).

**Sequencing**: do this after or alongside XC-2 (same `AuditLog`/`ChangedBy` mechanism) — building
new AuditLog coverage using the still-broken `SYSTEM_USER` pattern just grows the population XC-2
has to fix later. No dependency on XC-1/XC-3.

---

## XC-6. Delete/hard-delete does not exist anywhere in PlantSafety (93/93 aggregates)

**Category**: Compliance (data-retention / right-to-erasure) — conditional severity.
**Cites**: `gaps/_severity-ranking.md` #6 (Tier 1); `inventory.md`.

**Problem**: full grep of all `Aggregates/*.cs` confirms zero `Delete` methods anywhere. Plausibly
correct-by-design for regulatory/audit records (you generally do not want an OSHA-reportable
accident record hard-deletable), but there is no path — C# or SQL — to remove a record from any of
~93 aggregates if one is ever legally required to.

**Concrete fix**: this is a policy decision, not a default engineering task. Confirm with legal/
compliance whether any category of PlantSafety data is subject to a right-to-erasure or
data-retention deletion mandate (e.g., contractor/employee PII embedded in `ContractorSafety`/
`EmployeeSafety` aggregates is the most plausible candidate; regulatory accident/incident records
are the least plausible, since those typically must be *retained*, not deleted). Only build
soft-delete/hard-delete plumbing for the categories that need it, once identified — do not add
`Delete` to all 93 speculatively.

**Effort**: S (policy conversation) + M per aggregate category that's confirmed to need it.

**Risk of not fixing**: **Conditional** — a hard compliance blocker only if a real deletion mandate
applies to some subset of this data; otherwise arguably correct design, not a gap.

**Sequencing**: independent of XC-1 through XC-5 — no code-level interaction with any of them.

---

## Summary — required order

| Order | Item | Depends on |
|---|---|---|
| 1 | **XC-1** — Tenancy check + RLS engagement | None (do first) |
| 2 | **XC-3** — Auth-policy registration | **XC-1 complete and verified** |
| any time | **XC-2** — Actor/correlation propagation | None — parallel track |
| any time | **XC-4** — Zero-implementation repositories | None — parallel track (use XC-1's correct tenantId-first pattern when building) |
| any time, after/with XC-2 | **XC-5** — AuditLog coverage expansion | Soft dependency on XC-2 (same mechanism) |
| any time | **XC-6** — Delete/retention policy decision | None — policy conversation, not blocked on anything |

**The single highest-priority action in this entire recommendations set is XC-1** — not XC-3, even
though XC-3 is the more visible, currently-live production defect. Fixing XC-3 first is the one
mistake this whole audit exists to prevent.
