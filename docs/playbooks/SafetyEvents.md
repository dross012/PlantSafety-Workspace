# SafetyEvents Module Playbook

Phase 4 domain-modernization pass, eighth module. Status as of 2026-07-30.

## Scope: six sub-aggregates, one already done

`SafetyEvents.IncidentManagement` was already the fully-modernized reference implementation this entire effort has been mirroring conventions from (`IIncidentRepository`, `usp_Incident_Update`'s TRY/CATCH+AuditLog+ErrorLog+ETag pattern, etc.) — untouched this pass, nothing to do there.

The other six sub-aggregates split into two starting states, same pattern as Scheduling:

- **EmergencyManagement, Observation** — full DDD scaffold already present (Aggregates/Enums/Events/Exceptions/Factories/Interfaces/Policies/Rules/Snapshots/Specifications/ValueObjects), but Create-only, and — worse than the Scheduling sub-aggregates — **zero Insert/Update SQL existed at all**, only a single list-read proc each. Also found: orphaned, non-functional `EmergencyManagementRepository.cs`/`ObservationRepository.cs` (+`ReadModel.cs` files) referencing stored procedures that never existed — same dead-scaffold pattern as `Compliance.Permits`'s orphaned `PermitsRepository.cs`. Deleted after confirming zero references.
- **CrisisManagement, DrillsAndExercises, NearMisses, RootCauseAnalysis** — literal empty placeholder marker classes (`public static class <Name>ModuleMarker { }`), same as `Operational.PredictiveMaintenance`/`PreventiveMaintenance` were. No Domain model existed at all; only a read-only list query per module.

## Domain models built this pass

- **EmergencyManagement**: Created → **RespondTo** → Active → **Resolve** → Completed → **Close** → Closed → **Archive** → Archived, plus a same-status **Stabilize** action (sets `StabilizedAtUtc`, already a scaffolded-but-unused field). "Highest business-continuity-priority Critical Module" per the aggregate's own header comment — the field names (`StabilizedAtUtc`, `ResolvedAtUtc`, `ResponseTeamNotified`) already documented the intended real lifecycle before this pass; the lifecycle methods just wired them up.
- **Observation**: Created → **Activate** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived.
- **CrisisManagement**: Declared → **RespondTo** → Active → **Resolve** → Completed → **Close** → Closed → **Archive** → Archived, plus same-status **Stabilize** — deliberately mirrors EmergencyManagement's shape since both are severity-based event-response aggregates in the same bounded context.
- **DrillsAndExercises**: Scheduled → **Conduct** → Conducted → **Complete** → Completed → **Close** → Closed → **Archive** → Archived, plus **Cancel** (Scheduled → Cancelled, a genuinely new status value — the pre-existing DTO's own doc comment documented "Scheduled/Conducted/Cancelled" as the real status vocabulary).
- **NearMisses**: Reported → **StartReview** → UnderReview → **Complete** → Completed → **Close** → Closed → **Archive** → Archived.
- **RootCauseAnalysis**: InProgress → **SubmitForReview** → PendingReview → **Approve** → Completed → **Close** → Closed → **Archive** → Archived.

All six status enums/vocabularies were grounded in each module's pre-existing read-model DTO doc comments (`Status (e.g., Reported/UnderReview/Closed)` etc.) rather than invented — the intended domain language was already documented, just never wired to real behavior.

## Execution: three parallel agents, six aggregates, zero manual fixes needed

Unlike the Scheduling pass (which needed one manual `using`-directive fix after three agents converged on a shared controller), all three SafetyEvents agents' work integrated cleanly with no intervention:

1. **EmergencyManagement + Observation** — mechanical build-out on top of domain models I wrote directly (both had existing scaffolding, just needed lifecycle methods).
2. **CrisisManagement + DrillsAndExercises** — full build from empty stubs, including complete domain-layer design (aggregate, enum, events, exceptions, rules, IDs) built by the agent from a detailed specification.
3. **NearMisses + RootCauseAnalysis** — same full-build treatment.

A genuine technical finding surfaced independently by two of the three agents: a same-simple-name collision between an aggregate class (e.g. `CrisisManagement`) and its own enclosing namespace segment (`...SafetyEvents.CrisisManagement.Persistence`) — an unqualified reference to the type binds to the namespace segment first, so a `using` alias with the same name doesn't resolve. Both agents independently worked around this by naming the write-side repository classes distinctly (`CrisisManagementAggregateDapperRepository`, `NearMissesAggregateDapperRepository`, etc.) rather than reusing the aggregate's bare name — consistent, sensible, and matches how `PermitWorkflowDapperRepository.cs` already handles the identical `Permits`/`Compliance.Permits.Persistence` collision.

One deliberate interface-shape deviation: the CrisisManagement/DrillsAndExercises agent added `GetByIdAsync` directly onto the pre-existing list-only Application repository interfaces rather than creating new same-simple-name-different-namespace interfaces (the `IPermitsRepository`-split precedent used everywhere else). Reasoning given: the simple names were already taken by the live list interfaces in that exact namespace, and extending was the smaller, more reversible choice. Noted as an inconsistency with the rest of this modernization effort's convention, not a defect — worth normalizing in a future pass if the split pattern is preferred long-term.

## Verified independently (not just trusting agent self-reports)

- All 6 write-side repositories: `outboxWriter.Enqueue` present exactly twice each (Add + Update).
- All 6: `ExecuteScalarAsync` + `ConcurrencyConflictException` present.
- 33 files use `ErrorSeverity.NotFound`; 27 have the `ConcurrencyConflictException` passthrough across the SafetyEvents Application layer.
- All three DI registration calls (`AddSafetyEventsWriteRepositories`, `AddSafetyEventsNearMissesRootCauseAnalysisInfrastructure`, `AddCrisisManagementDrillsAndExercisesInfrastructure`) present and coexisting in the central `InfrastructureServiceRegistration.cs`.
- Full solution build (`Api`, `IntegrationTests`) clean; 798/798 unit tests passing, verified with a fresh independent run.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results across any of the 6 sub-aggregates.
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for any of the newly built write paths.
- The Application-layer interface-shape inconsistency noted above (combined vs. split read interfaces) — not urgent, but a source of drift if left long-term.
- `SafetyEvents.Features.*` (the older parallel "Features" tree referenced in the DI file, e.g. `PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement`) exists alongside the new aggregate-backed path for several of these same sub-areas — same "legacy path left running alongside the new one" situation as `Compliance.Permits`'s `Features.Compliance.Permits`. Not reconciled this pass; flagged for a future decision on migration/deprecation.
