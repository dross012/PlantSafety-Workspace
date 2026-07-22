# SafetyCulture Module

## Purpose
Provides the structured framework for measuring and reinforcing a plant's safety culture: the shared values, beliefs, behaviors, and expectations that shape how employees think about and act on safety. Covers periodic culture-health assessments and the non-punitive, employee-empowerment "Stop-Work Authority" record.

Source: PlantSafety_Program_Structure_extract.txt (legacy 19-module "PlantSafety Program Structure" document, "Safety Culture Module" section), scaffolded as a standalone top-level module alongside (not merged into) the existing PlantSafety.Domain / PlantSafety.Application module set. Note: this is a **separate bounded context** from the pre-existing `PlantSafety.Domain.Analytics.Culture` sub-module; per Handbook Part IX §9.4 "No Bleed" rule, the same real-world concept is independently modeled in each module rather than shared.

## Responsibilities
- Owns the `CultureAssessment` aggregate (a periodic culture-health assessment/survey: title, period, overall culture score, leadership engagement score, narrative summary) and its lifecycle (`CultureAssessmentStatus`: Created -> Active -> Completed -> Closed -> Archived).
- Owns the `StopWorkEvent` aggregate (a single, non-punitive exercise of employee Stop-Work Authority: work area, hazard description, initiating actor, resolution, optional recognition) and its identical lifecycle shape.
- Owns the `CultureMetricType` Smart Enum: the full 8-KPI taxonomy from the source document's "Culture Metrics & KPIs" section, each carrying its preferred trend direction (Rule 2.7).
- Provides Commands (CreateCultureAssessment, CreateStopWorkEvent), Queries (read-model projections per metric below), Validators, DTOs, and repository interfaces (Domain-layer, Dapper/stored-procedure-backed in Infrastructure -- not generated in this pass per the C#-only scope).
- Does NOT own SQL scripts, EF Core migrations, DbContext, or repository *implementations* -- those belong to a future Infrastructure-layer pass.

## Dependencies
Risk Management (hazard reporting, RCA, FMEA -- sibling module), Scorecards (cultural indicators: near-misses, observations -- sibling module), PPE (compliance and behavioral reinforcement -- sibling module), Scheduling (fatigue management and training calendars -- sibling module), Compliance/Permits (behavioral compliance during high-risk work), Observability (real-time behavior monitoring -- sibling module), Checklists (leadership walk-around documentation).

No direct project references to other modules' Domain/Application internals (Handbook Part IX "No Bleed" rule). All cross-module dependencies are conceptual/event-based only.

## CultureMetricType taxonomy (8, full source-document list)
NearMissReportingRate, StopWorkActions, PpeComplianceRate, BehavioralObservationCompletion, LeadershipEngagementFrequency, TrainingCompletionRate, SafetyMeetingParticipation, SafetySuggestionSubmissions.

## Application query areas (read-model projections, all 8 metrics)
NearMissReportingRate, StopWorkActions, PpeComplianceRate, BehavioralObservationCompletion, LeadershipEngagementFrequency, TrainingCompletionRate, SafetyMeetingParticipation, SafetySuggestionSubmissions.

## Inputs
- Commands: CreateCultureAssessmentCommand (TenantId, SiteId, OwnerId, AssessmentTitle, PeriodStartUtc, PeriodEndUtc, Summary), CreateStopWorkEventCommand (TenantId, SiteId, OwnerId, WorkArea, HazardDescription) -- both with the full command envelope (CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
- Queries: Get{Metric}Query(TenantId, SiteId, From, To) for each read-model area listed above.

## Outputs
- CreateCultureAssessmentResult / CreateStopWorkEventResult (aggregate Id, Status) on command success.
- {Metric}QueryResult (TenantId, SiteId, GeneratedAt, Items) on query success.
- All handlers return the canonical Result<T> discriminated union; no exceptions cross the Application boundary except truly exceptional conditions.

## Events emitted
- CultureAssessmentCreatedEvent, StopWorkEventCreatedEvent (EventId, aggregate Id, CreatedAtUtc, Actor, CorrelationId, CausationId) -- raised by the respective aggregate's `Create(...)` factory method. Additional lifecycle events (Activated/Completed/Closed/Archived) are additive extension points for a future pass; only the Created event is materialized in this scaffold.

## Events consumed
- None in this C#-only Domain+Application scaffold. Cross-module event subscriptions (e.g. SafetyCulture reacting to Scorecards' near-miss trend events) belong to the Infrastructure/messaging layer and are out of scope for this pass.

## Integration points
- Repository interfaces (ICultureAssessmentRepository, ICultureAssessmentFactory, IStopWorkEventRepository, IStopWorkEventFactory, and one I{Metric}Repository per query area) are Domain/Application-layer *contracts only*. Concrete Dapper/stored-procedure implementations, EF Core, migrations, and API controllers are intentionally NOT generated in this pass (C#-only Domain+Application scope; no SQL).
- DI registration: SafetyCultureApplicationServiceRegistration.AddSafetyCultureApplicationServices(IServiceCollection) is a self-contained extension method. It is NOT wired into the shared PlantSafety.Application.DependencyInjection.ApplicationServiceRegistration composition root in this pass, to avoid merge collisions with other concurrently-running module-scaffolding agents editing that same shared file. Wiring it in (one additive line under the existing `.AddXxxApplicationServices()` chain) is a follow-up integration step for whoever finalizes composition-root wiring.

## Structural notes / deviations from the Compliance template
- Two Domain aggregates (CultureAssessment, StopWorkEvent) reflecting the source document's two most operationally distinct, individually-trackable safety-culture record types (a periodic rollup assessment vs. a discrete, timestamped employee action); the other 6 documented culture "pillars" (Visible Leadership Commitment, Just Reporting System, Consistent Communication, Training/Competency/Accountability, Recognition & Reinforcement, Continuous Improvement & Learning Culture) are modeled as narrative/behavioral inputs that roll up into `CultureAssessment.OverallCultureScore` and the `CultureMetricType` KPI taxonomy rather than as separate aggregates, since the source document does not describe them as independently persisted records with their own lifecycle.
- All 8 documented culture KPIs received a dedicated Application-layer read-model query area (matching the source document's exhaustive, small (8-item) metrics list).
- `StopWorkEvent.InitiatedByActor` uses `PerformedByActor` (not `UserId`) per Handbook Rule 1.8 (Actor & User Identity Semantics), since exercising Stop-Work Authority is a domain action, not CRUD authorship; `CreatedBy` still carries the CRUD-authorship `UserId` for the record itself.
- Deliberately namespaced as a top-level `PlantSafety.Domain.SafetyCulture` / `PlantSafety.Application.SafetyCulture` module, distinct from `PlantSafety.Domain.Analytics.Culture`, to satisfy both this task's "own new top-level folder" instruction and the Handbook's "No Bleed" rule. No type-name collisions result: the pre-existing Analytics sub-module's aggregate is named `Culture`; this module's aggregates are named `CultureAssessment` and `StopWorkEvent`.
