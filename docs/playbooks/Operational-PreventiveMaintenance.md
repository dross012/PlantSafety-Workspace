# Operational.PreventiveMaintenance Module Playbook

Phase 4 domain-modernization pass, sixth module (built in parallel with `Operational.PredictiveMaintenance` — easy to confuse the two names, note the "Pre**vent**ive" vs "Pre**dict**ive" spelling). Status as of 2026-07-30.

## Starting state: not anemic, non-existent

Same starting shape as its sibling: `PreventiveMaintenanceStub.cs` was an empty placeholder marker class, and only a read-only date-range list query existed (`GetPreventiveMaintenanceQuery` → `PreventiveMaintenanceItemDto` → `PreventiveMaintenanceDapperRepository` → `usp_PreventiveMaintenance_List` against `[Operational].[PreventiveMaintenanceReadModel]`), per the same architecture blueprint finding. Full build-out per the standing policy, aggregate grounded in the pre-existing read model's real columns (`EquipmentId`, `TaskDescription`, `Status`, `ScheduledAtUtc`, `CompletedBy`).

## Domain model

`PreventiveMaintenanceTask` aggregate — models a scheduled maintenance task through execution to verification:

Scheduled → **Start** → InProgress → **Complete** → Completed → **Verify** → Verified
Scheduled/InProgress → **Cancel** → Cancelled
Scheduled → **MarkOverdue** → Overdue

6 lifecycle methods, 6 domain events, guarded transitions.

## Write path built alongside the existing read path

Extended the existing `PreventiveMaintenanceReadModel` table additively — same idempotent `ALTER TABLE` approach as its sibling. One extra wrinkle here: the pre-existing `CompletedBy` column was `NOT NULL` in the original DDL, which can't hold true once a task can be Scheduled/InProgress/Cancelled/Overdue with no completer yet. Fixed with a guarded `ALTER COLUMN ... NULL`, and the pre-existing `PreventiveMaintenanceItemDto.CompletedBy` (used by the untouched list query) was correspondingly widened from `Guid` to `Guid?` — a small, necessary, additive change to keep that existing read path from throwing once real non-completed rows exist, not a scope creep.

New: `usp_PreventiveMaintenanceTask_{Insert,Update,GetById}` (tenant-scoped, ETag-guarded, TRY/CATCH+AuditLog+ErrorLog), `PreventiveMaintenanceTaskDapperRepository` (write side, both `AddAsync`/`UpdateAsync` enqueue to outbox), 6 commands (`SchedulePreventiveMaintenanceTaskCommand` + 5 lifecycle commands, concurrency-aware and NotFound-severity-correct from day one), `GetPreventiveMaintenanceTaskByIdQuery`.

## API

Unlike its sibling, the new actions were added directly to the existing `PreventiveMaintenanceV1Controller.cs` — that controller already served the read-only list endpoint and follows this API area's established single-controller-per-leaf-module convention, so extending it (rather than creating a new nested-route controller) was the right call here. New routes: `POST .../preventive-maintenance`, `POST .../{id}/{start|complete|verify|cancel|mark-overdue}`, `GET .../{id}`. The pre-existing `GET .../preventive-maintenance` list route is untouched.

## Tests

`StartPreventiveMaintenanceTaskCommandHandlerTests`, `GetPreventiveMaintenanceTaskByIdQueryHandlerTests` — same regression pattern as the other modules. Full suite verified independently at 761/761 passing.

## Built in parallel with a sibling module

Built concurrently with `Operational.PredictiveMaintenance` by a separate background agent — see that module's playbook for the shared-file reconciliation note (`GlobalUsing.cs`, `InfrastructureServiceRegistration.cs`, both survived intact, verified independently).

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results — clients re-fetch via the by-id query.
- No `PlantSafety.DataIntegrationTests`/`PlantSafety.IntegrationTests` coverage against a live SQL Server.
- `usp_PreventiveMaintenance_List.sql` was not extended to project the new lifecycle columns — the by-id query is the only path to full detail today.
