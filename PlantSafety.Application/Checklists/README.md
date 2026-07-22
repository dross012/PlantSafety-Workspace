# Checklists Module

## Purpose
Provides the structured, repeatable checklist/inspection-form framework used across the plant floor to standardize recurring safety inspections (Machine Guarding & LOTO, PPE, Hazardous Materials, Emergency Systems, Confined Space Entry, Hot Work, Working at Heights, Contractor Safety, and 12 further categories) and to capture their completions.

Source: PlantSafety_Program_Structure_extract.txt (legacy 19-module "PlantSafety Program Structure" document, "Checklists & Auditing" section and "Comprehensive Manufacturing Safety Checklists" appendix), scaffolded as a standalone top-level module alongside (not merged into) the existing PlantSafety.Domain / PlantSafety.Application module set.

## Responsibilities
- Owns the `ChecklistTemplate` aggregate (a checklist definition/category record: title, category, description, item count, mandatory flag, regulatory reference) and its lifecycle (`ChecklistTemplateStatus`: Created -> Active -> Completed -> Closed -> Archived).
- Owns the `ChecklistCompletion` aggregate (a single completed/in-progress execution of a template at a site: item pass/fail counts, overall outcome) and its identical lifecycle shape.
- Owns the `ChecklistCategory` Smart Enum: the full 20-category taxonomy from the source document (Rule 2.7 -- classification with metadata/dashboard grouping behavior).
- Provides Commands (CreateChecklistTemplate, CreateChecklistCompletion), Queries (read-model projections per category below), Validators, DTOs, and repository interfaces (Domain-layer, Dapper/stored-procedure-backed in Infrastructure -- not generated in this pass per the C#-only scope).
- Does NOT own SQL scripts, EF Core migrations, DbContext, or repository *implementations* -- those belong to a future Infrastructure-layer pass.

## Dependencies
Auditing (leadership walk-around documentation feeds audit evidence), SafetyCulture (checklist completion is a culture KPI input), Scheduling (checklist calendars -- owned by a sibling agent), PPE (PPE checklist category).

No direct project references to other modules' Domain/Application internals (Handbook Part IX "No Bleed" rule). All cross-module dependencies are conceptual/event-based only.

## ChecklistCategory taxonomy (20, full source-document list)
MachineGuardingAndLoto, PersonalProtectiveEquipment, HazardousMaterialsAndChemicalSafety, EmergencySystemsAndLifeSafety, EnvironmentalAndHousekeeping, MaterialHandlingEquipment, ElectricalSafety, ConfinedSpaceEntry, HotWork, WorkingAtHeights, ContractorSafety, MachineMaintenanceAndPm, FacilityAndTrafficControl, FireSafetyAndPrevention, ErgonomicsAndWorkstation, IncidentAndNearMissReporting, EnvironmentalCompliance, SecurityAndAccessControl, EmergencyDrill, ObservabilityAndSensorHealth.

## Application query areas (read-model projections, 8 of the 20 categories)
MachineGuardingAndLoto, PersonalProtectiveEquipment, HazardousMaterialsAndChemicalSafety, EmergencySystemsAndLifeSafety, ConfinedSpaceEntry, HotWork, WorkingAtHeights, ContractorSafety -- see "Structural notes" below for why only 8 of the 20 categories received a dedicated read-model query area.

## Inputs
- Commands: CreateChecklistTemplateCommand (TenantId, SiteId, OwnerId, TemplateTitle, Category, Description, ItemCount, IsMandatory, RegulatoryReference), CreateChecklistCompletionCommand (TenantId, SiteId, OwnerId, ChecklistTemplateId, ItemsTotal, ItemsPassed, ItemsFailed, Outcome) -- both with the full command envelope (CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
- Queries: Get{Category}Query(TenantId, SiteId, From, To) for each read-model area listed above.

## Outputs
- CreateChecklistTemplateResult / CreateChecklistCompletionResult (aggregate Id, Status) on command success.
- {Category}QueryResult (TenantId, SiteId, GeneratedAt, Items) on query success.
- All handlers return the canonical Result<T> discriminated union; no exceptions cross the Application boundary except truly exceptional conditions.

## Events emitted
- ChecklistTemplateCreatedEvent, ChecklistCompletionCreatedEvent (EventId, aggregate Id, CreatedAtUtc, Actor, CorrelationId, CausationId) -- raised by the respective aggregate's `Create(...)` factory method. Additional lifecycle events (Activated/Completed/Closed/Archived) are additive extension points for a future pass; only the Created event is materialized in this scaffold.

## Events consumed
- None in this C#-only Domain+Application scaffold. Cross-module event subscriptions (e.g. Checklists reacting to Scheduling's calendar events) belong to the Infrastructure/messaging layer and are out of scope for this pass.

## Integration points
- Repository interfaces (IChecklistTemplateRepository, IChecklistTemplateFactory, IChecklistCompletionRepository, IChecklistCompletionFactory, and one I{Category}Repository per query area) are Domain/Application-layer *contracts only*. Concrete Dapper/stored-procedure implementations, EF Core, migrations, and API controllers are intentionally NOT generated in this pass (C#-only Domain+Application scope; no SQL).
- DI registration: ChecklistsApplicationServiceRegistration.AddChecklistsApplicationServices(IServiceCollection) is a self-contained extension method. It is NOT wired into the shared PlantSafety.Application.DependencyInjection.ApplicationServiceRegistration composition root in this pass, to avoid merge collisions with other concurrently-running module-scaffolding agents editing that same shared file. Wiring it in (one additive line under the existing `.AddXxxApplicationServices()` chain) is a follow-up integration step for whoever finalizes composition-root wiring.

## Structural notes / deviations from the Compliance template
- Two Domain aggregates (ChecklistTemplate, ChecklistCompletion) rather than Compliance's several -- the source document's checklist content splits cleanly into "what to check" (template/category) and "what happened when someone checked it" (completion).
- Only 8 of the 20 documented checklist categories were given a dedicated Application-layer read-model query area (Handlers/Models/Queries/Repositories folder set), covering the highest life-safety-critical categories (Machine Guarding & LOTO, PPE, Hazardous Materials, Emergency Systems, Confined Space Entry, Hot Work, Working at Heights, Contractor Safety). The full 20-category taxonomy IS fully modeled as the `ChecklistCategory` Smart Enum on the Domain layer and is a valid value for `ChecklistTemplate.Category`; the remaining 12 categories can receive their own query-area folder additively (Handbook additive-only evolution) without any structural change once real read-model needs are identified. This keeps the generated surface proportionate rather than mechanically duplicating a folder set 20 times for what is fundamentally one enum's worth of variation.
- Domain files are registered in the shared `PlantSafety.Domain/GlobalUsing.cs` (matching the original Compliance template's global-using convention) rather than using per-file explicit usings; the edit was additive (new `#region`-style comment blocks appended, no existing lines touched) and verified via a scoped `dotnet build` immediately afterward to confirm zero Checklists-attributable compile errors.
