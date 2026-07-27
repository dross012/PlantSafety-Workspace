# Scheduling / Maintenance / Compliance Extraction Blueprint

**Date:** 2026-07-27
**Status:** Planning only — no code, no SQL, no project files created by this
document. This is item 5 of a 5-item follow-up list; items 1-4 (Reporting
AzureAd config, dead policy cleanup, Incident aggregate migration, Reporting
read-model snapshot tests) are out of scope here and untouched.

This blueprint answers one question: what would it take to pull
`Scheduling`, `Maintenance`, and `Compliance` out of the single
`PlantSafety.Domain` project into their own standalone repos, the way
`Reporting`, `Shared.AuditLogging`, `Shared.Authorization`, `Shared.EventBus`,
and `Shared.SafetyData` already were? It is grounded in what actually exists
on disk today (verified by direct inspection of `PlantSafety.Domain`,
`PlantSafety.Application`, `PlantSafety.Api`, and `PlantSafety.Infrastructure`,
plus the sibling repos under `C:\Users\dross\source\`), not in the
aspirational module list from the earlier modernization checklist.

**Headline finding, stated up front:** `Scheduling` and `Compliance` are real,
substantial bounded contexts inside `PlantSafety.Domain` and are legitimate
extraction candidates. **`Maintenance` does not exist as a bounded context at
all** — there is no `PlantSafety.Domain/Maintenance/` folder, no `Maintenance`
aggregate, and no `Maintenance` schema. What the checklist called
"Maintenance" is a handful of unrelated, mostly read-only leaf features
scattered across other contexts (`Operational.PredictiveMaintenance`,
`Operational.PreventiveMaintenance`, `Compliance.Procedures.Maintenance`,
`SafetyPolicies.MachineMaintenanceInspection`, and a single
`CrossDomain.Machines.Maintenance.MaintenanceReference` cross-reference type).
§1.3 documents this in detail instead of forcing a three-way blueprint onto
something that isn't there.

---

## 1. Current state

### 1.1 Scheduling — real bounded context, partial write path

**Location:** `PlantSafety.Domain/Scheduling/`, `PlantSafety.Application/Scheduling/`
(plus a parallel, unrelated `Scheduling` folder tree under
`PlantSafety.Application/Features/PlanningProjects/*/Scheduling` — a
different, PlanningProjects-owned concept that happens to share the word
"Scheduling" and is **not** part of this bounded context; not touched by this
blueprint), `PlantSafety.Api/Scheduling/`, `PlantSafety.Infrastructure/Scheduling/`.

Five sub-aggregates, each with the full scaffold (Aggregate, DomainEvents
marker, Entities, Enums, Events, Exceptions, Factories, Interfaces, Policies,
Rules, Services, Snapshots, Specifications, ValueObjects/Ids) under
`PlantSafety.Domain/Scheduling/<SubContext>/`:

| Sub-aggregate | Domain events raised (`AddDomainEvent`) | Write path wired end-to-end? |
|---|---|---|
| `PMScheduling` | `PMSchedulingCreatedEvent`, `PMSchedulingScheduledEvent`, `PMSchedulingCompletedEvent`, `PMSchedulingEscalatedEvent`, `PMSchedulingCancelledEvent` | **Yes** — `SchedulePreventiveMaintenanceCommandHandler`/`RecordPMSchedulingCompletionCommandHandler`/`EscalatePMSchedulingCommandHandler` load the aggregate via `IPMSchedulingRepository.GetByIdAsync`, call the domain method, `UpdateAsync` — genuine load-mutate-save DDD pattern |
| `WorkOrderPriority` | `WorkOrderPriorityCreatedEvent`, `WorkOrderPriorityStartedEvent`, `WorkOrderPriorityEscalatedEvent`, `WorkOrderPriorityResolvedEvent`, `WorkOrderPriorityClosedEvent` | **Yes** — `EscalateWorkOrderPriorityCommandHandler`/`ResolveWorkOrderPriorityCommandHandler`, same load-mutate-save pattern |
| `EmergencyDrillPlanning` | `EmergencyDrillPlanningCreatedEvent` | No — `Insert`/`Update` stored procedures and an `EmergencyDrillPlanningDapperRepository` exist, but only `GetEmergencyDrillPlanningQueryHandler` (read) exists in `PlantSafety.Application`; nothing calls Create/Update |
| `HazardIsolationSequencing` | `HazardIsolationSequencingCreatedEvent` | No — same shape: Insert/Update SQL + repository exist, only a Get query handler is wired |
| `ShiftFatigue` | `ShiftFatigueCreatedEvent` | No — same shape: Insert/Update SQL + repository exist, only a Get query handler is wired |

Plus a cross-cutting `BulkUpdateSchedulingCommand` →
`SchedulingBulkDapperRepository` → `usp_Scheduling_BulkUpdateResource`
(operates directly on rows, bypasses the aggregates entirely — a distinct,
already-anemic bulk-operations path, same shape as similar `BulkUpdate*`
commands elsewhere in this codebase).

**API surface:** `PlantSafety.Api/Scheduling/Controllers/V1/SchedulingV1Controller.cs`
— one controller, `/api/v1/scheduling/...`, covering all five sub-aggregates'
reads plus PMScheduling/WorkOrderPriority writes plus the bulk endpoint.
Every action goes through MediatR (`ISender`) to the Application layer; no
domain logic in the controller. Authorization uses `SafetyTierPolicies.Read`/
`Write`/`Approve` (see `docs/architecture/authorization-tiers.md`).

**Data:** all five tables live in `PlantSafety.CoreDB`, schema `[Scheduling]`
(`[Scheduling].[PMScheduling]`, `[Scheduling].[WorkOrderPriority]`,
`[Scheduling].[EmergencyDrillPlanning]`, `[Scheduling].[HazardIsolationSequencing]`,
`[Scheduling].[ShiftFatigue]`), 31 stored procedures under
`PlantSafety.Infrastructure/Scheduling/Sql/Procedures/` (5 Insert + 5 Update +
3 Get* per sub-aggregate, plus the one bulk proc), 5 `CREATE TABLE` scripts
under `.../Sql/Tables/`. DI is **not yet wired into the shared
`InfrastructureServiceRegistration.cs`** — `SchedulingInfrastructureServiceRegistration.cs`
exists as a standalone extension method with an explicit code comment saying
a human still needs to wire it in centrally. This is a real, current gap,
noted here rather than assumed fixed.

### 1.2 Compliance — real bounded context, overwhelmingly read-only

**Location:** `PlantSafety.Domain/Compliance/`, `PlantSafety.Application/Compliance/`
+ `PlantSafety.Application/Features/Compliance/`, `PlantSafety.Api/Compliance/`,
`PlantSafety.Infrastructure/Compliance/`.

Eight sub-aggregates under `PlantSafety.Domain/Compliance/<SubContext>/`
(same full DDD scaffold as Scheduling): `Auditing`, `Certification`,
`Compliance` (the "self-named" sub-aggregate — a genuine, if confusing,
naming collision with the parent folder, confirmed by direct inspection, not
a typo), `ManagementOfChange`, `Permits`, `Policy`, `Procedures`,
`RegulatoryCalendar`. 16 `AddDomainEvent` call sites total (one
`<Name>CreatedEvent` per sub-aggregate's `Create()` factory method — no
sub-aggregate here has more than a Created event; contrast with Scheduling's
PMScheduling/WorkOrderPriority, which model a real multi-state lifecycle).

**Write path is almost entirely bypassed at the Application layer** — this is
the single most important finding for Compliance, structurally identical to
the pre-redesign `IncidentManagement` anemic-aggregate pattern documented in
`plantsafety-architecture.md` §5. Only three real write commands exist
anywhere under `PlantSafety.Application/Features/Compliance/`:
`CreateMocCommand`, `CreatePermitCommand`, `ApprovePermitCommand` (plus one
`BulkUpdateComplianceCommand`), and **none of them load or mutate a Domain
aggregate** — `CreateMocCommandHandler` (read in full) calls
`IMocRepository.CreateAsync(...)` directly with primitives, never touching
`PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange`.
The Domain-layer `ManagementOfChange`/`Permits`/etc. aggregates and their
`CreatedEvent`s are fully built and unit-testable but are not on any
production code path today. The remaining ~121 of ~124 Compliance API actions
are `HttpGet` reads (confirmed by grep — 121 `HttpGet` vs 3 non-`HttpGet`
actions in `PlantSafety.Api/Compliance/**`).

**Data reality is much wider than the 8 Domain sub-aggregates suggest.** Each
sub-aggregate folder in `PlantSafety.Domain/Compliance/` maps to one Domain
aggregate, but the actual SQL surface fans out into dozens of narrower,
independently-repositoried read-model tables that were never modeled as their
own Domain aggregates — e.g. `Permits` alone backs 11 leaf tables/procedures
(`PermitHotWork`, `PermitConfinedSpace`, `PermitLockoutTagout`,
`PermitElectrical`, `PermitExcavation`, `PermitColdWork`, `PermitHotTap`,
`PermitLifting`, `PermitLineBreak`, `PermitPressureTesting`,
`PermitRadiography`, `PermitWorkatHeight`, `PermitChemicalHandling` — each
with its own `<Name>DapperRepository` and `usp_<Name>_Get<Name>`/`_List`
procedure), and `Policy`/`Procedures`/`Certification`/`RegulatoryCalendar`
follow the same one-aggregate-many-leaf-tables shape. In total: 124 SQL
scripts (procedures + tables) versus 8 Domain aggregates — a roughly 15:1
fan-out that any extraction plan has to account for (see §3).

**Data:** `PlantSafety.CoreDB`, schema `[Compliance]`. Only one stored
procedure in the entire Compliance surface performs a write
(`usp_ComplianceResources_BulkUpdate.sql`) — every other one of the 124
scripts is a `CREATE TABLE` or a `_Get*`/`_List` read query, consistent with
the "almost entirely read-only" finding above.

### 1.3 "Maintenance" — not a bounded context; do not extract as one

Direct search confirms there is no `PlantSafety.Domain/Maintenance/` folder,
no `Maintenance` aggregate, no `[Maintenance]` SQL schema, and no
`IMaintenanceRepository`-style domain repository interface tied to a Domain
aggregate anywhere in the codebase. What exists under the word "Maintenance"
is four unrelated, independently-owned leaf features living inside *other*
bounded contexts, each a thin `Get*Query` → `Get*QueryHandler` →
`*DapperRepository` read slice with no create/update/domain model at all:

| "Maintenance" surface | Actually belongs to | Shape |
|---|---|---|
| `Operational.PredictiveMaintenance` | `Operational` bounded context | `PredictiveMaintenanceStub.cs` — `public static class PredictiveMaintenanceModuleMarker { }`, literally an empty placeholder. `GetPredictiveMaintenanceQueryHandler` exists and reads from `PredictiveMaintenanceDapperRepository`, but there is no aggregate behind it |
| `Operational.PreventiveMaintenance` | `Operational` bounded context | Identical shape — `PreventiveMaintenanceStub.cs` empty marker, one read-only query handler + Dapper repository, no aggregate |
| `Compliance.Procedures.Maintenance` | `Compliance` bounded context (specifically the `Procedures` sub-aggregate) | One leaf read table (`ProceduresMaintenance` / `usp_ProceduresMaintenance_List`), same shape as `Compliance`'s other ~50 leaf read tables described in §1.2 — not a separate concept, just one more `Procedures`-owned maintenance-procedure document type |
| `SafetyPolicies.MachineMaintenanceInspection` | `SafetyPolicies` bounded context | One more read-only leaf query slice, unrelated to the two above |
| `CrossDomain.Machines.Maintenance.MaintenanceReference` | `CrossDomain.Machines` cross-reference layer | A read-only `MaintenanceReference` record + `MaintenanceId` strong ID, explicitly documented in its own header as "Other aggregates reference Maintenance by MaintenanceId only" — i.e. a placeholder for a *future* Maintenance aggregate that has never been built. It is a forward-looking stub, not evidence of an existing bounded context |

**Conclusion:** there is nothing cohesive to extract. Building a
`Maintenance` repo today would mean either (a) inventing a bounded context
that doesn't exist in the domain model to justify the repo, or (b) shipping
an empty shell around the `MaintenanceReference` stub. Per the task's own
instruction to say so plainly rather than force a blueprint onto something
absent: **this document does not propose a `Maintenance` extraction.** If a
real Maintenance bounded context is built in the future (the
`MaintenanceReference`/`MaintenanceId` stub is exactly the seam for it), it
should get its own blueprint at that time, grounded in whatever aggregate
actually gets built — not retrofitted from today's four unrelated leaf
features.

---

## 2. Reference pattern — how existing extractions are shaped

Confirmed by inspecting `Reporting/`, `Shared/EventBus/`, `Shared/AuditLogging/`,
`Shared/Authorization/`:

- **One repo per module**, own `.git`, own `.sln` at the repo root (e.g.
  `Reporting/Reporting.sln`, `Shared/EventBus/Shared.EventBus.sln`).
- **Reporting's layout** (the closest precedent to a full application-shaped
  extraction, as opposed to a Shared library): `Reporting.Domain`,
  `Reporting.Application`, `Reporting.Infrastructure`, `Reporting.Api`,
  `Reporting.UI`, `Reporting.Tests`, `Reporting.IntegrationTests` — one
  project per layer, mirroring `PlantSafety`'s own
  `PlantSafety.Domain`/`.Application`/`.Infrastructure`/`.Api`/`.Tests`/
  `.IntegrationTests` split.
- **`Shared.*` libraries are narrower**: `Shared.EventBus` ships
  `Shared.DomainEvents.Abstractions` / `Shared.EventBus.Abstractions` /
  `Shared.EventBus.Infrastructure` / `Shared.EventBus.Tests` (no Api, no UI —
  it's a library, not a deployable). `Shared.AuditLogging` and
  `Shared.Authorization` follow the same Abstractions/Infrastructure/Tests
  shape.
- **Cross-repo references are relative-path `.csproj` references inside the
  consuming solution**, not NuGet packages — `Reporting.sln` includes
  `..\Shared\AuditLogging\Shared.AuditLogging.Abstractions\...csproj` and
  `..\Shared\AuditLogging\Shared.AuditLogging.Infrastructure\...csproj`
  directly as solution projects, sitting alongside `Reporting.*` in the same
  `.sln`. The hard rule enforced today (per `plantsafety-architecture.md`
  §2): **a consuming module never takes a project reference on another
  module's `Domain`/`Application`/`Infrastructure`/`Api` project** — only on
  a `Shared.*.Abstractions` (contract) project, with `Shared.*.Infrastructure`
  wired in via DI extension methods, never referenced by business logic
  directly.
- **No EF Core anywhere** — every one of these modules is Dapper +
  hand-written stored procedures, `IDbTransaction`-scoped, no `DbContext`.
  Any extraction plan must preserve this.

---

## 3. Proposed target shape

### 3.1 `Scheduling` (real, extractable)

New repo `C:\Users\dross\source\Scheduling\` (sibling to `PlantSafety`,
`Reporting`, `Shared`), `Scheduling.sln`:

```
Scheduling/
  Scheduling.Domain/          -- 5 sub-aggregates, moved verbatim from
                                  PlantSafety.Domain/Scheduling/*
  Scheduling.Application/     -- MediatR commands/queries/handlers, moved
                                  verbatim from PlantSafety.Application/Scheduling/*
  Scheduling.Infrastructure/  -- Dapper repositories + Sql/Procedures + Sql/Tables,
                                  moved from PlantSafety.Infrastructure/Scheduling/*
  Scheduling.Api/             -- SchedulingV1Controller, moved from
                                  PlantSafety.Api/Scheduling/*
  Scheduling.Tests/
  Scheduling.IntegrationTests/
```

Dependencies on `Shared.*`:
- **`Shared.EventBus`** (`Shared.EventBus.Abstractions` +
  `Shared.EventBus.Infrastructure`, `IEventPublisher`/`IEventSubscriber`) —
  required from day one of the extraction, since Scheduling must now publish
  `PMSchedulingCreatedEvent`/`...ScheduledEvent`/`...CompletedEvent`/
  `...EscalatedEvent`/`...CancelledEvent` and the four `WorkOrderPriority`
  events cross-process instead of in-process (see §4).
- **`Shared.Authorization`** (`Shared.Authorization.Abstractions`) —
  `SchedulingV1Controller` uses `SafetyTierPolicies.Read`/`Write`/`Approve`
  today as PlantSafety-local policy constants; extraction requires these to
  come from the shared contract the same way `Reporting.Api` already
  consumes it, rather than a duplicated local copy.
- **`Shared.AuditLogging`** — likely, not confirmed: PlantSafety's own audit
  writes happen inside the stored procedure itself (`INSERT INTO dbo.AuditLog`
  via synonym, per `audit-logging.md` §2), and none of the 31 Scheduling
  procedures currently do this (confirmed — no `AuditLog` reference in any
  `Scheduling/Sql/Procedures/*.sql`). If Scheduling's audit trail should
  continue to land in the same shared `dbo.AuditLog` table post-extraction,
  its new stored procedures need the same embedded `INSERT`/`CATCH`-block
  pattern against whichever database the table's synonym resolves to — a
  decision for whoever owns the extraction's SQL work, flagged here as an
  open item rather than assumed.
- **`Shared.SafetyData`-style read contract** (new, not existing) — if
  Reporting or another module ever needs Scheduling data (today it doesn't;
  `Shared.SafetyData` today only covers Risk/CorrectiveAction), a
  `Shared.SchedulingData` abstractions project following the exact
  `ISafetyDataRepository` precedent would be the way to expose it, built only
  when a real consumer needs it.

### 3.2 `Compliance` (real, extractable, but riskier — see §6)

New repo `C:\Users\dross\source\Compliance\`, `Compliance.sln`, same
five/six-project shape as Scheduling:

```
Compliance/
  Compliance.Domain/          -- 8 sub-aggregates
  Compliance.Application/     -- commands/queries/handlers/validators
  Compliance.Infrastructure/  -- ~60+ Dapper repositories, 124 SQL scripts
  Compliance.Api/             -- all Compliance/* controllers (Auditing,
                                  Certification, ManagementOfChange, Permits,
                                  Policy, Procedures, RegulatoryCalendar,
                                  plus the self-named Compliance controller)
  Compliance.Tests/
  Compliance.IntegrationTests/
```

Dependencies on `Shared.*`:
- **`Shared.EventBus`** — same rationale as Scheduling, for the 16
  `<Name>CreatedEvent`s (once/if the write path is actually rewired to raise
  them — see §6).
- **`Shared.Authorization`** — Compliance's 121 read + 3 write controller
  actions are already `[Authorize(Policy = ...)]`-gated; same migration as
  Scheduling.
- **`Shared.AuditLogging`** — more likely to matter here than for Scheduling,
  since Compliance is audit/regulatory-focused by definition; needs the same
  "does today's SQL already write `dbo.AuditLog`" check per procedure before
  assuming it, not assumed here.
- Given Reporting's existing `SafetyCompliance` report type (§4 of
  `plantsafety-architecture.md` — a *derived* rollup over `RiskMgmt.Risk`/
  `ContinuousImprovement.CAPA`, **not** sourced from this `Compliance`
  bounded context at all, despite the name collision), extracting this
  `Compliance` module does **not** touch that existing Reporting report type.
  Flagged explicitly so nobody conflates the two "Compliance" concepts during
  planning.

---

## 4. Data ownership

Both contexts currently live in **`PlantSafety.CoreDB`**, each under its own
SQL schema (`[Scheduling]`, `[Compliance]`) — already logically separated at
the schema level, which is the best-case starting point for an extraction.

**What moves:**
- Scheduling: 5 tables, 31 procedures, all under `[Scheduling].*` — move to
  a new dedicated database (e.g. `Scheduling.CoreDB`) once the module is
  fully cut over (§5).
- Compliance: all tables/procedures under `[Compliance].*` — same target
  shape, `Compliance.CoreDB`.

**What stays / needs a cross-database bridge, and for how long:**
- **During migration** (the read-only-extraction and dual-write phases in
  §5), the new module's database and PlantSafety's `PlantSafety.CoreDB` need
  to be reachable from both codebases simultaneously. This codebase's
  existing precedent for that is the **cross-database `dbo.AuditLog`
  synonym** pattern documented in `audit-logging.md` — a per-database SQL
  `SYNONYM` pointing at the shared table, resolved by each database's own
  connection string, no linked server, no cross-database `JOIN` in
  application code. The same synonym-per-database approach (a
  `Scheduling`-side synonym pointing back at `PlantSafety.CoreDB.[Scheduling].*`
  during the transition, dropped once the write path fully cuts over) is the
  lowest-risk option consistent with existing conventions — **no EF, no ORM,
  Dapper + stored procedures only**, per the Maximalist Module Rule already
  enforced across this codebase.
- **Permanently, if anything needs to stay:** the `BulkUpdateSchedulingCommand`
  → `usp_Scheduling_BulkUpdateResource` and `BulkUpdateComplianceCommand` →
  `usp_ComplianceResources_BulkUpdate` bulk-operation paths both operate
  directly on rows across (potentially) multiple resource types in a single
  call; confirm during implementation whether either bulk operation ever
  spans a resource that would land in a *different* target module after
  extraction (e.g. a bulk Scheduling update that also needs to touch a
  Compliance-owned Permit) — not found in the current SQL (each bulk proc
  only touches its own schema's resources), but worth a final grep-before-cut
  check since bulk operations are exactly the kind of call site most likely
  to hide a silent cross-module assumption.
- **Compliance's ~15:1 aggregate-to-table fan-out (§1.2)** means the
  extraction unit for Compliance is the whole `[Compliance]` schema, not
  "8 aggregates" — all ~120 leaf tables/procedures move together with their
  owning sub-aggregate folder (e.g. all 12 `Permit*` leaf
  tables/repositories move with `Permits`), since none of them are
  independently referenced by any other bounded context today (confirmed: no
  cross-schema foreign keys or cross-namespace repository references found
  in either `Scheduling` or `Compliance` during this review).

---

## 5. Event-based decoupling

Today, `Scheduling` and `Compliance` are **not** in the curated 12-event,
4-module cross-module contract set built in the `Shared.EventBus` pass (see
`eventbus-architecture.md` §1 — that pass explicitly checked for a
Scheduling/Maintenance/Compliance *module* and found none, so no
`CrossModuleDomainEventMapper` case arms or Reporting-side handlers exist for
any of the 18 Scheduling / 16 Compliance domain events yet). Post-extraction,
both new modules would need to add themselves to that same infrastructure
rather than invent a new one:

1. **Publisher side (new module):** the equivalent of
   `PlantSafety.Infrastructure/Messaging/CrossModule/CrossModuleDomainEventMapper.cs`
   + `CrossModuleEventRelay` lives in the new module's own
   `<Module>.Infrastructure/Messaging/CrossModule/` — mapping the module's
   own domain event records (e.g. `PMSchedulingScheduledEvent`,
   `WorkOrderPriorityEscalatedEvent`, `ManagementOfChangeCreatedEvent`) to
   new `Shared.DomainEvents.Abstractions` DTOs (`Scheduling.PMSchedulingScheduled`,
   `Compliance.ManagementOfChangeCreated`, etc., following the exact
   `<Module>.<EventName>` naming convention already used by the 12 curated
   events), and enqueuing an `EventEnvelope` via `IEventPublisher` — same
   `INSERT INTO dbo.EventQueue` SQL-queue transport already built, same
   database (`PlantSafety.IntegrationDB`) unless/until a dedicated queue
   database is judged necessary.
2. **The Unit-of-Work publish-after-commit ordering** (outbox flush + SQL
   commit, *then* cross-module relay flush, per `eventbus-architecture.md`
   §5) is the pattern to replicate in each new module's own `UnitOfWork`, not
   a shared piece of infrastructure to reference directly.
3. **Consumer side (PlantSafety and Reporting):** PlantSafety would gain a
   `SchedulingEventSubscriberHostedService`-equivalent (or fold subscription
   into its existing hosted-service pattern) with `ConsumerGroup =
   "PlantSafety"`, registering `IEventHandler<TPayload>` implementations for
   whichever Scheduling/Compliance events PlantSafety's own UI/workflows
   still need to react to post-extraction (e.g. a Work Order Priority
   escalation surfacing in a PlantSafety dashboard). Reporting would do the
   same with `ConsumerGroup = "Reporting"`, following the exact
   `EventBusSubscriberHostedService` + `InMemorySafetyEventActivityFeedStore`-
   style read-model precedent already built for the 12 curated events.
4. **No in-process call sites survive the cut.** Today, nothing outside
   `PlantSafety.*` calls into `Scheduling`/`Compliance` code directly (both
   are folders inside the same assembly boundary as everything else in
   `PlantSafety.Domain`, so there's no existing *cross-module* in-process
   call to find — the decoupling work is entirely about the *new* boundary
   that extraction itself creates, not about removing an existing coupling).
   The concrete risk is the reverse: other `PlantSafety.Domain` contexts
   referencing `Scheduling`/`Compliance` types directly today, since they all
   currently share one assembly — see §6.

---

## 6. Phased migration order

A conservative, reversible-at-each-step sequence, applied independently to
`Scheduling` and `Compliance` (they do not need to move together):

1. **Phase 0 — Inventory & freeze.** Grep-confirm (repeat the searches this
   document is based on) that no other `PlantSafety.Domain` bounded context
   references `Scheduling.*`/`Compliance.*` types directly (aggregates,
   value objects, enums) outside their own folder. If any such reference
   exists, it must be replaced with a `StrongId`-only cross-domain reference
   (the same pattern already used by `CrossDomain.Machines.Maintenance.MaintenanceReference`
   and documented as the convention in that file's own header) *before*
   extraction, not during.
2. **Phase 1 — Read-only extraction.** Stand up the new repo/solution with
   `<Module>.Domain`/`.Application`/`.Infrastructure`/`.Api` scaffolding.
   Copy (not move) the read paths first: Domain aggregates + `Get*`/`List*`
   query handlers + Dapper read repositories + a **read-only cross-database
   synonym** back into `PlantSafety.CoreDB`'s `[Scheduling]`/`[Compliance]`
   schemas (per §4). The new module's API serves reads; PlantSafety's
   existing controllers keep serving reads too (dual-read, zero risk,
   trivially revertible by just not deploying the new module).
3. **Phase 2 — Write-path shadow.** For sub-aggregates with a real write
   path today (Scheduling's `PMScheduling`/`WorkOrderPriority`), wire the new
   module's own command handlers against the *same* underlying tables
   (still in `PlantSafety.CoreDB` at this point) and run them in shadow
   (write to both old and new code paths, compare results) before cutting
   PlantSafety's controllers over. For Compliance's mostly-absent write path,
   this phase is where the anemic-aggregate gap from §1.2 either gets fixed
   as part of the extraction (aggregate-backed `Create`/`Update` commands,
   mirroring the `IncidentManagement` redesign precedent) or is explicitly
   carried over as a known gap — a decision to make explicitly, not silently,
   since extraction is the natural moment to fix it but is not required to.
4. **Phase 3 — Cutover.** Point PlantSafety's own `SchedulingV1Controller`/
   Compliance controllers to call the new module over HTTP (or retire them
   entirely in favor of clients hitting the new module's API directly —
   an app-routing decision outside this blueprint's scope). Flip
   `IEventPublisher` wiring live (§5) so cross-module events start flowing.
   Remove the old in-process Application/Domain code from `PlantSafety.*`
   only after the new module has run the write path in production
   successfully for an agreed bake-in period.
5. **Phase 4 — Data ownership finalized.** Move the `[Scheduling]`/
   `[Compliance]` schemas to their own dedicated databases
   (`Scheduling.CoreDB`/`Compliance.CoreDB`), drop the temporary
   cross-database synonyms from §4, update connection strings. This is the
   only phase that is genuinely hard to reverse quickly, which is why it is
   last.
6. **Phase 5 — Repo cleanup.** Delete the now-dead `PlantSafety.Domain/Scheduling`,
   `PlantSafety.Application/Scheduling`, `PlantSafety.Api/Scheduling`,
   `PlantSafety.Infrastructure/Scheduling` folders (and the `Compliance`
   equivalents) from the PlantSafety repo, and the now-unused
   `SchedulingInfrastructureServiceRegistration.cs`/DI registrations.

Scheduling and Compliance can run this sequence on independent timelines;
nothing in either module's Phase 0 inventory found a cross-dependency between
the two.

---

## 7. Open risks / unknowns

- **Compliance's write path is mostly missing, not mostly hidden.** Unlike
  the pre-redesign `IncidentManagement` (which had *a* production write path,
  just one that bypassed its Domain aggregate), Compliance genuinely has only
  3 write actions across ~124 API endpoints. Any extraction plan that assumes
  "move the existing write path" for Compliance is assuming something that
  barely exists; most of what would move is a large read-model surface plus
  8 domain-complete-but-unexercised aggregates. This should be scoped and
  estimated as "extract a mostly read-only reporting-style module + fix 8
  anemic aggregates," not "extract a CRUD module," or the effort will be
  badly underestimated.
- **`SchedulingInfrastructureServiceRegistration.cs` is explicitly
  not wired into the shared DI root yet** (per its own header comment,
  "a human wires this extension in centrally after all module-scoped agents
  finish") — confirm this has actually happened before assuming Scheduling's
  current in-repo write path is even reachable in a running PlantSafety
  instance today; if it isn't wired in, Phase 2's "shadow write" baseline
  needs to start from "make it reachable in PlantSafety first," not from "it
  already works."
- **No stored procedure in either `[Scheduling]` or `[Compliance]` writes to
  `dbo.AuditLog`** (confirmed by grep). If audit coverage is expected to
  continue post-extraction, this is new work for the extraction, not a
  behavior being preserved — flagged so it isn't silently dropped.
- **The self-named `Compliance.Compliance` sub-aggregate** (`PlantSafety.Domain/Compliance/Compliance/Aggregates/Compliance.cs`)
  is a real, distinct aggregate from the parent `Compliance` bounded context
  folder — the naming collision is confirmed, not a documentation error, and
  needs a deliberate naming decision during extraction (e.g. rename the
  sub-aggregate, or rename the module) to avoid `Compliance.Compliance.*`
  namespaces surviving into a brand-new repo.
- **Reporting's existing `SafetyCompliance` report type shares a name with
  this `Compliance` module but sources from entirely different tables**
  (`RiskMgmt.Risk`/`ContinuousImprovement.CAPA`, not `[Compliance].*` — see
  §3.2). Restated here as a risk because it is the single most likely
  point of confusion for anyone picking up this blueprint without having
  read `plantsafety-architecture.md` §4 first.
- **Bulk-operation procs** (`usp_Scheduling_BulkUpdateResource`,
  `usp_ComplianceResources_BulkUpdate`) were checked and found to only touch
  their own schema's rows today, but bulk-by-nature endpoints are exactly
  where a future, unreviewed change is most likely to quietly introduce a
  cross-module row touch — worth a final automated check (not just this
  review's manual grep) immediately before Phase 0 is declared complete for
  either module.
- **No analytics/reporting code was found querying `[Scheduling]` or
  `[Compliance]` tables directly** (only Reporting's `Shared.SafetyData`-
  mediated `SafetyCompliance` report type touches anything with "Compliance"
  in the name, and it doesn't touch this module's tables — see above). This
  is a genuine "nothing to update" finding, not an oversight, but is worth
  re-confirming with a repo-wide grep for `[Scheduling].` / `[Compliance].`
  literal schema references right before cutover, since new report types
  could have been added between this review and implementation.
