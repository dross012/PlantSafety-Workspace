# Compliance.Permits Module Playbook

Phase 4 domain-modernization pass, fourth module — but a different shape from the first three. Status as of 2026-07-30.

## Why this one was different

PermitToWork, RiskManagement, and AccidentManagement all had working write paths that just needed concurrency/tenant/NotFound-severity fixes and a missing read query. `Compliance.Permits` had **no write path at all**: the `Permits` aggregate existed with only a `Create` factory, `PermitsLifecycleRules`/`PermitsArchivalPolicy` were fully scaffolded but never called, and zero Application-layer commands ever loaded or mutated it — confirmed by the repo's own `docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md` (dated 3 days prior), which found only 3 real write actions across ~124 Compliance API endpoints platform-wide, none touching a Domain aggregate.

Per explicit user instruction ("I want a complete application"), this was a full build-out from nothing, not a patch — see [[feedback-anemic-modules-full-buildout]] memory note, which is now a standing policy for the rest of this modernization effort whenever another anemic module turns up.

## Investigated first: is this a duplicate of PermitToWork?

No. Confirmed via the architecture blueprint and direct inspection — `Compliance.Permits` and `PermitToWork.PermitWorkflow` are genuinely separate bounded contexts that happen to share the word "permit." No merge/dedup needed.

## Domain model

`Permits` aggregate (`PlantSafety.Domain.Compliance.Permits.Aggregates.Permits`) — went from Create-only to a real 7-state lifecycle:

- Created → **Approve** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived
- Created → **Reject** → Rejected
- Active → **Expire** → Expired

6 new lifecycle methods (`Approve`, `Reject`, `Complete`, `Expire`, `Close`, `Archive`), 6 new domain events (`PermitsApprovedEvent` etc.), `PermitsStatus` extended additively with `Rejected`/`Expired`. The pre-existing `PermitsLifecycleRules.Can*` guards are now actually called by the aggregate instead of being dead code.

## Commands

`CreatePermitsCommand` (create) plus 6 lifecycle commands (`Approve/Reject/Complete/Expire/Close/ArchivePermitsCommand`). All 6 mutating commands implement `IConcurrencyAwareCommand` with `ETag` **from day one** — not retrofitted like the first three modules, since the pattern is now well-established.

## Queries

`GetPermitsByIdQuery` (single item, tenant-scoped, ETag) and `GetPermitsQuery` (date-range list) — both built from the start, unlike PermitToWork/RiskManagement/AccidentManagement where the by-id query was added in a follow-up pass.

## Repository / SQL

New `[Compliance].[Permits]` table (plural — distinct from the pre-existing `[Compliance].[Permit]` singular table owned by the separate, untouched `Features.Compliance.Permits` legacy tree) with native `RowVersion`, tenant-scoped indexes, and full TRY/CATCH+AuditLog+ErrorLog stored procedures (`Insert`/`Update`/`GetById`/`GetRange`/`GetAllActive`) built with `@TenantId` and `@ETag` guards from the start. `PermitsDapperRepository` (write) and `PermitsReadDapperRepository` (read) both follow the now-standard pattern exactly — both `AddAsync` and `UpdateAsync` enqueue to the outbox, `UpdateAsync` uses `ExecuteScalarAsync` and throws `ConcurrencyConflictException` on zero rows affected.

## Cleanup performed

Deleted `PlantSafety.Infrastructure\Compliance\Permits\Persistence\PermitsRepository.cs` and `PermitsReadModel.cs` — orphaned scaffolding from an earlier, abandoned attempt that referenced stored procedures (`usp_Permits_GetById`/`usp_Permits_GetAll`) which didn't exist anywhere in the codebase and weren't referenced by any live code. Confirmed zero other references before deleting.

## API

`PermitsV1Controller` at `api/v1/compliance/permits` — Create, Approve, Reject, Complete, Expire, Close, Archive, GetById, GetRange. Confirmed no route collision with the 13 existing read-only permit-type sub-controllers (`.../permits/hotwork` etc., untouched) or the separate Features-tree Permits controller.

## DI

Wired into the central `InfrastructureServiceRegistration.cs` directly — the older per-module "a human wires this in centrally" comment pattern (seen on Scheduling's and other modules' registration files) turned out to already be stale/done for the modules checked, so this one was wired in following the actual live pattern rather than leaving it as a dangling extension method.

## Explicitly out of scope (left alone)

- `PlantSafety.Application.Features.Compliance.Permits` — the older, separate "Features" tree with `CreatePermitCommand`/`ApprovePermitCommand` against the singular `[Compliance].[Permit]` table. Still the live path for whatever currently calls it; not touched, not migrated onto the new aggregate-backed path.
- The 13 read-only permit-type category modules (HotWork, ColdWork, ConfinedSpace, Electrical, Excavation, HotTap, Lifting, LineBreak, LockoutTagout, PressureTesting, Radiography, WorkatHeight, ChemicalHandling) and their controllers — separate read-model slices, not modified.
- The other 7 Compliance sub-aggregates (Auditing, Certification, Compliance-the-self-named-one, ManagementOfChange, Policy, Procedures, RegulatoryCalendar) — same anemic shape per the blueprint, not touched this pass. Each is its own future full-build-out unit of work under the same standing policy.

## Tests

- `PlantSafety.Tests/Compliance/Permits/Commands/ApprovePermitsCommandHandlerTests.cs` and `Handlers/GetPermitsByIdQueryHandlerTests.cs` — same regression pattern as the other three modules.
- Full suite verified independently: 751/751 unit tests passing.
- `PlantSafety.Api`, `PlantSafety.Tests`, `PlantSafety.IntegrationTests` all build with 0 errors (integration tests not executed — no live SQL Server in this session).

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results (`PermitsStatusResult`), same limitation as the other three modules — clients re-fetch via `GetPermitsByIdQuery`.
- No `PlantSafety.DataIntegrationTests`/`PlantSafety.IntegrationTests` coverage exercising the new stored procedures against a live SQL Server.
- The other 7 Compliance sub-aggregates remain fully anemic (Create-only or less), consistent with the blueprint's finding — each needs its own full build-out.
- No reconciliation between this new aggregate-backed path and the legacy `Features.Compliance.Permits` path — both now exist and write to different tables (`[Compliance].[Permits]` vs `[Compliance].[Permit]`). A future decision is needed on whether/how to migrate the legacy path's callers over, per the extraction blueprint's Phase 2 "write-path shadow" plan.
