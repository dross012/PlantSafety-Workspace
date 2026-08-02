# Analytics Module Playbook

Phase 4 domain-modernization pass, ninth module. Status as of 2026-07-30.

## Scope: five sub-aggregates, one partial reference implementation

All five (`Culture`, `Dashboard`, `Engagement`, `PredictiveAnalytics`, `ScoreCards`) had full Domain-layer DDD scaffolding (Aggregates/Enums/Events/Exceptions/Factories/Interfaces/Policies/Rules/Snapshots/Specifications/ValueObjects) but only a `Create` factory — no other lifecycle methods existed anywhere. Unlike SafetyEvents, none were literal empty stubs, but maturity still varied:

- **Culture** had a genuinely good `Create` write path already (a prior "Culture reference implementation: full canonical pattern" commit had built it to a high standard) — extended, not replaced.
- **Dashboard, PredictiveAnalytics, ScoreCards** had `Create` only, no Application-layer commands wired for the aggregate itself (though PredictiveAnalytics had a partial read-only list stack).
- **Engagement** was the most extreme case found in this entire modernization effort: a fully-built Domain aggregate with **zero references anywhere in the Application layer** — not even a read query. Confirmed the pre-existing `EngagementV1Controller`/`GetEngagementQuery` in the API were an unrelated, differently-scoped generic engagement-metric feature, not this aggregate at all.

All five reused the identical generic `Created→Active→Completed→Closed→Archived` status enum and `Can*` guard scaffolding already present (unchanged), with each aggregate given domain-appropriate method names on top of the same shape:

## Domain models built this pass

- **Culture**: Created → **Review** → Active → **Publish** → Completed → **Close** → Closed → **Archive** → Archived.
- **Dashboard**: Created → **Publish** → Active → **Deprecate** → Completed → **Close** → Closed → **Archive** → Archived, plus same-status **RefreshData** (sets `LastRefreshedAtUtc`).
- **Engagement**: Created → **Review** → Active → **Complete** → Completed → **Close** → Closed → **Archive** → Archived, plus same-status **RecordTouchpoint** (updates `LastEngagedAtUtc`/`ResponseCount`).
- **PredictiveAnalytics**: Created → **Review** → Active → **Action** → Completed → **Close** → Closed → **Archive** → Archived — vocabulary ("Reviewed"/"Actioned") grounded in the pre-existing read DTO's own doc comment.
- **ScoreCards**: Created → **Publish** → Active → **Finalize** → Completed → **Close** → Closed → **Archive** → Archived.

## Execution: three parallel agents, five aggregates, all clean

1. **Culture + Dashboard** — Culture extended (not rebuilt) since its Create path was already good; Dashboard built from nothing.
2. **Engagement + PredictiveAnalytics** — Engagement built entirely from scratch (confirmed zero prior wiring); PredictiveAnalytics extended its partial read stack with a real write path.
3. **ScoreCards** — full write-path build, explicitly leaving its many read-only category sub-features (Compliance, Department, Supervisor, Training) untouched.

Same recurring cross-cutting finding as SafetyEvents: the aggregate-class/namespace-segment name collision (e.g. `Dashboard` the class vs. `...Analytics.Dashboard.Persistence` the namespace) surfaced again in every agent's work and was independently worked around the same way each time (`<Name>AggregateDapperRepository` naming) — now a well-established pattern across this whole modernization effort.

**A worthwhile deviation the Culture agent made correctly**: my task instructions assumed a Domain-layer write interface following the pattern used everywhere else, but the agent investigated and found Culture's actual live write-side abstraction is `PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository` — a different location — confirmed via the real DI registration rather than my assumption. It adapted correctly instead of building a second, unused interface.

**A second orphaned dead-repository pair** was found (`ScoreCardsRepository.cs`/`ScoreCardsReadModel.cs`, referencing nonexistent stored procedures) — same pattern as `Compliance.Permits` and the two `SafetyEvents` orphans. The ScoreCards agent correctly avoided colliding with it but left deletion out of its stated scope; deleted directly after the independent verification pass confirmed zero live references.

## Verified independently (not just trusting agent self-reports)

- All 5 write-side repositories: `outboxWriter.Enqueue` present exactly twice each (Add + Update).
- All 5: `ExecuteScalarAsync` + `ConcurrencyConflictException` present.
- 27 files use `ErrorSeverity.NotFound`; 22 have the `ConcurrencyConflictException` passthrough across the Analytics Application layer.
- All 5 aggregates' repositories confirmed registered in DI (`AddScoreCardsAggregateInfrastructure`, `AddEngagementAggregateInfrastructure`, `AddPredictiveAnalyticsAggregateInfrastructure`, plus `IDashboardRepository`/Culture's interface via `AnalyticsLegacyRepositoryRegistration.cs`).
- Full solution build (`Api`, `IntegrationTests`) clean; 823/823 unit tests passing on a fresh independent run, both before and after deleting the orphaned ScoreCards files.

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results across any of the 5 sub-aggregates.
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for any of the newly built write paths.
- The many read-only category sub-features under Culture/Dashboard/ScoreCards (Behavior, CultureIndex, EngagementScoring, Executive, KPISnapshots, Compliance, Department, Supervisor, Training, etc.) were not audited this pass — same open question as RiskManagement's/AccidentManagement's category features from earlier in this effort.
- `PredictiveAnalytics`'s pre-existing business properties (`PredictedRiskScore`, `ConfidenceLevel`, `PredictionType`, `TargetEntityId`, `PredictionHorizonDays`) were kept as-is rather than reshaped to the simpler `Score`/`ModelName` vocabulary my original task spec assumed — the agent correctly deferred to what already existed rather than restructuring live properties beyond the stated scope.
