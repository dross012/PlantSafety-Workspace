// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           InfrastructureServiceRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Canonical DI registration entry point for all Infrastructure services.
//                  Additive-only evolution.
// ============================================================================
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Infrastructure.Common.Configuration;
using PlantSafety.Infrastructure.BackgroundJobs;
using PlantSafety.Infrastructure.Caching;
using PlantSafety.Infrastructure.Common.Clock;
using PlantSafety.Infrastructure.Common.Extensions;
using PlantSafety.Infrastructure.Common.Mapping;
using PlantSafety.Infrastructure.ExternalServices;
using PlantSafety.Infrastructure.FileStorage;
using PlantSafety.Infrastructure.Financials.Persistence;
using PlantSafety.Infrastructure.Financials.Services;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Security;
using PlantSafety.Infrastructure.Identity;
using PlantSafety.Infrastructure.Messaging.Publishers;
using PlantSafety.Infrastructure.OperationalSafety.Alerts.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Alerts.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.Checklists.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Checklists.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Repositories;
using PlantSafety.Infrastructure.OperationalSafety.Schedules.Interfaces;
using PlantSafety.Infrastructure.OperationalSafety.Schedules.Repositories;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
using PlantSafety.Infrastructure.Persistence.Dapper;
using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;
using PlantSafety.Infrastructure.PlanningProjects.Persistence;
using PlantSafety.Infrastructure.PlanningProjects.Services;
using PlantSafety.Infrastructure.Search;
using PlantSafety.Infrastructure.Training.CompetencyManagement.Interfaces;
using PlantSafety.Infrastructure.Training.CompetencyManagement.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;
using PlantSafety.Infrastructure.Features.SafetyEvents.RootCauseAnalysis.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.Exports;
using PlantSafety.Infrastructure.Features.SafetyEvents.Exports.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;
using PlantSafety.Infrastructure.Features.SafetyEvents.DrillsAndExercises.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.EmergencyManagement.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.CrisisManagement.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;
using PlantSafety.Infrastructure.Features.SafetyEvents.Observation.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;
using PlantSafety.Infrastructure.Features.SafetyEvents.NearMisses.Repositories;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;
using PlantSafety.Application.Features.Compliance.Handlers.Permits;
using PlantSafety.Infrastructure.Features.Compliance.Permits.Repositories;
using PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;
using PlantSafety.Infrastructure.Features.Compliance.ManagementOfChange.Repositories;
using PlantSafety.Application.Features.Analytics.Handlers.Dashboard;
using PlantSafety.Infrastructure.Features.Analytics.Dashboard.Repositories;
using PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;
using PlantSafety.Infrastructure.Features.PlanningProjects.Projects.Repositories;
using PlantSafety.Application.Features.Financials.Handlers.Budgets;
using PlantSafety.Infrastructure.Features.Financials.Budgets.Repositories;
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;
using PlantSafety.Infrastructure.Features.EnterpriseFabric.Users.Repositories;
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories;
using PlantSafety.Application.Features.Operational.Handlers.Training;
using PlantSafety.Infrastructure.Features.Operational.Training.Repositories;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Infrastructure.Operations.Backup.Options;
using PlantSafety.Infrastructure.Operations.Backup.Persistence;
using PlantSafety.Application.Operations.SchemaGuard.Repositories;
using PlantSafety.Infrastructure.Operations.SchemaGuard.Options;
using PlantSafety.Infrastructure.Operations.SchemaGuard.Persistence;
using PlantSafety.Infrastructure.Operations.SchemaGuard.FileSystem;
using PlantSafety.Application.Operations.Monitoring.Repositories;
using PlantSafety.Infrastructure.Operations.Monitoring.Options;
using PlantSafety.Infrastructure.Operations.Monitoring.Persistence;
using Shared.AuditLogging.Infrastructure.DependencyInjection;
using Shared.EventBus.Infrastructure.DependencyInjection;
using PlantSafety.Application.Operations.Archival.Repositories;
using PlantSafety.Infrastructure.Operations.Archival.Options;
using PlantSafety.Infrastructure.Operations.Archival.Persistence;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Infrastructure.Operations.Configuration.Options;
using PlantSafety.Infrastructure.Operations.Configuration.Persistence;
using PlantSafety.Application.Operations.SystemHealth.Repositories;
using PlantSafety.Infrastructure.Operations.SystemHealth.Options;
using PlantSafety.Infrastructure.Operations.SystemHealth.Persistence;
using PlantSafety.Infrastructure.EmployeeSafety.DependencyInjection;
using PlantSafety.Infrastructure.ContractorSafety.DependencyInjection;
using PlantSafety.Infrastructure.MachineSafety.DependencyInjection;
using PlantSafety.Infrastructure.SafetyProcedures.DependencyInjection;
using PlantSafety.Infrastructure.SafetyPolicies.DependencyInjection;
using PlantSafety.Infrastructure.AccidentManagement.DependencyInjection;
using PlantSafety.Infrastructure.RiskManagement.DependencyInjection;
using PlantSafety.Infrastructure.Scorecards.DependencyInjection;
using PlantSafety.Infrastructure.Scheduling.DependencyInjection;
using PlantSafety.Infrastructure.PPE.DependencyInjection;
using PlantSafety.Infrastructure.PermitToWork.DependencyInjection;
using PlantSafety.Infrastructure.Observability.DependencyInjection;
using PlantSafety.Infrastructure.Checklists.DependencyInjection;
using PlantSafety.Infrastructure.Auditing.DependencyInjection;
using PlantSafety.Infrastructure.SafetyCulture.DependencyInjection;
using PlantSafety.Infrastructure.Alerts.DependencyInjection;
using PlantSafety.Infrastructure.Dashboards.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.ScoreCards.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.Engagement.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Permits.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.ManagementOfChange.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Policies.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Procedures.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Auditing.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Certification.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Compliance.DependencyInjection;
using PlantSafety.Infrastructure.Operational.PredictiveMaintenance.DependencyInjection;
using PlantSafety.Infrastructure.Operational.PreventiveMaintenance.DependencyInjection;
using PlantSafety.Infrastructure.Features.DependencyInjection;
using PlantSafety.Infrastructure.Integrations.DependencyInjection;
using PlantSafety.Infrastructure.Operational.DependencyInjection;
using PlantSafety.Infrastructure.Persistence.Repositories.CrossDomain;
using PlantSafety.Infrastructure.SafetyEvents.DependencyInjection;
using PlantSafety.Infrastructure.ContinuousImprovement.DependencyInjection;
using PlantSafety.Infrastructure.Financials.DependencyInjection;
using PlantSafety.Infrastructure.HealthChecks.DependencyInjection;
using PlantSafety.Infrastructure.Intelligence.DependencyInjection;
using PlantSafety.Infrastructure.PlanningProjects.DependencyInjection;
using PlantSafety.Application.Compliance.Commands;
using PlantSafety.Infrastructure.Compliance.Commands.Persistence;

namespace PlantSafety.Infrastructure.DependencyInjection;

/// <summary>
/// Canonical DI entry point for PlantSafety.Infrastructure.
/// Call <c>services.AddInfrastructure(configuration)</c> from the host startup.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>Registers all Infrastructure services into the DI container.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options validation
        services.AddInfrastructureOptionsValidation();

        // Dapper type handlers (idempotent; safe to call multiple times)
        DapperTypeHandlers.RegisterAll();

        // Persistence
        // Singleton: SqlConnectionFactory is stateless (wraps only immutable DatabaseOptions and
        // opens a new SqlConnection per call), so it's safe to consume from singleton hosted
        // services (OutboxProcessor, ProcessedEventsCleaner) without the scoped-from-singleton
        // DI validation error.
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Tenant-isolation remediation rollout switch (docs\lifecycle\security\rls-audit.md).
        // Defaults to OFF (unbound section binds to the type's default, EnableTenantContextPropagation
        // = false) so introducing this option has zero effect until explicitly enabled per the staged
        // rollout plan: integration tests -> staging -> canary tenant -> global.
        services.Configure<TenantContextPropagationOptions>(
            configuration.GetSection(TenantContextPropagationOptions.SectionName));
        services.AddScoped<DapperCommandExecutor>();
        services.AddScoped<DapperQueryExecutor>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Shared.EventBus: cross-module event bus (separate from the intra-module Outbox ->
        // MassTransit/RabbitMq pipeline registered above, and separate from Shared.AuditLogging).
        // Binds Shared.EventBus.Infrastructure's SQL-queue-backed IEventPublisher/
        // IEventSubscriberFactory from the "EventBus" configuration section, then registers
        // PlantSafety's own relay that maps the curated set of real domain events (see
        // Messaging\CrossModule\CrossModuleDomainEventMapper.cs) onto that bus.
        services.AddSharedEventBus(configuration);
        services.AddScoped<Messaging.CrossModule.ICrossModuleEventRelay, Messaging.CrossModule.CrossModuleEventRelay>();

        // Application-layer transactional Unit of Work (consumed by TransactionBehavior<,> for
        // EVERY ICommand). Registered here with the FULLY QUALIFIED interface name -- deliberately
        // NOT relying on the ambiguous bare "IUnitOfWork" that this file's `using
        // PlantSafety.Infrastructure.Persistence.UnitOfWork;` resolves to -- because that name
        // collides with the differently-shaped PlantSafety.Infrastructure.Persistence.UnitOfWork.
        // IUnitOfWork registered immediately above. See TransactionalUnitOfWork.cs remarks for the
        // full history of this bug (previously: zero registration, so TransactionBehavior<,> could
        // not be constructed for any command in any module).
        services.AddScoped<PlantSafety.Application.Behaviors.Transactions.IUnitOfWork, TransactionalUnitOfWork>();

        // Caching
        services.AddStackExchangeRedisCache(opts =>
            opts.Configuration = configuration.GetConnectionString("Redis"));
        services.AddScoped<ICacheService, DistributedCacheService>();

        // Application-layer cache abstraction (consumed by CachingBehavior<,>, which wraps EVERY
        // MediatR request). Registered with the FULLY QUALIFIED interface name for the same reason
        // as the IUnitOfWork registration above: PlantSafety.Application.Abstractions.Caching.
        // ICacheService and PlantSafety.Infrastructure.Caching.ICacheService (registered
        // immediately above via the bare "ICacheService" this file's `using
        // PlantSafety.Infrastructure.Caching;` resolves to) are two differently-shaped interfaces
        // with the same simple name. Previously the Application interface had NO implementation
        // and NO registration at all, so CachingBehavior<,> could never be constructed for any
        // request. See ApplicationCacheServiceAdapter.cs remarks for the full history.
        services.AddScoped<PlantSafety.Application.Abstractions.Caching.ICacheService, ApplicationCacheServiceAdapter>();

        // Identity
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenValidationService, TokenValidationService>();
        services.AddScoped<ActorContextResolver>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // File storage
        // BlobServiceClient was previously unregistered (a real DI gap, closed here). Local dev
        // defaults to the well-known Azurite emulator connection string (publicly documented by
        // Microsoft, not a secret) via the "BlobStorage:ConnectionString" config key; production
        // deployments override this with a real Managed-Identity-backed connection string.
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            var connectionString = string.IsNullOrWhiteSpace(opts.ConnectionString)
                ? "UseDevelopmentStorage=true"
                : opts.ConnectionString;
            return new BlobServiceClient(connectionString);
        });
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        // Search
        services.AddScoped<ISearchService, AzureSearchService>();

        // Messaging
        // MassTransit.IBus was previously unregistered (a real DI gap, closed here). Local dev
        // targets a local RabbitMQ broker via the "RabbitMq" config section; production
        // deployments override with real Azure Service Bus credentials.
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                var rabbitUser = configuration["RabbitMq:Username"] ?? "guest";
                var rabbitPass = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });
            });
        });
        services.AddScoped<IIntegrationEventPublisher, ServiceBusEventPublisher>();

        // Background jobs
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<ProcessedEventsCleaner>();

        // OperationalSafety module repositories
        services.AddScoped<IAlertsInfrastructure, AlertsDapperRepository>();
        services.AddScoped<IChecklistsInfrastructure, ChecklistsDapperRepository>();
        services.AddScoped<IPPEManagementInfrastructure, PPEManagementDapperRepository>();
        services.AddScoped<IRiskManagementInfrastructure, RiskManagementDapperRepository>();
        services.AddScoped<ISchedulesInfrastructure, SchedulesDapperRepository>();

        // Training module repositories
        services.AddScoped<ICompetencyManagementInfrastructure, CompetencyManagementDapperRepository>();

        // Features tree — ProcessManagement module repositories
        services.AddScoped<PlantSafety.Application.Features.ProcessManagement.Handlers.Processes.IProcessRepository,
            PlantSafety.Infrastructure.Features.ProcessManagement.Processes.Repositories.ProcessDapperRepository>();

        // Features tree — WorkerWellBeing module repositories
        services.AddScoped<PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records.IWorkerWellBeingRecordRepository,
            PlantSafety.Infrastructure.Features.WorkerWellBeing.Records.Repositories.WorkerWellBeingDapperRepository>();

        // Features tree — EnvironmentalHealth module repositories
        services.AddScoped<PlantSafety.Application.Features.EnvironmentalHealth.Handlers.Readings.IEnvironmentalHealthReadingRepository,
            PlantSafety.Infrastructure.Features.EnvironmentalHealth.Readings.Repositories.EnvironmentalHealthDapperRepository>();

        // Financials
        services.AddScoped<FinancialsRepository>();
        services.AddScoped<FinancialsService>();

        // PlanningProjects
        services.AddScoped<PlanningProjectsRepository>();
        services.AddScoped<PlanningProjectsService>();

        // Features tree — SafetyEvents module repositories
        services.AddScoped<IRcaRepository, RcaDapperRepository>();
        services.AddScoped<IExportJobRepository, ExportJobDapperRepository>();
        services.AddScoped<IDrillRepository, DrillDapperRepository>();
        services.AddScoped<IEmergencyResponseRepository, EmergencyResponseDapperRepository>();
        services.AddScoped<ICrisisEventRepository, CrisisEventDapperRepository>();
        services.AddScoped<IObservationRepository, ObservationDapperRepository>();
        services.AddScoped<INearMissRepository, NearMissDapperRepository>();
        services.AddScoped<IIncidentReadRepository, IncidentReadDapperRepository>();
        services.AddScoped<IIncidentRepository, IncidentDapperRepository>();

        // Features tree — Compliance module repositories
        services.AddScoped<IPermitRepository, PermitDapperRepository>();
        services.AddScoped<IMocRepository, MocDapperRepository>();

        // Features tree — Analytics module repositories
        services.AddScoped<IDashboardRepository, DashboardDapperRepository>();

        // Features tree — PlanningProjects module repositories
        services.AddScoped<IProjectRepository, ProjectDapperRepository>();

        // Features tree — Financials module repositories
        services.AddScoped<IBudgetRepository, BudgetDapperRepository>();

        // Features tree — EnterpriseFabric module repositories
        services.AddScoped<IUserRepository, UserDapperRepository>();

        // Features tree — ContinuousImprovement module repositories
        services.AddScoped<ICorrectiveActionRepository, CorrectiveActionDapperRepository>();

        // Features tree — Operational module repositories
        services.AddScoped<ITrainingRepository, TrainingDapperRepository>();

        // Operations — Backup/DR (Phase 12 + Phase 18 SQL Server backup automation, surfaced
        // for the first time in C#/UI here; NOT the unrelated CrossDomain.Backup CRUD stub).
        services.Configure<BackupOptions>(configuration.GetSection(BackupOptions.SectionName));
        services.AddScoped<IBackupRepository, BackupDapperRepository>();

        // Operations — Schema Drift Guard (Phase 7 Governance [System].[SchemaMigrations]
        // tracking, compared against a filesystem manifest scan of Migrations\**\*.sql).
        // Manifest provider is a Singleton: it scans the filesystem once and caches the
        // result in-process for the lifetime of the app instance.
        services.Configure<SchemaGuardOptions>(configuration.GetSection(SchemaGuardOptions.SectionName));
        services.AddScoped<ISchemaVersionRepository, SchemaVersionDapperRepository>();
        services.AddSingleton<IMigrationManifestProvider, MigrationManifestProvider>();

        // Operations — Archival (Phase 7 Governance retention/purge automation:
        // sp_AuditLog_Purge/sp_ErrorLog_Purge/sp_IntegrationLogEntry_Purge/
        // sp_ReplayRecord_Purge/sp_DiagnosticEvent_Purge, 74-retention-archival.sql, plus
        // Phase 18 Invoke-PlantSafetyPurge.ps1 Task Scheduler automation), surfaced for the
        // first time in C#/UI here. Sibling module to Operations.Backup/SchemaGuard.
        services.Configure<ArchivalOptions>(configuration.GetSection(ArchivalOptions.SectionName));
        services.AddScoped<IArchivalRepository, ArchivalDapperRepository>();

        // Operations — Audit Viewer (read-only search/export/diff view over the shared,
        // cross-database [PlantSafety.AuditDB].dbo.AuditLog table written to by hundreds of
        // stored procedures across all 8 module databases via the dbo.AuditLog synonym --
        // Phase8-Structural\83-synonyms-audit-error.sql). Sibling module to
        // Operations.Backup/SchemaGuard/Archival. Never issues INSERT/UPDATE/DELETE.
        //
        // As of 2026-07-26, IAuditLogRepository/AuditLogDapperRepository/options moved out
        // of PlantSafety into the shared Shared.AuditLogging module (dbo.AuditLog is
        // server-level platform data, not PlantSafety-tenant data) so Reporting's Audit
        // Report PDF can consume the exact same repository -- see
        // Shared\AuditLogging\docs\README.md. Configuration section renamed
        // "Operations:AuditViewer" -> "Shared:AuditLogging".
        services.AddSharedAuditLogging(configuration);

        // Operations — Monitoring (DB Performance via per-database Query Store, DB Errors via
        // the shared [PlantSafety.ErrorDB].dbo.ErrorLog table, DB Deadlocks via the built-in
        // "system_health" Extended Events session). DB Health and DB Size are deliberately NOT
        // duplicated here -- see IDatabaseHealthReportProvider (wraps the existing
        // HealthCheckService) and GetMonitoringDatabaseSizeQueryHandler (delegates to the
        // existing IBackupRepository) respectively. Sibling module to Operations.Backup/
        // SchemaGuard/Archival/AuditViewer.
        services.Configure<MonitoringOptions>(configuration.GetSection(MonitoringOptions.SectionName));
        services.AddScoped<IMonitoringRepository, MonitoringDapperRepository>();
        services.AddScoped<IDatabaseHealthReportProvider, DatabaseHealthReportProvider>();

        // Operations — Configuration (Phase 14 Governance [System].[FeatureFlag] editor,
        // 144-feature-flags.sql), confirmed present ONLY in PlantSafety.ArchiveDB and
        // PlantSafety.IntegrationDB (verified by reading the full migration script). Each
        // database's flags are independent — no cross-database sync. "Thresholds" /
        // "Routing Rules" / "Tenant Overrides" have no backing SQL Server tables in this
        // codebase as of this build and are deliberately out of scope — see
        // IFeatureFlagRepository. Sibling module to Operations.Backup/SchemaGuard/
        // Archival/AuditViewer/Monitoring.
        services.Configure<ConfigurationOptions>(configuration.GetSection(ConfigurationOptions.SectionName));
        services.AddScoped<IFeatureFlagRepository, FeatureFlagDapperRepository>();

        // Operations — System Health (Step 10/10, final roadmap deliverable: unified rollup
        // dashboard over Backup/SchemaGuard/Monitoring/Archival/AuditViewer/Configuration via
        // MediatR ISender fan-out, plus the two genuinely new pieces here -- SQL Server
        // version/edition of the primary tenant connection (SERVERPROPERTY) and live Windows
        // Task Scheduler status for the Phase 18 automation jobs (schtasks.exe). App version is
        // a pure Assembly read with no repository. Sibling module to Operations.Backup/
        // SchemaGuard/Archival/AuditViewer/Monitoring/Configuration.
        services.Configure<SystemHealthOptions>(configuration.GetSection(SystemHealthOptions.SectionName));
        services.AddScoped<ISystemHealthRepository, SystemHealthDapperRepository>();

        // Legacy-taxonomy modules (17 modules added per the PlantSafety Program Structure
        // master prompt) — Infrastructure layer built with Dapper + stored procedures, no EF.
        services
            .AddEmployeeSafetyInfrastructure()
            .AddContractorSafetyInfrastructure()
            .AddMachineSafetyInfrastructure()
            .AddSafetyProceduresInfrastructure()
            .AddSafetyPoliciesInfrastructure()
            .AddAccidentManagementInfrastructure()
            .AddRiskManagementInfrastructure()
            .AddScorecardsInfrastructure()
            .AddSchedulingInfrastructure()
            .AddPPEInfrastructure()
            .AddPermitToWorkInfrastructure()
            .AddPermitsInfrastructure()
            .AddManagementOfChangeAggregateInfrastructure()
            .AddPolicyAggregateInfrastructure()
            .AddProceduresAggregateInfrastructure()
            .AddRegulatoryCalendarInfrastructure()
            .AddComplianceAuditingInfrastructure()
            .AddComplianceCertificationInfrastructure()
            .AddComplianceRuleInfrastructure()
            .AddScoreCardsAggregateInfrastructure()
            .AddEngagementAggregateInfrastructure()
            .AddPredictiveAnalyticsAggregateInfrastructure()
            .AddPredictiveMaintenanceInfrastructure()
            .AddPreventiveMaintenanceTaskInfrastructure()
            .AddObservabilityInfrastructure()
            .AddChecklistsInfrastructure()
            .AddAuditingInfrastructure()
            .AddSafetyCultureInfrastructure()
            .AddAlertsInfrastructure()
            .AddDashboardsInfrastructure();

        // Legacy-module repositories (pre-existing Compliance/Analytics/Operational/etc.
        // modules whose Application-layer repository interfaces had no Infrastructure
        // implementation until this pass — closes the ValidateOnBuild gap documented in
        // PlantSafety.Api/Program.cs).
        services
            .AddComplianceLegacyRepositories()
            .AddComplianceLegacyRepositoriesBatch2()
            .AddOperationalLegacyRepositories()
            .AddAnalyticsLegacyRepositories()
            .AddSafetyEventsLegacyRepositories()
            .AddSafetyEventsWriteRepositories()
            .AddSafetyEventsNearMissesRootCauseAnalysisInfrastructure()
            .AddCrisisManagementDrillsAndExercisesInfrastructure()
            .AddIntegrationsInfrastructure()
            .AddCrossDomainRepositories()
            .AddFeaturesLegacyRepositories()
            .AddIntelligenceRepositories()
            .AddContinuousImprovementRepositories()
            .AddContinuousImprovementRCAAggregateInfrastructure()
            .AddPlanningProjectsRepositories()
            .AddHealthChecksRepositories()
            .AddFinancialsRepositories();

        services.AddScoped<IComplianceBulkRepository, ComplianceBulkDapperRepository>();

        return services;
    }
}
