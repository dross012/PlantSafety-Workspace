# Pure-Stub Modules Playbook — Batch 1 (11 modules)

Phase 4 domain-modernization pass, twelfth module batch. Status as of 2026-08-02.

## Scope: the barest starting state seen in this entire effort

Following a survey of every top-level `PlantSafety.Domain\` folder not yet touched by this effort, 11 modules were identified as pure Create-only stubs — the barest state found in this whole modernization pass, barer even than the SafetyEvents/Analytics "Create factory + no Application layer" modules: `AiGovernance`, `AiSafetyIntelligence`, `ContractorVisitor`, `DigitalTwin`, `EnvironmentalHealth`, `Governance`, `HumanFactors`, `OperationalSafety`, `ProcessManagement` (aggregate `ProcessDefinition`), `Sustainability`, `WorkerWellBeing`.

10 of the 11 had literally nothing but `<Module>Id`/`CreatedAtUtc`/`CreatedBy` on the aggregate, a single `<Module>CreatedEvent`, a marker event interface, an `I<Module>Repository` interface (living in a `Repositories\` folder rather than the `Interfaces\` folder convention used everywhere else in the codebase — left as-is per module, not relocated), and a strong-typed Id. No `TenantId`/`SiteId`, no Enums, no Rules, no Exceptions, no Application layer, no Infrastructure write path, no Api. `ProcessManagement`'s aggregate (`ProcessDefinition`) was somewhat more developed — it already had `TenantId`/`SiteId`/`ProcessName`/`Category`/a business-string `Status`/`Version`/`OwnerId` and a validated `Create` factory, just no lifecycle methods beyond Create.

## Pre-flight audit: live "Features" verticals hiding under the same module names

Before designing anything, a dedicated audit (foreground Explore agent) found that 6 of the 11 module names are shared with a completely different, already-LIVE "Features" vertical wired into the central `InfrastructureServiceRegistration.cs` — a coincidence of naming, not the same concept:

- **AiGovernance**: `Application.Features.AiGovernance.Handlers.IAiGovernanceRepository` → `AiGovernanceDapperRepository`, backing a real "GovernedModel" registry (`[AiGovernance].[Model]` table, `usp_Model_*` procs), live via `.AddFeaturesLegacyRepositories()`.
- **DigitalTwin**: same pattern, a "Twin" registry (`usp_Twin_*`).
- **Sustainability**: same pattern, a "Metric" tracker (`usp_Metric_*`).
- **ProcessManagement**: `IProcessRepository` → `ProcessDapperRepository` (a generic "Process" tracking vertical), registered directly (not via the legacy helper).
- **WorkerWellBeing**: `IWorkerWellBeingRecordRepository` → `WorkerWellBeingDapperRepository`, registered directly.
- **EnvironmentalHealth**: `IEnvironmentalHealthReadingRepository` → `EnvironmentalHealthDapperRepository` (an "EnvironmentalHealthReading" vertical, distinct from both the DDD aggregate built this pass AND the separate, already-modernized `EmployeeSafety.EnvironmentalHealthMonitoring` aggregate — three distinct "environmental health" concepts now coexist in this codebase).

`OperationalSafety`'s Infrastructure folder additionally contains real, live, unrelated registrations for `OperationalSafety.Alerts`/`Checklists`/`PPEManagement`/`RiskManagement`/`Schedules` — a pre-existing feature-flag-style module structure, not the bare aggregate stub.

The audit also confirmed `PlantSafety.Infrastructure\Persistence\Configurations\*` (16 files across many modules, EF-Core-`*PersistenceConfiguration`-shaped) is entirely dead scaffolding with no `DbContext` anywhere in this Dapper-only codebase — ignored across the board, not built upon, not cleaned up (out of scope for this pass).

Every dispatched agent was given an explicit per-module do-not-touch list for these live verticals, confirmed by the final DI-registration verification below to have been respected with zero collisions.

## Domain models built this pass

10 of 11 modules got `TenantId`/`SiteId` added (previously missing) plus business fields grounded in the module's evident purpose (no pre-existing DTO to ground vocabulary in, unlike every prior from-scratch module in this effort — a first for this pass), and the same generic `Created→Active→Completed→Closed→Archived` lifecycle shape used everywhere else in this codebase, via standard `Activate`/`Complete`/`Close`/`Archive` methods:

- **AiGovernance** — AI system governance/oversight registry (`SystemName`/`AiUseCase`/`RiskClassification`/`OwnerUserId`/`RequiresHumanOversight`).
- **AiSafetyIntelligence** — AI-generated safety insight/prediction record (`InsightType`/`Description`/`ConfidenceScore`/`SourceModelName`).
- **ContractorVisitor** — contractor/visitor site-access record (`FullName`/`CompanyName`/`VisitorType`/`PurposeOfVisit`/`HostUserId`/`ScheduledArrivalUtc`), plus a same-status **CheckIn** action (`ActualArrivalUtc`).
- **DigitalTwin** — digital-twin model registry (`TwinName`/`AssetReferenceId`/`ModelType`/`LastSyncedAtUtc`), plus a same-status **RefreshSync** action, mirroring `Analytics.Dashboard`'s `RefreshData`.
- **EnvironmentalHealth** — environmental-health monitoring record (`MonitoringType`/`MeasuredValue`/`Unit`/`ThresholdExceeded`).
- **Governance** — general safety-governance/policy-oversight record (`GovernanceArea`/`PolicyReferenceId`/`ReviewCycleMonths`/`OwnerUserId`).
- **HumanFactors** — human-factors/behavioral-risk assessment (`FactorType`/`AffectedUserId`/`SeverityRating`/`Description`).
- **OperationalSafety** — general operational-safety oversight record (`SafetyDomain`/`RiskLevel`/`OwnerUserId`/`ReviewDueAtUtc`).
- **Sustainability** — sustainability/ESG initiative tracking (`InitiativeName`/`Category`/`TargetValue`/`OwnerUserId`).
- **WorkerWellBeing** — worker wellbeing/mental-health check-in record (`WorkerId`, exposed as `WorkerUserId` alias/`CheckInType`/`WellBeingScore`/`FollowUpRequired`).

**ProcessDefinition** (ProcessManagement) got its lifecycle grounded in its own pre-existing doc comment ("Draft, Active, UnderReview, Retired") rather than the generic shape: `Draft → SubmitForReview → UnderReview → Approve → Active → Retire → Retired → Archive → Archived`, plus a same-status **Revise** action (increments `Version`), mirroring `Compliance.Policy`'s `Supersede`.

## Execution: three parallel agents, 11 modules, zero net regressions

1. **AiGovernance + AiSafetyIntelligence + ContractorVisitor + DigitalTwin** — full build from bare scaffolds. Hit one transient self-introduced build error (missing `PlantSafety.Application.Common.Errors` using in the 4 write repositories), self-corrected before final verification.
2. **EnvironmentalHealth + Governance + HumanFactors + OperationalSafety** — full build from bare scaffolds. `EnvironmentalHealth` was a partial exception: it already had some business fields including a business-string `Status` (Normal/Warning/Critical) — renamed `MetricType`→`MonitoringType`, `MeasurementValue`(decimal)→`MeasuredValue`(decimal?), `Status`→`EvaluatedStatus` to free the `Status` name for the new lifecycle enum, after confirming zero external consumers of the old names. Also checked API route names against the live `OperationalSafety.Alerts/Checklists/...` and `Features.EnvironmentalHealth` controllers to avoid route collisions.
3. **ProcessManagement + Sustainability + WorkerWellBeing** — `ProcessDefinition` converted its `string Status` to the documented enum and got the `Revise` same-status action; also converted `IProcessDefinitionDomainEvent` to extend the platform-canonical `IDomainEvent` (it had been a standalone duplicate contract, not flowing through the shared outbox pipeline — now fixed). `Sustainability`/`WorkerWellBeing` built from bare scaffolds. Found that a `using`-alias did not resolve the aggregate-name/namespace-segment collision here (`CS0118`, the namespace segment shadows the alias) — worked around with fully-qualified type references throughout, a more thorough fix than the alias approach used elsewhere in this effort. `WorkerWellBeing.WorkerUserId` was implemented as a computed alias over the pre-existing `WorkerId` field rather than a redundant duplicate column.

All three agents' `InfrastructureServiceRegistration.cs` and `GlobalUsing.cs` edits integrated cleanly with clear same-day comment blocks distinguishing new registrations from the pre-existing live verticals — verified below.

Same recurring naming-collision workaround as every prior module: 9 of 11 aggregate class names equal their own module/namespace segment (all except `ProcessDefinition` and, trivially, none avoid it by design) — resolved via `<Module>AggregateDapperRepository` naming and fully-qualified references.

No orphaned dead-scaffold files were found for this batch (unlike every prior module) — the various `*Stub.cs` placeholder markers discovered in these modules' Infrastructure folders (e.g. `AiInvestigationsStub`, `DataLakeStub`, `EquipmentSafetyStub`, `WellnessStub`) belong to entirely different, still-unbuilt future modules, not leftovers of this batch's own work — correctly left untouched.

## Verified independently (not just trusting agent self-reports)

- All 11 write-side repositories: `outboxWriter.Enqueue` present exactly twice each (Add + Update).
- All 11: `ExecuteScalarAsync` + `ConcurrencyConflictException` present (OperationalSafety's folder shows more `ExecuteScalarAsync` hits because it also contains the pre-existing, unrelated Alerts/Checklists/PPEManagement/RiskManagement/Schedules repositories — confirmed those are the live, correctly-untouched vertical, not new code).
- 58 files use `ErrorSeverity.NotFound`; 47 have the `ConcurrencyConflictException` passthrough across the 11 modules' Application layers.
- All 11 DI registration calls (`AddAiGovernanceModuleInfrastructure`, `AddAiSafetyIntelligenceModuleInfrastructure`, `AddContractorVisitorModuleInfrastructure`, `AddDigitalTwinModuleInfrastructure`, `AddEnvironmentalHealthAggregateInfrastructure`, `AddGovernanceAggregateInfrastructure`, `AddHumanFactorsAggregateInfrastructure`, `AddOperationalSafetyAggregateInfrastructure`, `AddProcessManagementAggregateInfrastructure`, `AddSustainabilityAggregateInfrastructure`, `AddWorkerWellBeingAggregateInfrastructure`) confirmed present in the central `InfrastructureServiceRegistration.cs`, coexisting with all pre-existing registrations (including the 6 live Features verticals sharing module names) with no lost or overwritten entries.
- `EnvironmentalHealth`'s field rename confirmed isolated to the new aggregate's own table (`EnvironmentalHealthAggregate.sql`) — the pre-existing, unrelated `EnvironmentalHealthReadings.sql`/`usp_EnvironmentalHealth_{GetById,List}.sql` (Features vertical) still correctly use the original `MetricType`/`MeasurementValue` names, untouched.
- Full solution build (`Api`, `Tests`, `IntegrationTests`) clean.
- 934/934 unit tests passing, verified with a fresh independent run (baseline was 864 before this batch; +70 new tests across 11 modules — 2 handler test files × 11 modules, plus extra CheckIn/RefreshSync/Revise coverage).

## Known remaining gaps

- No fresh ETag returned on lifecycle-command results across any of the 11 modules (same open item as every prior module).
- No `PlantSafety.DataIntegrationTests`/live-SQL-Server coverage for any of the newly built write paths.
- These 11 modules had no pre-existing DTO/read-model to ground field/status vocabulary in (a first for this effort) — field lists were designed from the module's evident purpose rather than extracted from existing code; worth a domain-expert review pass if these fields turn out not to match real business requirements.
- The 6 modules with live "Features" verticals sharing their name (AiGovernance, DigitalTwin, Sustainability, ProcessManagement, WorkerWellBeing, EnvironmentalHealth) now have two parallel, unrelated persistence stacks under similar namespaces — not reconciled or renamed this pass; a future architectural decision may want to rename one side for clarity, though no functional conflict exists today.
- The remaining stub-module survey identified further candidates not covered by this batch: `Intelligence`'s 4 stub classes (`AIInvestigationsStub`, `AITrainingStub`, `DataLakeStub`, `PredictiveSafetyStub`), and the anemic read-only modules `Financials`/`PlanningProjects`/`EnterpriseFabric`/`CrossDomain`.
