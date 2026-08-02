# Operational.PredictiveMaintenance Module Playbook

Phase 4 domain-modernization pass, fifth module (built in parallel with `Operational.PreventiveMaintenance`). Status as of 2026-07-30.

## Starting state: not anemic, non-existent

Even more bare than `Compliance.Permits`: there was no aggregate at all, not even a Create-only one. `PlantSafety.Domain\Operational\PredictiveMaintenance\PredictiveMaintenanceStub.cs` was a literal empty placeholder (`public static class PredictiveMaintenanceModuleMarker { }`). Only a read-only date-range list query existed (`GetPredictiveMaintenanceQuery` → `PredictiveMaintenanceItemDto` → `PredictiveMaintenanceDapperRepository` → `usp_PredictiveMaintenance_List` against `[Operational].[PredictiveMaintenanceReadModel]`), confirmed by the repo's own `docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md` §1.3.

Per the standing "full build-out" policy (see `feedback_anemic_modules_full_buildout` memory), this got a complete aggregate designed from scratch — grounded in the pre-existing read model's real columns (`EquipmentId`, `PredictedFailureType`, `Status`, `PredictedAtUtc`, `ReviewedBy`), not invented.

## Domain model

`PredictiveMaintenanceAlert` aggregate — models an ML/analytics-driven failure prediction through human triage to resolution:

Raised → **Review** → UnderReview → **ScheduleAction** → ActionScheduled → **Resolve** → Resolved → **Close** → Closed
Raised/UnderReview → **Dismiss** → Dismissed → **Close** → Closed

6 lifecycle methods, 6 domain events, guarded transitions — same conventions as `Compliance.Permits`/`PermitToWork`.

## Write path built alongside the existing read path

The existing `PredictiveMaintenanceReadModel` table had no audit columns at all and none of the new lifecycle columns. Extended additively (idempotent `ALTER TABLE ... IF NOT EXISTS` guards) rather than replaced — the pre-existing `usp_PredictiveMaintenance_List` / `GetPredictiveMaintenanceQuery` / `PredictiveMaintenanceDapperRepository` read-list path is untouched and still works, verified by the full test run.

New: `usp_PredictiveMaintenanceAlert_{Insert,Update,GetById}` (tenant-scoped, ETag-guarded, TRY/CATCH+AuditLog+ErrorLog), `PredictiveMaintenanceAlertDapperRepository` (write side, both `AddAsync`/`UpdateAsync` enqueue to outbox), 6 commands (`RaisePredictiveMaintenanceAlertCommand` + 5 lifecycle commands, all concurrency-aware and NotFound-severity-correct from day one), `GetPredictiveMaintenanceAlertByIdQuery`.

## API

New `PredictiveMaintenanceAlertV1Controller` at a nested route (`.../predictive-maintenance/alerts`) distinct from the pre-existing read-only `PredictiveMaintenanceV1Controller`/`V2Controller` at `.../predictive-maintenance` — deliberate choice to guarantee zero route/verb collision risk rather than editing a controller with an unknown V1/V2 versioning history.

## Tests

`ReviewPredictiveMaintenanceAlertCommandHandlerTests`, `GetPredictiveMaintenanceAlertByIdQueryHandlerTests` — same regression pattern as the other modules. Full suite verified independently at 761/761 passing (combined with the sibling `PreventiveMaintenanceTask` build built in parallel).

## Built in parallel with a sibling module

This and `Operational.PreventiveMaintenance` were built by two concurrent background agents, since they're fully independent in file scope. Both touched two shared files (`PlantSafety.Domain\GlobalUsing.cs`, `PlantSafety.Infrastructure\DependencyInjection\InfrastructureServiceRegistration.cs`) — verified independently after both completed that neither clobbered the other's registration.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results — clients re-fetch via the by-id query.
- No `PlantSafety.DataIntegrationTests`/`PlantSafety.IntegrationTests` coverage against a live SQL Server.
- `usp_PredictiveMaintenance_List.sql` was not extended to project the new lifecycle columns — the by-id query is the only path to full detail today; the list view still shows only the original 8 columns.
