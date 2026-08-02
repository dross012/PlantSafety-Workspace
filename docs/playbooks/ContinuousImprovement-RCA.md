# ContinuousImprovement.RCA Playbook

Phase 4 domain-modernization pass, eleventh module. Status as of 2026-08-02.

## Scope: completing a partially-modernized module

`ContinuousImprovement` has two sub-aggregates: `CAPA`/`CorrectiveAction` (already fully modernized with real lifecycle events — Started/Completed/Closed/MarkedOverdue — before this pass) and `RCA`, which was a **literal empty placeholder stub** (`public static class RCAModuleMarker { }`, both in Domain and Infrastructure), same as `SafetyEvents.CrisisManagement`/`DrillsAndExercises`/`NearMisses`/`RootCauseAnalysis` were before their own modernization pass. Only a coarse, date-range read-model query stack existed (`GetRCAQuery` → `usp_RCA_GetRcaRecords`), plus a large pre-existing API scaffold (Controllers V1/V2, Endpoints, Mapping, Telemetry, Diagnostics, Filters, Authorization) wired only to that read-only query.

This closes out `ContinuousImprovement` entirely.

## Not to be confused with `SafetyEvents.RootCauseAnalysis`

The codebase has two distinct, unrelated "root cause analysis" concepts: `SafetyEvents.RootCauseAnalysis` (safety-incident RCA, modernized in an earlier pass) and `ContinuousImprovement.RCA` (continuous-improvement RCA, built this pass). Used the former purely as a structural/depth reference (guard pattern, exception naming, SQL/outbox/concurrency conventions) — no files shared or touched between the two.

## Domain model built this pass

Grounded in the pre-existing `RCAItemDto`'s own documented vocabulary rather than invented: **Initiated → StartAnalysis → InProgress → SubmitForApproval → PendingApproval → Close → Closed → Archive → Archived**. `Archived` was added as the platform's standard terminal retention state (not in the original DTO's documented three statuses) for consistency with every other aggregate modernized in this effort — the same kind of small, deliberate vocabulary extension made for `DrillsAndExercises`'s `Cancelled` status in the SafetyEvents pass.

Fields: `RCAId`, `TenantId`, `SiteId` (Guid?), `ProblemStatement`, `Methodology` (free text, e.g. FiveWhys/FishboneDiagram/FaultTree/Bowtie — kept as a string per the DTO's own comment, not forced into an enum), `OwnerUserId`, `InitiatedAtUtc`, `CreatedBy`/`CreatedAtUtc`.

## Execution: single agent, one aggregate, zero manual fixes needed

Given the single-aggregate scope (unlike Compliance/SafetyEvents/Analytics batches), this was dispatched to one agent rather than parallelized. It:

- Built the full Domain/Infrastructure/Application/Api/Tests stack from nothing, deliberately leaving the pre-existing date-range read stack (`RCAItemDto`, `GetRCAQuery`, `IRCARepository` read-side) completely untouched per explicit instruction.
- Named the new Domain-layer write interface `IRCARepository` in `PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces` — same simple name as the pre-existing Application-layer read interface in a different namespace, the standard disambiguation pattern used everywhere in this codebase; DI fully qualifies both.
- Named the write repository `RCAAggregateDapperRepository` (the plain `RCADapperRepository` name was already taken by the pre-existing read-model class) and added a new `RCADetailDapperRepository` for the new by-id read query, deliberately choosing a brand-new `IRCADetailRepository` interface rather than extending the existing `IRCARepository`, since the task explicitly required leaving that read-model stack untouched — a deliberate, justified divergence from the `SafetyEvents.RootCauseAnalysis` precedent (which extended its own existing interface in a context with no such constraint).
- Investigated the pre-existing API scaffold rather than assuming it was live: found the V1/V2 controllers, Endpoints/Mapping/Telemetry/Diagnostics/Filters were wired only to the old date-range query. Extended the existing `RCAV1Controller` in place with the new Create/lifecycle/by-id actions (matching how `RootCauseAnalysisV1Controller` was extended in the earlier pass) rather than touching the unrelated Endpoints/Mapping/Telemetry/Diagnostics/Filters scaffold files, which remain pre-existing no-op stubs. `RCAAuthorizationPolicies.Write` already existed and is auto-discovered by reflection, so no extra policy wiring was needed.
- Included a `RCANotFoundException` for structural symmetry with the sibling module even though it's unused (handlers use `ApplicationError`+`ErrorSeverity.NotFound` instead) — same as `RootCauseAnalysisNotFoundException`'s unused counterpart.

## Obsolete stub markers found and deleted

After the real aggregate/infrastructure landed, both original placeholder marker classes (`PlantSafety.Domain\ContinuousImprovement\RCA\RCAStub.cs` — `RCAModuleMarker`, and `PlantSafety.Infrastructure\ContinuousImprovement\RCA\RCAStub.cs`) became dead code — zero live references (only a code comment in the new `RCA.cs` mentioning the migration, and stale worktree copies under `.claude\worktrees\` outside the working tree). Deleted both directly after the independent verification pass; a different flavor of the same "clean up now-obsolete scaffolding" discipline applied in every prior module (though those were unused Repository/ReadModel pairs referencing nonexistent procs, not stub markers).

## Verified independently (not just trusting agent self-reports)

- `outboxWriter.Enqueue` present exactly twice in `RCAAggregateDapperRepository.cs` (Add + Update).
- `ExecuteScalarAsync` + `ConcurrencyConflictException` both present in the same file.
- `ErrorSeverity.NotFound` used in all 5 Application handlers (4 lifecycle + by-id query); `catch (ConcurrencyConflictException) { throw; }` passthrough present in all 4 lifecycle command handlers.
- DI registration `.AddContinuousImprovementRCAAggregateInfrastructure()` confirmed present alongside the pre-existing `.AddContinuousImprovementRepositories()` in the central `InfrastructureServiceRegistration.cs`, with no lost entries.
- Full solution build (`Api`, `Tests`, `IntegrationTests`) clean both before and after deleting the two obsolete stub-marker files.
- 864/864 unit tests passing (858 baseline + 6 new), verified with a fresh independent run both before and after the stub-marker deletion.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results (same open item as every prior module).
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for the new write path.
- The pre-existing API scaffold's Endpoints/Mapping/Telemetry/Diagnostics/Filters files remain unwired no-op stubs — not reconciled this pass, same "legacy scaffold left running alongside the new path" situation noted in `SafetyEvents.md`.

## ContinuousImprovement module: now fully modernized

With this pass, both of `ContinuousImprovement`'s sub-aggregates (`CAPA`/`CorrectiveAction` + `RCA`) have real guarded lifecycles, tenant-scoped ETag-aware write paths, and NotFound-mapped read queries. This closes out the `ContinuousImprovement` module entirely.
