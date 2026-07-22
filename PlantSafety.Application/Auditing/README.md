# Auditing Module

## Purpose
Provides a structured, repeatable, compliance-driven method for evaluating manufacturing operations, safety systems, environmental controls, and regulatory adherence: audit engagements across 8 disciplines (Operations, Systems, Procedures, Quality Control, Regulatory Compliance, Efficiency, Inspecting, Facility Assessment) and the findings/corrective-actions they raise.

**Important clarification (per source document):** This module covers strictly **feature-level audits** of manufacturing operations, safety processes, regulatory compliance, and facility conditions. It is explicitly **NOT** an application audit, software audit, cybersecurity audit, or code review, and does not evaluate the PlantSafety application itself.

Source: PlantSafety_Program_Structure_extract.txt (legacy 19-module "PlantSafety Program Structure" document, "Checklists & Auditing" section and dedicated "Auditing Module" section), scaffolded as a standalone top-level module alongside (not merged into) the existing PlantSafety.Domain / PlantSafety.Application module set. Note: this is a **separate bounded context** from the pre-existing `PlantSafety.Domain.Compliance.Auditing` sub-module (Compliance's own audit-program tracking); per Handbook Part IX §9.4 "No Bleed" rule, the same real-world concept is independently modeled in each module rather than shared.

## Responsibilities
- Owns the `AuditEngagement` aggregate (an audit program/execution record: title, discipline, scope, lead auditor, schedule, findings count, compliance score/rating) and its lifecycle (`AuditEngagementStatus`: Created -> Active -> Completed -> Closed -> Archived), which corresponds to the source document's 6-step Audit Workflow (Planning -> Execution -> Findings -> Corrective Actions -> Reporting -> Continuous Improvement).
- Owns the `AuditFinding` aggregate (a non-conformance/finding raised during an engagement: title, severity, corrective-action assignment, due date, verification) and its identical lifecycle shape.
- Owns the `AuditDisciplineType` Smart Enum: the full 8-discipline taxonomy from the source document, each carrying its typical output artifacts (Rule 2.7).
- Provides Commands (CreateAuditEngagement, CreateAuditFinding), Queries (read-model projections per discipline below), Validators, DTOs, and repository interfaces (Domain-layer, Dapper/stored-procedure-backed in Infrastructure -- not generated in this pass per the C#-only scope).
- Does NOT own SQL scripts, EF Core migrations, DbContext, or repository *implementations* -- those belong to a future Infrastructure-layer pass.

## Dependencies
Checklists (daily/weekly inspection data feeds audit evidence), Observability (sensor-based audit evidence -- sibling module), Compliance/Permits (Permit-to-Work compliance checks), Scheduling (audit calendars -- sibling module), SafetyCulture (leadership engagement and reporting), Scorecards (audit completion and findings metrics -- sibling module).

No direct project references to other modules' Domain/Application internals (Handbook Part IX "No Bleed" rule). All cross-module dependencies are conceptual/event-based only.

## AuditDisciplineType taxonomy (8, full source-document list)
Operations, Systems, Procedures, QualityControl, RegulatoryCompliance, Efficiency, Inspecting, FacilityAssessment.

## Application query areas (read-model projections, all 8 disciplines)
Operations, Systems, Procedures, QualityControl, RegulatoryCompliance, Efficiency, Inspecting, FacilityAssessment.

## Inputs
- Commands: CreateAuditEngagementCommand (TenantId, SiteId, OwnerId, EngagementTitle, DisciplineType, Scope, LeadAuditorId, ScheduledAtUtc), CreateAuditFindingCommand (TenantId, SiteId, OwnerId, AuditEngagementId, FindingTitle, Severity, CorrectiveActionRequired, AssignedToUserId, DueDateUtc) -- both with the full command envelope (CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
- Queries: Get{Discipline}Query(TenantId, SiteId, From, To) for each read-model area listed above.

## Outputs
- CreateAuditEngagementResult / CreateAuditFindingResult (aggregate Id, Status) on command success.
- {Discipline}QueryResult (TenantId, SiteId, GeneratedAt, Items) on query success.
- All handlers return the canonical Result<T> discriminated union; no exceptions cross the Application boundary except truly exceptional conditions.

## Events emitted
- AuditEngagementCreatedEvent, AuditFindingCreatedEvent (EventId, aggregate Id, CreatedAtUtc, Actor, CorrelationId, CausationId) -- raised by the respective aggregate's `Create(...)` factory method. Additional lifecycle events (Activated/Completed/Closed/Archived) are additive extension points for a future pass; only the Created event is materialized in this scaffold.

## Events consumed
- None in this C#-only Domain+Application scaffold. Cross-module event subscriptions (e.g. Auditing reacting to Checklists' completion events as evidence) belong to the Infrastructure/messaging layer and are out of scope for this pass.

## Integration points
- Repository interfaces (IAuditEngagementRepository, IAuditEngagementFactory, IAuditFindingRepository, IAuditFindingFactory, and one I{Discipline}Repository per query area) are Domain/Application-layer *contracts only*. Concrete Dapper/stored-procedure implementations, EF Core, migrations, and API controllers are intentionally NOT generated in this pass (C#-only Domain+Application scope; no SQL).
- DI registration: AuditingApplicationServiceRegistration.AddAuditingApplicationServices(IServiceCollection) is a self-contained extension method. It is NOT wired into the shared PlantSafety.Application.DependencyInjection.ApplicationServiceRegistration composition root in this pass, to avoid merge collisions with other concurrently-running module-scaffolding agents editing that same shared file. Wiring it in (one additive line under the existing `.AddXxxApplicationServices()` chain) is a follow-up integration step for whoever finalizes composition-root wiring.

## Structural notes / deviations from the Compliance template
- Two Domain aggregates (AuditEngagement, AuditFinding) modeling the engagement-vs-finding split explicit in the source document's Audit Workflow ("Step 3 -- Findings" through "Step 4 -- Corrective Actions" are a distinct sub-lifecycle from the parent engagement).
- All 8 documented audit disciplines received a dedicated Application-layer read-model query area (matching the source document's exhaustive, small (8-item) discipline list -- no scope reduction was necessary here, unlike the 20-category Checklists module).
- Deliberately namespaced as a top-level `PlantSafety.Domain.Auditing` / `PlantSafety.Application.Auditing` module, distinct from `PlantSafety.Domain.Compliance.Auditing`, to satisfy both this task's "own new top-level folder" instruction and the Handbook's "No Bleed" rule (a concept appearing in two modules must be independently modeled in each). No type-name collisions result: the pre-existing Compliance sub-module's aggregate is named `Auditing`; this module's aggregates are named `AuditEngagement` and `AuditFinding`.
