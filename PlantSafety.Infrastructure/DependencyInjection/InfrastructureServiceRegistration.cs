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
using PlantSafety.Infrastructure.Common.Concurrency;
using PlantSafety.Infrastructure.Common.Extensions;
using PlantSafety.Infrastructure.Common.Mapping;
using PlantSafety.SharedKernel.Abstractions.Concurrency;
using PlantSafety.Infrastructure.ExternalServices;
using PlantSafety.Infrastructure.FileStorage;
using PlantSafety.Infrastructure.Financials.Persistence;
using PlantSafety.Infrastructure.Financials.Services;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Security;
using PlantSafety.Application.Abstractions.Identity;
using PlantSafety.Application.Abstractions.Authentication;
using PlantSafety.Infrastructure.Identity;
using PlantSafety.Infrastructure.PublicSafetyCommunication.DependencyInjection;
using PlantSafety.Infrastructure.CyberPhysicalSecurity.DependencyInjection;
using PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.DependencyInjection;
using PlantSafety.Infrastructure.Messaging.Publishers;
using PlantSafety.Infrastructure.Messaging.Consumers.OperationalSafety;
using PlantSafety.Infrastructure.Messaging.Consumers.Compliance;
using PlantSafety.Infrastructure.Messaging.Consumers.Analytics;
using PlantSafety.Infrastructure.Messaging.Consumers.SafetyEvents;
using PlantSafety.Infrastructure.Messaging.Consumers.Training;
using PlantSafety.Infrastructure.Messaging.Consumers.Sustainability;
using PlantSafety.Infrastructure.Messaging.Consumers.WorkerWellBeing;
using PlantSafety.Infrastructure.Messaging.Consumers.EnvironmentalHealth;
using PlantSafety.Infrastructure.Messaging.Consumers.EnterpriseFabric;
using PlantSafety.Infrastructure.Messaging.Consumers.Governance;
using PlantSafety.Infrastructure.Messaging.Consumers.HumanFactors;
using PlantSafety.Infrastructure.Messaging.Consumers.Integrations;
using PlantSafety.Infrastructure.Messaging.Consumers.DigitalTwin;
using PlantSafety.Infrastructure.Messaging.Consumers.ContractorVisitor;
using PlantSafety.Infrastructure.Messaging.Consumers.AiGovernance;
using PlantSafety.Infrastructure.Messaging.Consumers.AiSafetyIntelligence;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;
using PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter;
using PlantSafety.Infrastructure.Messaging.DeadLetter;
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.Application.Messaging.Repositories;
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
using PlantSafety.Infrastructure.Common.Secrets;
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
using PlantSafety.Infrastructure.PhysicalAccessControl.DependencyInjection;
using PlantSafety.Infrastructure.AutonomousVehicleSafety.DependencyInjection;
using PlantSafety.Infrastructure.LogisticsTransportSafety.DependencyInjection;
using PlantSafety.Infrastructure.CommunitySafety.DependencyInjection;
using PlantSafety.Infrastructure.HazardousWasteManagement.DependencyInjection;
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
using PlantSafety.Infrastructure.Auditing.AuditTimeline.DependencyInjection;
using PlantSafety.Infrastructure.Auditing.AuditReport.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.RulesEngine.DependencyInjection;
using PlantSafety.Infrastructure.Tenancy.Tenant.DependencyInjection;
using PlantSafety.Infrastructure.Scheduling.PredictiveScheduling.DependencyInjection;
using PlantSafety.Infrastructure.SafetyCulture.DependencyInjection;
using PlantSafety.Infrastructure.SafetyPrograms.DependencyInjection;
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
using PlantSafety.Infrastructure.ProcessManagement.DependencyInjection;
using PlantSafety.Infrastructure.ProcessManagement.Routing.DependencyInjection;
using PlantSafety.Infrastructure.Sustainability.DependencyInjection;
using PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.DependencyInjection;
using PlantSafety.Infrastructure.WorkerWellBeing.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Certification.DependencyInjection;
using PlantSafety.Infrastructure.Compliance.Compliance.DependencyInjection;
using PlantSafety.Infrastructure.Operational.PredictiveMaintenance.DependencyInjection;
using PlantSafety.Infrastructure.Operational.PreventiveMaintenance.DependencyInjection;
using PlantSafety.Infrastructure.Operational.WorkOrders.DependencyInjection;
using PlantSafety.Infrastructure.Operational.ProductionTracking.DependencyInjection;
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
using PlantSafety.Infrastructure.AiGovernance.DependencyInjection;
using PlantSafety.Infrastructure.AiSafetyIntelligence.DependencyInjection;
using PlantSafety.Infrastructure.ContractorVisitor.DependencyInjection;
using PlantSafety.Infrastructure.DigitalTwin.DependencyInjection;
using PlantSafety.Infrastructure.EnvironmentalHealth.DependencyInjection;
using PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DependencyInjection;
using PlantSafety.Infrastructure.Governance.DependencyInjection;
using PlantSafety.Infrastructure.HumanFactors.DependencyInjection;
using PlantSafety.Infrastructure.HumanFactors.CognitiveLoadAnalytics.DependencyInjection;
using PlantSafety.Infrastructure.OperationalSafety.DependencyInjection;
using PlantSafety.Infrastructure.EnterpriseFabric.DependencyInjection;
using PlantSafety.Infrastructure.Workflow.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.UnifiedModel.DependencyInjection;
using PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.DependencyInjection;
using PlantSafety.Infrastructure.Observability.Tracing;
using PlantSafety.Infrastructure.Billing.DependencyInjection;
using PlantSafety.Infrastructure.GlobalSafetyInsights.DependencyInjection;
using PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Consumers;

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

        // Canonical structured logging (Handbook §2.19-2.22): IApplicationLogger previously had
        // zero implementation and zero consumers anywhere in the solution (a dead abstraction).
        // Registered here so LoggingBehavior and other envelope-emitting call sites can resolve it.
        services.AddScoped<PlantSafety.Application.Abstractions.Logging.IApplicationLogger,
            PlantSafety.Infrastructure.Common.Logging.ApplicationLogger>();

        // Distributed locking (Phase 6 pre-requisite, 2026-08-02): SQL Server sp_getapplock-backed
        // IDistributedLockProvider protecting concurrent/racing state mutation of the Workflow
        // module's WorkflowInstance aggregate (see WorkflowInstanceLockBehavior) and available for
        // the 8 Phase 5 cross-module orchestration consumers to adopt next. Singleton for the same
        // reason as SqlConnectionFactory immediately above: it is stateless (wraps only the
        // singleton ISqlConnectionFactory) and opens a brand-new, dedicated SqlConnection per
        // AcquireAsync/TryAcquireAsync call, so it is safe to consume from singleton hosted
        // services as well as scoped request pipelines.
        services.AddSingleton<IDistributedLockProvider, SqlServerDistributedLockProvider>();

        // Cross-module orchestration consumer replay-atomicity fix (Phase 6 blocker, 2026-08-02):
        // business-key dedup ledger for the 8 CrossModuleOrchestration consumers (see
        // CrossModuleIdempotentConsumerBase.TryExecuteOnceAsync). Singleton for the same reason as
        // ISqlConnectionFactory/IDistributedLockProvider above -- stateless, opens its own
        // connection per call.
        services.AddSingleton<
            PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.ICrossModuleDedupStore,
            PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.SqlCrossModuleDedupStore>();

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

        // Tenant federation (Phase 6 §2.2) — external IdP tenant/org claim -> internal
        // TenantId mapping. Configuration-driven, gracefully no-ops (see
        // ConfigurationExternalIdentityProviderMapping.IsFederationConfigured) when the
        // "ExternalIdentityProviders" section is empty, which it is by default in this
        // environment (no real external IdP/SSO tenant is configured here).
        services.Configure<ExternalIdentityProviderOptions>(
            configuration.GetSection(ExternalIdentityProviderOptions.SectionName));
        services.AddScoped<IExternalIdentityProviderMapping, ConfigurationExternalIdentityProviderMapping>();

        // SAML2 SSO (Phase 6 §2.1) — deliberate "bring your own SAML library" extension
        // seam. No SAML2 package is referenced anywhere in this solution; see
        // UnconfiguredSamlAuthenticationProvider's remarks for how to wire in a real
        // implementation without touching any other layer.
        services.AddScoped<ISamlAuthenticationProvider, UnconfiguredSamlAuthenticationProvider>();

        // Partner identity mapping (Phase 6 §2.2) — registered as a Singleton because
        // InMemoryPartnerIdentityMappingRepository's backing store must survive across
        // the per-request Scoped lifetime (a mapping registered by one request must be
        // resolvable by a later request). See that class's remarks for the documented
        // "not yet SQL-backed" limitation and extension seam.
        services.AddSingleton<PlantSafety.Domain.Identity.Repositories.IPartnerIdentityMappingRepository,
            PlantSafety.Infrastructure.Identity.InMemoryPartnerIdentityMappingRepository>();

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

        // Phase 6 §3.2 Enterprise Event Mesh -- outbound ERP/MES publishers (2026-08-03). No live
        // ERP/MES credentials exist in this environment: EnterpriseEventMeshOptions defaults every
        // endpoint to an empty string, and HttpExternalSystemPublisher gracefully no-ops (logs, does
        // not fail) whenever its configured endpoint is unset -- same "optional, gracefully
        // degrading" convention as BlobStorageOptions/AzureBlobStorageService above.
        services.Configure<EnterpriseEventMeshOptions>(configuration.GetSection(EnterpriseEventMeshOptions.SectionName));
        services.AddHttpClient("EnterpriseEventMesh.Erp", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });
        services.AddHttpClient("EnterpriseEventMesh.Mes", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });
        services.AddSingleton<IExternalSystemPublisher>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EnterpriseEventMesh.Erp");
            return new HttpExternalSystemPublisher(
                EnterpriseSourceSystem.Erp, opts.ErpEndpointUrl, client,
                sp.GetRequiredService<Application.Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer>(),
                sp.GetRequiredService<ILogger<HttpExternalSystemPublisher>>(),
                metricsRecorder: sp.GetRequiredService<Application.Abstractions.Observability.IRequestMetricsRecorder>());
        });
        services.AddSingleton<IExternalSystemPublisher>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EnterpriseEventMesh.Mes");
            return new HttpExternalSystemPublisher(
                EnterpriseSourceSystem.Mes, opts.MesEndpointUrl, client,
                sp.GetRequiredService<Application.Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer>(),
                sp.GetRequiredService<ILogger<HttpExternalSystemPublisher>>(),
                metricsRecorder: sp.GetRequiredService<Application.Abstractions.Observability.IRequestMetricsRecorder>());
        });

        // Phase 6 §1 Enterprise System Integrations (2026-08-03): outbound CMMS/HR/Vendor
        // publishers, same gracefully-degrading HTTP pattern as the ERP/MES publishers above.
        services.AddHttpClient("EnterpriseEventMesh.Cmms", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });
        services.AddHttpClient("EnterpriseEventMesh.Hr", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });
        services.AddHttpClient("EnterpriseEventMesh.Vendor", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });
        services.AddSingleton<IExternalSystemPublisher>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EnterpriseEventMesh.Cmms");
            return new HttpExternalSystemPublisher(
                EnterpriseSourceSystem.Cmms, opts.CmmsEndpointUrl, client,
                sp.GetRequiredService<Application.Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer>(),
                sp.GetRequiredService<ILogger<HttpExternalSystemPublisher>>(),
                metricsRecorder: sp.GetRequiredService<Application.Abstractions.Observability.IRequestMetricsRecorder>());
        });
        services.AddSingleton<IExternalSystemPublisher>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EnterpriseEventMesh.Hr");
            return new HttpExternalSystemPublisher(
                EnterpriseSourceSystem.Hr, opts.HrEndpointUrl, client,
                sp.GetRequiredService<Application.Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer>(),
                sp.GetRequiredService<ILogger<HttpExternalSystemPublisher>>(),
                metricsRecorder: sp.GetRequiredService<Application.Abstractions.Observability.IRequestMetricsRecorder>());
        });
        services.AddSingleton<IExternalSystemPublisher>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<EnterpriseEventMeshOptions>>().Value;
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EnterpriseEventMesh.Vendor");
            return new HttpExternalSystemPublisher(
                EnterpriseSourceSystem.Vendor, opts.VendorEndpointUrl, client,
                sp.GetRequiredService<Application.Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer>(),
                sp.GetRequiredService<ILogger<HttpExternalSystemPublisher>>(),
                metricsRecorder: sp.GetRequiredService<Application.Abstractions.Observability.IRequestMetricsRecorder>());
        });

        // Phase 7 §1.3 Tenant-Aware Eventing (2026-08-03): outbound Enterprise Event Mesh mappers
        // resolve each event's originating tenant Region via ITenantRegionRouter before constructing
        // the EnterpriseEventEnvelope -- singleton-registered, mirroring ITenantRegionRouter's own
        // singleton lifetime (pure, side-effect-free, no scoped dependencies).
        services.AddSingleton<Messaging.EnterpriseMesh.WorkOrderEnterpriseEventMapper>();
        services.AddSingleton<Messaging.EnterpriseMesh.Mes.ProductionOrderEnterpriseEventMapper>();
        services.AddSingleton<Messaging.EnterpriseMesh.Erp.PartStockConsumptionEnterpriseEventMapper>();
        services.AddSingleton<Messaging.EnterpriseMesh.Erp.FinancialRecordApprovedEnterpriseEventMapper>();

        // Search
        services.AddScoped<ISearchService, AzureSearchService>();

        // Messaging
        // MassTransit.IBus was previously unregistered (a real DI gap, closed here). Local dev
        // targets a local RabbitMQ broker via the "RabbitMq" config section; production
        // deployments override with real Azure Service Bus credentials.
        services.AddMassTransit(x =>
        {
            // Previously every {Module}ConsumerRegistration.Register(cfg) helper was a "TODO"
            // stub that never actually called cfg.AddConsumer<T>(), and none of these Register()
            // methods were ever invoked from here — so no MassTransit consumer (including all the
            // pre-existing *CreatedConsumer classes) was ever wired to the bus, despite the classes
            // themselves being fully implemented and idempotent (see IdempotentConsumerBase). This
            // was a real cross-module eventing gap, closed here; additive-only evolution.
            OperationalSafetyConsumerRegistration.Register(x);
            ComplianceConsumerRegistration.Register(x);
            AnalyticsConsumerRegistration.Register(x);
            SafetyEventsConsumerRegistration.Register(x);
            TrainingConsumerRegistration.Register(x);
            SustainabilityConsumerRegistration.Register(x);
            WorkerWellBeingConsumerRegistration.Register(x);
            EnvironmentalHealthConsumerRegistration.Register(x);
            EnterpriseFabricConsumerRegistration.Register(x);
            GovernanceConsumerRegistration.Register(x);
            HumanFactorsConsumerRegistration.Register(x);
            IntegrationsConsumerRegistration.Register(x);
            DigitalTwinConsumerRegistration.Register(x);
            ContractorVisitorConsumerRegistration.Register(x);
            AiGovernanceConsumerRegistration.Register(x);
            AiSafetyIntelligenceConsumerRegistration.Register(x);

            // Phase 5 §3 Cross-Module Orchestration (2026-08-02): the 8 real wiring points
            // connecting Maintenance<->PredictiveMaintenance, Production<->Scheduling,
            // RiskManagement<->ContinuousImprovement, and Intelligence<->everything. See
            // CrossModuleOrchestrationConsumerRegistration for the full consumer list.
            CrossModuleOrchestrationConsumerRegistration.Register(x);

            // Phase 8 §4 Multi-Site Safety Intelligence Network -- Event Mesh Extensions
            // (2026-08-03): GlobalSafetyTelemetryConsumer/CrossSiteActionConsumer, the real
            // internal cross-site event-mesh wiring points (distinct from the Phase 6 §3
            // Enterprise Event Mesh registered below, which is cross-SYSTEM/external).
            GlobalSafetyInsightsConsumerRegistration.Register(x);

            // Phase 6 §3 Enterprise Event Mesh (2026-08-03): cross-SYSTEM (platform <-> external
            // ERP/MES) outbound propagation wiring point. See EnterpriseEventMeshConsumerRegistration
            // and PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh for the envelope/publisher
            // abstractions.
            EnterpriseEventMeshConsumerRegistration.Register(x);

            // Phase 6 blocker fix (2026-08-02), reworked 2026-08-03: the single,
            // message-type-agnostic dead-letter fault sink. Originally registered as an
            // open-generic IConsumer<Fault<TMessage>> (DeadLetterFaultConsumer<TMessage> /
            // DeadLetterConsumerRegistration.Register(x)) -- that produced an internal MassTransit
            // RegisterConsumer<> marker type with an unbound generic parameter that
            // PlantSafety.Api's strict UseDefaultServiceProvider(ValidateOnBuild = true) eagerly
            // tried to Activator.CreateInstance at startup, crashing with "Cannot create an
            // instance of ... because Type.ContainsGenericParameters is true." Replaced with
            // DeadLetterFaultObserver (IConsumeObserver), a normal singleton with no open generics
            // anywhere -- see DeadLetterFaultObserver/DeadLetterEventPersister remarks. Connected
            // to the bus via cfg.ConnectConsumeObserver(...) below.
            services.AddSingleton<IDeadLetterEventPersister, DeadLetterEventPersister>();
            services.AddSingleton<DeadLetterFaultObserver>();

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

                // Handbook §7 Performance & Stress / Phase 5 §7.2 "parallel consumers" (2026-08-02):
                // no per-consumer ConcurrentMessageLimit convention previously existed anywhere in
                // this codebase (17 *ConsumerRegistration.Register(cfg) call sites, all bare
                // cfg.AddConsumer<T>() with no concurrency configuration). Rather than inventing a
                // per-file convention, this sets RabbitMQ's PrefetchCount once, centrally, which is
                // MassTransit's standard bus-wide throttle on unacknowledged/concurrently-processed
                // messages per receive endpoint -- the single lowest-risk place to bound consumer
                // parallelism without touching every registration file.
                cfg.PrefetchCount = InfrastructureConstants.DefaultRabbitMqPrefetchCount;

                // Phase 6 blocker fix (2026-08-02): bounded retry with exponential back-off,
                // applied bus-wide to every receive endpoint configured below (Consumer Hardening
                // Rules §12 "bounded retries with exponential backoff"). Previously there was NO
                // UseMessageRetry(...) middleware anywhere in this configuration, so any
                // transient consumer failure (e.g. a momentary SQL timeout) faulted the message on
                // the very first attempt with no retry at all, and nothing ever observed the
                // eventual failure -- see DeadLetterFaultObserver, connected below, which is what
                // actually persists the fault to [System].[DeadLetterEvents] once this retry
                // budget is exhausted.
                cfg.UseMessageRetry(r => r.Exponential(
                    InfrastructureConstants.DefaultRetryCount,
                    TimeSpan.FromMilliseconds(InfrastructureConstants.RetryBaseDelayMs),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMilliseconds(InfrastructureConstants.RetryBaseDelayMs)));

                // Reworked 2026-08-03 (see remarks above where DeadLetterFaultObserver is
                // registered as a singleton): connects the global fault observer to this bus so
                // ConsumeFault<T> fires for every permanently-faulted message across every
                // receive endpoint configured below, with no open-generic consumer registration.
                cfg.ConnectConsumeObserver(context.GetRequiredService<DeadLetterFaultObserver>());

                // Handbook §7 Performance & Stress / Outbox correctness fix (2026-08-02):
                // OutboxProcessor.PublishAsync deserialises the outbox payload back into the
                // concrete domain-event CLR type (using JsonSerializerConfiguration.Default,
                // which already registers this converter) and then calls bus.Publish(...) --
                // MassTransit's own transport serializer (this configuration) re-serialises that
                // object a second time onto the wire. Both boundaries must recognise this
                // codebase's Smart Enum convention (e.g. RiskManagement.Enums.RiskLevel) or the
                // second boundary would silently reintroduce the same round-trip failure.
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.Converters.Add(new SmartEnumJsonConverterFactory());

                    // Regression audit fix (2026-08-03): the transport-level (second) serialization
                    // boundary must recognise the same validating-constructor value-object
                    // convention (RiskId, CorrelationId, CausationId, UtcTimestamp,
                    // PerformedByActor, etc. -- see ValidatingValueObjectJsonConverterFactory
                    // remarks) as JsonSerializerConfiguration.Default, or this boundary would
                    // silently reintroduce the same zeroed-default-instance corruption on the wire.
                    options.Converters.Add(new PlantSafety.Infrastructure.Common.Serialization.ValidatingValueObjectJsonConverterFactory());
                    return options;
                });

                // Phase 6 blocker fix (2026-08-02): previously ABSENT from this configuration --
                // without ConfigureEndpoints, MassTransit never creates a receive endpoint for any
                // AddConsumer<T> registered above (17 pre-existing *ConsumerRegistration files and
                // the 8 Phase 5 cross-module orchestration consumers), so none of them would ever
                // actually receive a message regardless of retry/fault configuration. This call
                // uses MassTransit's standard consumer-type-name receive-endpoint convention (one
                // queue per consumer type), which DeadLetterEventPersister's ConsumerName recovery
                // (via ConsumeContext.ReceiveContext.InputAddress) also depends on.
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IIntegrationEventPublisher, ServiceBusEventPublisher>();
        services.AddScoped<IDeadLetterEventRepository, DeadLetterEventDapperRepository>();

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
        // 2026-08-03 (Claude Code modular-creation-agent): Incident timeline read repository
        // backing GetIncidentTimelineQuery (UI Gaps Scoping, IncidentManagement Part 1).
        services.AddScoped<PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement.IIncidentTimelineReadRepository,
            PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories.IncidentTimelineReadDapperRepository>();
        // 2026-08-03 (Claude Code modular-creation-agent): SLA policy repository backing the new
        // SLA Monitor engine (UI Gaps Scoping, IncidentManagement Part 2).
        services.AddScoped<PlantSafety.Application.SafetyEvents.SlaMonitoring.Repositories.ISlaPolicyRepository,
            PlantSafety.Infrastructure.SafetyEvents.SlaMonitoring.Repositories.SlaPolicyDapperRepository>();

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

        // Phase 7 §4.3/§4.4 Global Scalability & Multi-Region Deployment (2026-08-03):
        // deterministic, data-residency-aware region-failover decision. Pure domain logic --
        // Singleton since it is stateless and side-effect-free (no I/O of its own).
        services.AddSingleton<
            PlantSafety.Domain.MultiRegion.Services.IRegionFailoverPolicy,
            PlantSafety.Domain.MultiRegion.Services.RegionFailoverPolicy>();

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

        // Operations — Configuration, Phase 7 (SaaS Productization Blueprint §7.1 Feature Flag
        // System, §7.2 Release Pipeline): tenant/user/region-level feature flag override
        // repositories over [Release].[TenantFeatureFlagOverride] /
        // [Release].[UserFeatureFlagOverride] / [Release].[RegionFeatureFlagOverride] /
        // [Release].[TenantReleaseChannel] (migration
        // 200-tenant-user-region-feature-flag-overrides.sql), all co-located in
        // PlantSafety.ArchiveDB alongside the existing [System].[FeatureFlag] table. Consumed
        // by IFeatureFlagResolver (PlantSafety.Application.Operations.Configuration.Resolution)
        // via ResolveFeatureFlagQueryHandler.
        services.AddScoped<PlantSafety.Application.Operations.Configuration.Repositories.ITenantFeatureFlagOverrideRepository, Operations.Configuration.Persistence.TenantFeatureFlagOverrideDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operations.Configuration.Repositories.IUserFeatureFlagOverrideRepository, Operations.Configuration.Persistence.UserFeatureFlagOverrideDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operations.Configuration.Repositories.IRegionFeatureFlagOverrideRepository, Operations.Configuration.Persistence.RegionFeatureFlagOverrideDapperRepository>();
        services.AddScoped<PlantSafety.Application.Operations.Configuration.Repositories.ITenantReleaseChannelRepository, Operations.Configuration.Persistence.TenantReleaseChannelDapperRepository>();

        // Operations — Enterprise Secrets Management (Phase 6.6, §6.3). ISecretsProvider is
        // the ONLY type application code should ever resolve -- the concrete provider
        // (AzureKeyVaultSecretsProvider vs. ConfigurationSecretsProvider) is selected here,
        // based on an explicit "Secrets:Provider" override if present, else environment
        // (Production -> AzureKeyVault, everything else -> Configuration), and always wrapped
        // in AuditingSecretsProviderDecorator so every retrieval is logged (name + outcome
        // only, never the value). ConfigurationSecretsProvider itself throws in its own
        // constructor if ever resolved while IHostEnvironment.IsProduction() is true, as a
        // second, provider-level guard independent of this selection logic.
        services.Configure<SecretsProviderOptions>(configuration.GetSection(SecretsProviderOptions.SectionName));
        services.AddSingleton<AzureKeyVaultSecretsProvider>();
        services.AddScoped<ConfigurationSecretsProvider>();
        services.AddScoped<ISecretsProvider>(sp =>
        {
            var env = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecretsProviderOptions>>().Value;
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuditingSecretsProviderDecorator>>();

            var providerName = string.IsNullOrWhiteSpace(options.Provider)
                ? (env.IsProduction() ? "AzureKeyVault" : "Configuration")
                : options.Provider;

            ISecretsProvider inner = providerName == "AzureKeyVault"
                ? sp.GetRequiredService<AzureKeyVaultSecretsProvider>()
                : sp.GetRequiredService<ConfigurationSecretsProvider>();

            return new AuditingSecretsProviderDecorator(inner, logger);
        });

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
            .AddPhysicalAccessControlInfrastructure()
            .AddLogisticsTransportSafetyInfrastructure()
            .AddAutonomousVehicleSafetyInfrastructure()
            .AddCommunitySafetyInfrastructure()
            .AddPublicSafetyCommunicationInfrastructure()
            .AddCyberPhysicalSecurityInfrastructure()
            .AddEvacuationModelingInfrastructure()
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
            .AddProcessManagementAggregateInfrastructure()
            .AddSustainabilityAggregateInfrastructure()
            .AddSustainabilityAdvancedMetricsInfrastructure()
            .AddWorkerWellBeingAggregateInfrastructure()
            .AddComplianceCertificationInfrastructure()
            .AddComplianceRuleInfrastructure()
            .AddScoreCardsAggregateInfrastructure()
            .AddEngagementAggregateInfrastructure()
            .AddPredictiveAnalyticsAggregateInfrastructure()
            .AddPredictiveMaintenanceInfrastructure()
            .AddPreventiveMaintenanceTaskInfrastructure()
            .AddWorkOrdersInfrastructure()
            .AddProductionTrackingInfrastructure()
            .AddObservabilityInfrastructure()
            .AddChecklistsInfrastructure()
            .AddAuditingInfrastructure()
            .AddSafetyCultureInfrastructure()
            .AddAlertsInfrastructure()
            .AddDashboardsInfrastructure()
            .AddComplianceRulesEngineInfrastructure()
            .AddAuditTimelineInfrastructure()
            .AddAuditReportInfrastructure();

        // Phase 7 Tenancy.Tenant (2026-08-03): first-class Tenant aggregate + provisioning
        // pipeline (see docs/Phase7/tenant-isolation-model.md), and the tenant-scoped scheduling
        // rule override provider for Phase 5's PredictiveScheduling optimizer.
        services
            .AddTenantInfrastructure()
            .AddTenantSchedulingRuleOverrides(configuration);

        // SafetyPrograms (2026-08-03): thin composition-layer module over the existing
        // SafetyPolicies/SafetyProcedures/SafetyCulture modules -- see
        // docs/Phase5/ui-gaps-scoping.md section 8.
        services.AddSafetyProgramsInfrastructure();

        // ProcessManagement.Routing (2026-08-03): new sibling submodule alongside the existing
        // ProcessManagement.ProcessDefinition aggregate -- partial reuse of ProcessDefinition's
        // approval/versioning lifecycle plus genuinely new RouteDefinition/RouteVersionSnapshot
        // aggregates for ordered steps, diffing, and simulation -- see
        // docs/Phase5/ui-gaps-scoping.md section 9.
        services.AddRoutingAggregateInfrastructure();

        // Phase 4 full build-out (2026-08-02): previously-bare DDD aggregates AiGovernance,
        // AiSafetyIntelligence, ContractorVisitor, DigitalTwin. AiGovernance and DigitalTwin are
        // distinct concepts from the pre-existing, unrelated "GovernedModel"/"Twin" Features
        // verticals registered separately via .AddFeaturesLegacyRepositories() below.
        services
            .AddAiGovernanceModuleInfrastructure()
            .AddAiSafetyIntelligenceModuleInfrastructure()
            .AddContractorVisitorModuleInfrastructure()
            .AddDigitalTwinModuleInfrastructure();

        // Phase 4 full build-out (2026-08-02): previously-bare DDD aggregates
        // EnvironmentalHealth, Governance, HumanFactors, OperationalSafety. EnvironmentalHealth
        // is a distinct bounded context from the pre-existing, unrelated, LIVE
        // Features.EnvironmentalHealth "reading" vertical (registered separately, by
        // fully-qualified name, a few lines above via AddScoped<...IEnvironmentalHealthReadingRepository...>)
        // and from the separate, already-modernized EmployeeSafety.EnvironmentalHealthMonitoring
        // aggregate. OperationalSafety (this aggregate root) is likewise distinct from the
        // pre-existing, unrelated, LIVE OperationalSafety.Alerts/Checklists/PPEManagement/
        // RiskManagement/Schedules registrations above.
        services
            .AddEnvironmentalHealthAggregateInfrastructure()
            .AddWaterDischargeComplianceInfrastructure()
            .AddGovernanceAggregateInfrastructure()
            .AddHumanFactorsAggregateInfrastructure()
            .AddOperationalSafetyAggregateInfrastructure();

        // HumanFactors.CognitiveLoadAnalytics (Phase 4 full build-out, 2026-08-04): cognitive
        // workload measurement, task-complexity correlation, shift/workload trend analysis, and
        // predictive cognitive risk modeling. Sits alongside (not inside) the generic HumanFactors
        // aggregate registered immediately above -- mirrors the Sustainability.AdvancedMetrics
        // sub-module registration precedent a few lines below.
        services.AddHumanFactorsCognitiveLoadAnalyticsInfrastructure();

        // Phase 4 full build-out (2026-08-02): previously-bare/anemic DDD aggregates
        // EnterpriseFabric and Financials.FinancialRecord. EnterpriseFabric (this aggregate root)
        // is distinct from the pre-existing, unrelated, LIVE Features.EnterpriseFabric.Users
        // vertical (registered separately above via AddScoped<...IUserRepository...>) and the
        // pre-existing, unrelated Features.EnterpriseFabric.FabricNodes table. FinancialRecord is
        // a brand-new aggregate root (Financials had none before this pass) and is additive
        // alongside the pre-existing, unrelated AddFinancialsRepositories() read-only vertical
        // registered a few lines below.
        services
            .AddEnterpriseFabricAggregateInfrastructure()
            .AddFinancialRecordAggregateInfrastructure();

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
            .AddLocalEmergencyCoordinationInfrastructure()
            .AddIntegrationsInfrastructure()
            .AddCrossDomainRepositories()
            .AddFeaturesLegacyRepositories()
            .AddIntelligenceRepositories()
            .AddContinuousImprovementRepositories()
            .AddContinuousImprovementRCAAggregateInfrastructure()
            .AddPlanningProjectsRepositories()
            .AddHealthChecksRepositories()
            .AddFinancialsRepositories();

        // Phase 6 §1 Enterprise System Integrations (2026-08-03): the Machines reference-data
        // ingestion seam (inbound ERP asset-master sync -- see IMachineAssetReferenceDataIngestor's
        // own header for why this is not a fabricated CrossDomain "command") and the Enterprise
        // ERP cost-center-mapping reference-data repository.
        services.AddScoped<
            PlantSafety.Application.CrossDomain.Machines.Ingestion.IMachineAssetReferenceDataIngestor,
            PlantSafety.Infrastructure.CrossDomain.Machines.Persistence.MachineAssetReferenceDataDapperIngestor>();
        services.AddScoped<
            PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters.ICostCenterMappingRepository,
            PlantSafety.Infrastructure.Enterprise.Integrations.Erp.Persistence.CostCenterMappingDapperRepository>();

        // Phase 4 full build-out (2026-08-02): previously-bare/anemic DDD aggregates
        // PlanningProjects.Project and Intelligence.AIInvestigations. Project is a brand-new
        // aggregate root (PlanningProjects had only the ProjectMilestone child entity before this
        // pass) and is additive alongside the pre-existing, unrelated LIVE
        // Features.PlanningProjects.Handlers.Projects.IProjectRepository generic project-tracking
        // vertical and the pre-existing read-only AddPlanningProjectsRepositories() vertical above.
        // AIInvestigation is likewise a brand-new aggregate root (Intelligence.AIInvestigations
        // previously had only a placeholder marker class) and is additive alongside the
        // pre-existing, unrelated read-only AddIntelligenceRepositories() vertical above.
        services
            .AddPlanningProjectsAggregateInfrastructure()
            .AddAIInvestigationsAggregateInfrastructure();

        // Phase 4 full build-out (2026-08-02): previously-bare/anemic DDD aggregates
        // Intelligence.AITraining (AITrainingJob), Intelligence.DataLake (DataLakeActivity), and
        // Intelligence.PredictiveSafety (PredictiveSafetyAlert). Each previously had only a
        // placeholder marker class in the Domain layer; each is additive alongside the
        // pre-existing, unrelated read-only AddIntelligenceRepositories() vertical above.
        services.AddIntelligenceAggregatesInfrastructure();

        services.AddScoped<IComplianceBulkRepository, ComplianceBulkDapperRepository>();

        // Phase 5.5 Automation and workflow engine (2026-08-02): brand-new Workflow module
        // (workflow definitions + deterministic runtime engine), additive alongside every module
        // registered above.
        services.AddWorkflowInfrastructure();

        // Regression audit fix (2026-08-03): AddAnalyticsUnifiedModelDashboardInfrastructure()
        // existed fully-implemented (see UnifiedModelDashboardInfrastructureServiceRegistration
        // remarks -- self-documented as "NOT wired into the shared
        // InfrastructureServiceRegistration.cs") but was never called from here, so the six
        // unified-analytics-model BI dashboard repositories (Scheduling/Production/Maintenance/
        // Risk/ContinuousImprovement/Audit) could never be resolved. Closes the gap.
        services.AddAnalyticsUnifiedModelDashboardInfrastructure();

        // Phase 6 §5.3 Metrics & Dashboards (2026-08-03): PlatformHealthDashboard -- request
        // throughput/latency-percentile/error-rate KPIs (new [PlatformHealth].[RequestMetricEvent]
        // capture pipeline) composed with the EXISTING dead-letter/event-bus health infrastructure
        // (IDeadLetterEventRepository, already registered above) at the Application-layer handler.
        services.AddPlatformHealthDashboardInfrastructure();

        // Phase 6 §5.1 Distributed Tracing (2026-08-03): OpenTelemetry.Extensions.Hosting was
        // referenced in this project's .csproj but had ZERO AddOpenTelemetry()/WithTracing() call
        // anywhere in the solution -- a dead package reference. Wires real ASP.NET Core/
        // HttpClient/SqlClient span instrumentation so TraceId/SpanId (already read from
        // Activity.Current by OperationContextAccessor) correlate to an actual exported trace.
        services.AddPlantSafetyTracing(configuration);

        // Phase 7 §3 Billing, Metering & Subscription Management (2026-08-03): usage-metering
        // snapshot persistence, invoice persistence, tenant storage-usage provider, and the
        // payment-gateway extension seam (UnconfiguredPaymentProcessor by default).
        services.AddBillingInfrastructure();

        // Phase 8 §2/§4 Multi-Site Safety Intelligence Network (2026-08-03): GlobalSafetyInsights
        // Dapper repositories (write/read/audit-ledger) and the dashboard-facing mesh publisher.
        services.AddGlobalSafetyInsightsInfrastructure();

        // HazardousWasteManagement (2026-08-04): new top-level module -- full hazardous-waste
        // lifecycle (generation/characterization, accumulation, manifesting, transporter/TSDF
        // hand-off, disposal confirmation) with RCRA-style chain-of-custody. Integrates with
        // EnvironmentalHealth and Compliance.Procedures.HazMatProcedures (SOP linkage only; that
        // controller is left untouched) via event-based hooks; leaves an event-based integration
        // point for the future LogisticsTransportSafety module.
        services.AddHazardousWasteManagementInfrastructure();

        return services;
    }
}
