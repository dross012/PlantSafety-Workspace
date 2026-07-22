# Scorecards Module

## Purpose
Provides a unified, data-driven view of manufacturing plant safety performance, combining lagging indicators (TRIR, DART, lost workdays), leading indicators (training/audit/CAPA/PM compliance), and cultural/precursor metrics (near-miss reporting, behavioral observations, PPE compliance) into scorecard summary, panel, data-source, and reporting views.

Source: PlantSafety_Program_Structure_extract.txt (legacy 19-module "PlantSafety Program Structure" document), scaffolded as a standalone top-level module alongside (not merged into) the existing PlantSafety.Domain / PlantSafety.Application module set.

## Responsibilities
- Owns the Scorecard aggregate root, its lifecycle (ScorecardStatus: Created -> Active -> Completed -> Closed -> Archived), and the IndicatorCategory Smart Enum taxonomy.
- Provides Commands (CreateScorecard), Queries (read-model projections per category below), Validators, DTOs, and repository interfaces (Domain-layer, Dapper/stored-procedure-backed in Infrastructure -- not generated in this pass per the C#-only scope).
- Does NOT own SQL scripts, EF Core migrations, DbContext, or repository *implementations* -- those belong to a future Infrastructure-layer pass.

## Dependencies
AccidentManagement (TRIR/DART/lost workdays), Training, Auditing, Machines, Procedures, Contractors, Observability, RiskManagement -- Scorecards is a pure aggregation/read-model consumer of these modules' integration events.

No direct project references to other modules' Domain/Application internals (Handbook Part IX "No Bleed" rule). All cross-module dependencies are conceptual/event-based only.

## IndicatorCategory taxonomy
- `Lagging` -- Lagging Indicators (Reactive Outcomes)
- `Leading` -- Leading Indicators (Proactive Activities)
- `Cultural` -- Cultural & Precursor Metrics

## Application query areas (read-model projections)
- **LaggingIndicators** -- Lagging Indicators (TRIR, DART, Lost Workdays)
- **LeadingIndicators** -- Leading Indicators (Training, Audit, CAPA closure, PM compliance)
- **CulturalPrecursorMetrics** -- Cultural & Precursor Metrics (Near-Miss, Behavioral Observations)
- **ScorecardStructureVisualization** -- Scorecard Structure & Visualization Panels
- **ScorecardDataSources** -- Scorecard Data Sources (cross-module aggregation)
- **ReportingDistribution** -- Reporting & Distribution

## Inputs
- Commands: CreateScorecardCommand (TenantId, SiteId, ScorecardTitle, Category, OwnerId, EffectiveAtUtc, ReviewRequired) with full command envelope (CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
- Queries: Get{Category}Query(TenantId, SiteId, From, To) for each read-model area listed above.

## Outputs
- CreateScorecardResult (ScorecardId, Status) on command success.
- {Category}QueryResult (TenantId, SiteId, GeneratedAt, Items) on query success.
- All handlers return the canonical Result<T> discriminated union; no exceptions cross the Application boundary except truly exceptional conditions (Handbook Part XIV.4).

## Events emitted
- ScorecardCreatedEvent (EventId, ScorecardId, CreatedAtUtc, Actor, CorrelationId, CausationId) -- raised by Scorecard.Create(...). Additional lifecycle events (Activated/Completed/Closed/Archived) are additive extension points for a future pass; only the Created event is materialized in this scaffold.

## Events consumed
- None in this C#-only Domain+Application scaffold. Cross-module event subscriptions (e.g. Scorecards reacting to other modules' integration events) belong to the Infrastructure/messaging layer and are out of scope for this pass.

## Integration points
- Repository interfaces (IScorecardRepository, IScorecardFactory, and one I{Category}Repository per query area) are Domain/Application-layer *contracts only*. Concrete Dapper/stored-procedure implementations, EF Core, migrations, and API controllers are intentionally NOT generated in this pass (C#-only Domain+Application scope; no SQL).
- DI registration: ScorecardsApplicationServiceRegistration.AddScorecardsApplicationServices(IServiceCollection) is a self-contained extension method. It is NOT wired into the shared PlantSafety.Application.DependencyInjection.ApplicationServiceRegistration composition root in this pass, to avoid merge collisions with other concurrently-running module-scaffolding agents editing that same shared file. Wiring it in (one additive line under the existing .AddXxxApplicationServices() chain) is a follow-up integration step for whoever finalizes composition-root wiring.

## Structural notes / deviations from the Compliance template
- One Domain aggregate per module (rather than Compliance's several aggregates per module) -- each of these four modules maps to a single cohesive bounded concept per the source document, whereas Compliance spans eight historically-distinct sub-domains (Auditing, Certification, Compliance, ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar).
- Category counts are scaled to the module's own source-document content: SafetyPolicies (17, one per documented policy category), RiskManagement (11, one per documented risk-management component), Scorecards (6, one per documented indicator group/structure/reporting section), AccidentManagement (4, kept intentionally small because the source section is comparatively thin).
- Domain files use explicit per-file using directives rather than being added to the shared PlantSafety.Domain/GlobalUsing.cs, again to avoid concurrent-agent merge collisions on that shared file. This is a pure additive-safety choice; functionally equivalent to a global-using entry.