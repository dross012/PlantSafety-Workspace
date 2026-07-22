# SafetyPolicies Module

## Purpose
Centralizes the facility's 17 governing safety-policy categories (Hazard Communication, Equipment/Machine Operation, PPE, Emergency Response, Housekeeping/Ergonomics, Safety Training, Contractor, Environmental/Chemical, Electrical, Confined Space, Working at Heights, Hot Work, Material Handling, Machine Maintenance, Security/Access, Incident Investigation/CAPA, and Safety Culture) into a unified, searchable, auditable system supporting OSHA compliance, internal governance, insurance requirements, and industry best practices.

Source: PlantSafety_Program_Structure_extract.txt (legacy 19-module "PlantSafety Program Structure" document), scaffolded as a standalone top-level module alongside (not merged into) the existing PlantSafety.Domain / PlantSafety.Application module set.

## Responsibilities
- Owns the SafetyPolicy aggregate root, its lifecycle (SafetyPolicyStatus: Created -> Active -> Completed -> Closed -> Archived), and the PolicyCategory Smart Enum taxonomy.
- Provides Commands (CreateSafetyPolicy), Queries (read-model projections per category below), Validators, DTOs, and repository interfaces (Domain-layer, Dapper/stored-procedure-backed in Infrastructure -- not generated in this pass per the C#-only scope).
- Does NOT own SQL scripts, EF Core migrations, DbContext, or repository *implementations* -- those belong to a future Infrastructure-layer pass.

## Dependencies
Procedures, PPE, Machines, Contractors, RiskManagement (referenced conceptually only -- no direct project references; integration is event-based per Handbook Part IX/XII).

No direct project references to other modules' Domain/Application internals (Handbook Part IX "No Bleed" rule). All cross-module dependencies are conceptual/event-based only.

## PolicyCategory taxonomy
- `HazardCommunicationAssessment` -- Hazard Communication & Assessment Policies
- `EquipmentMachineOperation` -- Equipment & Machine Operation Policies
- `PersonalProtectiveEquipment` -- Personal Protective Equipment (PPE) Policies
- `EmergencyResponseEvacuation` -- Emergency Response & Evacuation Policies
- `HousekeepingErgonomics` -- Housekeeping & Ergonomics Policies
- `SafetyTrainingReporting` -- Safety Training & Reporting Policies
- `ContractorSafety` -- Contractor Safety Policies
- `EnvironmentalChemicalSafety` -- Environmental & Chemical Safety Policies
- `ElectricalSafety` -- Electrical Safety Policies
- `ConfinedSpaceEntry` -- Confined Space Entry Policies
- `WorkingAtHeights` -- Working at Heights Policies
- `HotWork` -- Hot Work Policies
- `MaterialHandlingStorage` -- Material Handling & Storage Policies
- `MachineMaintenanceInspection` -- Machine Maintenance & Inspection Policies
- `SecurityAccessControl` -- Security & Access Control Policies
- `IncidentInvestigationCorrectiveAction` -- Incident Investigation & Corrective Action Policies
- `SafetyCultureBehavioral` -- Safety Culture & Behavioral Policies

## Application query areas (read-model projections)
- **HazardCommunicationAssessment** -- Hazard Communication & Assessment Policies
- **EquipmentMachineOperation** -- Equipment & Machine Operation Policies
- **PersonalProtectiveEquipment** -- Personal Protective Equipment (PPE) Policies
- **EmergencyResponseEvacuation** -- Emergency Response & Evacuation Policies
- **HousekeepingErgonomics** -- Housekeeping & Ergonomics Policies
- **SafetyTrainingReporting** -- Safety Training & Reporting Policies
- **ContractorSafety** -- Contractor Safety Policies
- **EnvironmentalChemicalSafety** -- Environmental & Chemical Safety Policies
- **ElectricalSafety** -- Electrical Safety Policies
- **ConfinedSpaceEntry** -- Confined Space Entry Policies
- **WorkingAtHeights** -- Working at Heights Policies
- **HotWork** -- Hot Work Policies
- **MaterialHandlingStorage** -- Material Handling & Storage Policies
- **MachineMaintenanceInspection** -- Machine Maintenance & Inspection Policies
- **SecurityAccessControl** -- Security & Access Control Policies
- **IncidentInvestigationCorrectiveAction** -- Incident Investigation & Corrective Action Policies
- **SafetyCultureBehavioral** -- Safety Culture & Behavioral Policies

## Inputs
- Commands: CreateSafetyPolicyCommand (TenantId, SiteId, SafetyPolicyTitle, Category, OwnerId, EffectiveAtUtc, ReviewRequired) with full command envelope (CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
- Queries: Get{Category}Query(TenantId, SiteId, From, To) for each read-model area listed above.

## Outputs
- CreateSafetyPolicyResult (SafetyPolicyId, Status) on command success.
- {Category}QueryResult (TenantId, SiteId, GeneratedAt, Items) on query success.
- All handlers return the canonical Result<T> discriminated union; no exceptions cross the Application boundary except truly exceptional conditions (Handbook Part XIV.4).

## Events emitted
- SafetyPolicyCreatedEvent (EventId, SafetyPolicyId, CreatedAtUtc, Actor, CorrelationId, CausationId) -- raised by SafetyPolicy.Create(...). Additional lifecycle events (Activated/Completed/Closed/Archived) are additive extension points for a future pass; only the Created event is materialized in this scaffold.

## Events consumed
- None in this C#-only Domain+Application scaffold. Cross-module event subscriptions (e.g. SafetyPolicies reacting to other modules' integration events) belong to the Infrastructure/messaging layer and are out of scope for this pass.

## Integration points
- Repository interfaces (ISafetyPolicyRepository, ISafetyPolicyFactory, and one I{Category}Repository per query area) are Domain/Application-layer *contracts only*. Concrete Dapper/stored-procedure implementations, EF Core, migrations, and API controllers are intentionally NOT generated in this pass (C#-only Domain+Application scope; no SQL).
- DI registration: SafetyPoliciesApplicationServiceRegistration.AddSafetyPoliciesApplicationServices(IServiceCollection) is a self-contained extension method. It is NOT wired into the shared PlantSafety.Application.DependencyInjection.ApplicationServiceRegistration composition root in this pass, to avoid merge collisions with other concurrently-running module-scaffolding agents editing that same shared file. Wiring it in (one additive line under the existing .AddXxxApplicationServices() chain) is a follow-up integration step for whoever finalizes composition-root wiring.

## Structural notes / deviations from the Compliance template
- One Domain aggregate per module (rather than Compliance's several aggregates per module) -- each of these four modules maps to a single cohesive bounded concept per the source document, whereas Compliance spans eight historically-distinct sub-domains (Auditing, Certification, Compliance, ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar).
- Category counts are scaled to the module's own source-document content: SafetyPolicies (17, one per documented policy category), RiskManagement (11, one per documented risk-management component), Scorecards (6, one per documented indicator group/structure/reporting section), AccidentManagement (4, kept intentionally small because the source section is comparatively thin).
- Domain files use explicit per-file using directives rather than being added to the shared PlantSafety.Domain/GlobalUsing.cs, again to avoid concurrent-agent merge collisions on that shared file. This is a pure additive-safety choice; functionally equivalent to a global-using entry.