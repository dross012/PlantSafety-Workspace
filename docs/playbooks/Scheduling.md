# Scheduling Module Playbook

Phase 4 domain-modernization pass, seventh module. Status as of 2026-07-30.

## Mixed starting state — a genuine split, not uniform

Confirmed via the repo's own `docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md`: unlike prior modules, Scheduling's five sub-aggregates were NOT uniformly anemic:

- **PMScheduling, WorkOrderPriority** — already had real, working load-mutate-save command handlers (`SchedulePreventiveMaintenanceCommandHandler`, `RecordPMSchedulingCompletionCommandHandler`, `EscalatePMSchedulingCommandHandler`, `EscalateWorkOrderPriorityCommandHandler`, `ResolveWorkOrderPriorityCommandHandler`). Needed the same audit-and-fix pattern as PermitToWork/RiskManagement/AccidentManagement, not a full build-out.
- **EmergencyDrillPlanning, HazardIsolationSequencing, ShiftFatigue** — Create-only aggregates, full SQL/repository scaffolding already present (unlike the Operational.*Maintenance modules, which had nothing) but zero Application-layer commands wired. Needed the full build-out treatment, same as Compliance.Permits.

DI was already live (`AddSchedulingInfrastructure()` wired into the central `InfrastructureServiceRegistration.cs`) — the blueprint's 2026-07-27 note that it wasn't yet wired turned out to be stale by the time this pass started, same pattern discovered during the Compliance.Permits pass.

## Domain models built this pass (3 aggregates)

- **EmergencyDrillPlanning**: Created → **Conduct** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived.
- **HazardIsolationSequencing**: Created → **Activate** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived, plus a same-status **FlagConflict** action (sets `ConflictDetected`, no status change).
- **ShiftFatigue**: Created → **Activate** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived, plus a same-status **ReassessRisk** action (updates `FatigueRiskScore`/`ConsecutiveNightShifts`/`OvertimeHours`).

All three reused their pre-existing (previously dead) `Created/Active/Completed/Closed/Archived` status enum and `Can*` lifecycle-rule scaffolding rather than inventing richer bespoke statuses — kept proportionate given this pass covered 5 sub-aggregates in one go. Each got its own genuinely domain-meaningful method names (not generic "Activate"/"Complete" everywhere) and reused each aggregate's own already-scaffolded exception types.

## Fixes applied to the 2 mature aggregates (PMScheduling, WorkOrderPriority)

Same fix set as RiskManagement/AccidentManagement: tenant-scoped `GetByIdAsync`, ETag-guarded `UpdateAsync` (`ExecuteScalarAsync` + `ConcurrencyConflictException`), `ErrorSeverity.NotFound` on NotFound errors, `catch (ConcurrencyConflictException) { throw; }` before generic catches, and a by-id read query added where none existed.

## Execution note: three parallel agents, one shared file, one trivial casualty

Built via three concurrent background agents (PMScheduling+WorkOrderPriority; EmergencyDrillPlanning; HazardIsolationSequencing+ShiftFatigue) since the five sub-aggregates are otherwise file-disjoint. All three converged on the same shared `PlantSafety.Api\Scheduling\Controllers\V1\SchedulingV1Controller.cs` (all 5 sub-aggregates share one controller by existing convention) and the central `InfrastructureServiceRegistration.cs`/`SchedulingInfrastructureServiceRegistration.cs`.

All three agents hit a hard session/usage-limit termination near the very end of their work (during final self-verification, after their actual file edits were already complete) and reported as "failed." Rather than treat that label as ground truth, the actual repository state was inspected directly: all five sub-aggregates' commands, queries, repositories, and SQL were present and consistent. The only real casualty of the concurrent edits was one missing `using` directive in the shared controller (`PMSchedulingItemDto` referenced without importing its `Models` namespace) — fixed directly, one line. Full build across `PlantSafety.Api`/`PlantSafety.Tests`/`PlantSafety.IntegrationTests` and a full test run (766/766 passing) confirmed the rest of the concurrent work was clean.

## Verified independently (not just trusting agent self-reports)

- All 5 repositories: `outboxWriter.Enqueue` present exactly twice each (Add + Update) — no repeat of the platform-wide outbox-drop bug.
- All 5 repositories: `ExecuteScalarAsync` + `ConcurrencyConflictException` present.
- 24 files across the module use `ErrorSeverity.NotFound`; 18 have the `ConcurrencyConflictException` passthrough (matches the expected count: 2+2+4+5+5 = 18 lifecycle handlers across the five sub-aggregates).
- `SchedulingInfrastructureServiceRegistration.cs` registers all 5 sub-aggregates' write-side AND read-side repository interfaces with fully-qualified names (same-simple-name disambiguation pattern used throughout this modernization effort).

## Explicitly out of scope (left alone)

`BulkUpdateSchedulingCommand`/`BulkUpdateSchedulingCommandHandler`/`ISchedulingBulkRepository` — the cross-cutting bulk-operations path that bypasses aggregates entirely, same category as PermitToWork's and RiskManagement's bulk paths, consistently left untouched across every module pass in this effort.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results across any of the 5 sub-aggregates — clients re-fetch via the by-id queries.
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for the newly added `@ETag`/`@TenantId` guards or the 3 newly built lifecycles.
- `usp_Scheduling_BulkUpdateResource` (the bulk path) was not audited for the same cross-tenant/concurrency concerns found and fixed elsewhere — flagged, not assumed safe.
