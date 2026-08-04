// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Application layer dependency injection registration entry point.
//   Registers: MediatR handlers, FluentValidation validators, pipeline behaviors (in order),
//   and all Application layer services with the DI container.
//   Called from PlantSafety.Api Program.cs via AddApplicationServices().
//   No infrastructure types are registered here — those belong in Infrastructure project.
// SchemaVersion: 1
// Additive-only evolution: new registrations may be added; no existing registrations may be removed.

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Analytics.DependencyInjection;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Behaviors.Logging;
using PlantSafety.Application.Behaviors.Metrics;
using PlantSafety.Application.Behaviors.Performance;
using PlantSafety.Application.Behaviors.Security;
using PlantSafety.Application.Behaviors.Transactions;
using PlantSafety.Application.Behaviors.Validation;
using PlantSafety.Application.Compliance.DependencyInjection;
using PlantSafety.Application.ContinuousImprovement.DependencyInjection;
using PlantSafety.Application.EnterpriseFabric.DependencyInjection;
using PlantSafety.Application.Financials.DependencyInjection;
using PlantSafety.Application.Operational.DependencyInjection;
using PlantSafety.Application.PlanningProjects.DependencyInjection;
using PlantSafety.Application.SafetyEvents.DependencyInjection;
using PlantSafety.Application.EmployeeSafety.DependencyInjection;
using PlantSafety.Application.ContractorSafety.DependencyInjection;
using PlantSafety.Application.MachineSafety.DependencyInjection;
using PlantSafety.Application.SafetyProcedures.DependencyInjection;
using PlantSafety.Application.Alerts.DependencyInjection;
using PlantSafety.Application.Dashboards.DependencyInjection;
using PlantSafety.Application.SafetyPolicies.DependencyInjection;
using PlantSafety.Application.AccidentManagement.DependencyInjection;
using PlantSafety.Application.RiskManagement.DependencyInjection;
using PlantSafety.Application.Scorecards.DependencyInjection;
using PlantSafety.Application.Scheduling.DependencyInjection;
using PlantSafety.Application.PPE.DependencyInjection;
using PlantSafety.Application.PermitToWork.DependencyInjection;
using PlantSafety.Application.Observability.DependencyInjection;
using PlantSafety.Application.Checklists.DependencyInjection;
using PlantSafety.Application.Auditing.DependencyInjection;
using PlantSafety.Application.SafetyCulture.DependencyInjection;
using PlantSafety.Application.SafetyPrograms.DependencyInjection;
using PlantSafety.Application.Workflow.Behaviors;
using PlantSafety.Application.Workflow.DependencyInjection;
using PlantSafety.Application.Auditing.AuditReport.DependencyInjection;
using PlantSafety.Application.Auditing.AuditTimeline.DependencyInjection;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DependencyInjection;
using PlantSafety.Application.Compliance.RulesEngine.DependencyInjection;
using PlantSafety.Application.Scheduling.PredictiveScheduling.DependencyInjection;
using PlantSafety.Application.Operations.DependencyInjection;
using PlantSafety.Application.Enterprise.EventMesh.DependencyInjection;
using PlantSafety.Application.Tenancy.Tenant.DependencyInjection;
using PlantSafety.Application.Billing.DependencyInjection;
using PlantSafety.Application.Support.Impersonation.DependencyInjection;
using PlantSafety.Application.GlobalSafetyInsights.DependencyInjection;

namespace PlantSafety.Application.DependencyInjection;

/// <summary>
/// Extension method class for registering the Application layer with the DI container.
/// Called once from the composition root (API or test host).
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers all Application layer services: MediatR, validators, pipeline behaviors, and options.
    /// </summary>
    /// <param name="services">The DI service collection to register into.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Pipeline behavior registration order is critical (outermost first):
    /// 1. LoggingBehavior (outermost — wraps everything)
    /// 2. MetricsBehavior
    /// 3. PerformanceBehavior
    /// 4. TenantIsolationBehavior
    /// 5. AuthorizationBehavior
    /// 6. CachingBehavior
    /// 7. ValidationBehavior
    /// 8. ConcurrencyBehavior
    /// 9. WorkflowInstanceLockBehavior (Workflow module only; no-op for every other request)
    /// 10. TransactionBehavior (innermost — wraps the handler only)
    /// </remarks>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        // MediatR — scans the Application assembly for all IRequestHandler implementations
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // FluentValidation — scans the Application assembly for all AbstractValidator<T> implementations
        services.AddValidatorsFromAssembly(assembly, lifetime: ServiceLifetime.Scoped);

        // Pipeline behaviors (registration order = execution order; outermost registered first)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TenantIsolationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(WorkflowInstanceLockBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Options
        services.Configure<PerformanceOptions>(opts =>
        {
            opts.SlowRequestThresholdMs = Common.Constants.ApplicationConstants.DefaultSlowRequestThresholdMs;
        });

        // Module-specific registrations (mapping profiles, module-scoped singletons)
        services
            .AddSafetyEventsApplicationServices()
            .AddAnalyticsApplicationServices()
            .AddComplianceApplicationServices()
            .AddFinancialsApplicationServices()
            .AddPlanningProjectsApplicationServices()
            .AddOperationalApplicationServices()
            .AddContinuousImprovementApplicationServices()
            .AddEnterpriseFabricApplicationServices()
            .AddEmployeeSafetyApplicationServices()
            .AddContractorSafetyApplicationServices()
            .AddMachineSafetyApplicationServices()
            .AddSafetyProceduresApplicationServices()
            .AddAlertsApplicationServices()
            .AddDashboardsApplicationServices()
            .AddSafetyPoliciesApplicationServices()
            .AddAccidentManagementApplicationServices()
            .AddRiskManagementApplicationServices()
            .AddScorecardsApplicationServices()
            .AddSchedulingApplicationServices()
            .AddPPEApplicationServices()
            .AddPermitToWorkApplicationServices()
            .AddObservabilityApplicationServices()
            .AddChecklistsApplicationServices()
            .AddAuditingApplicationServices()
            .AddSafetyCultureApplicationServices()
            // Phase 5.5 Automation and workflow engine (2026-08-02): brand-new Workflow module
            // (workflow definitions + runtime engine), additive alongside every module registered
            // above.
            .AddWorkflowApplicationServices();

        // Regression audit fix (2026-08-03): these four Application-layer registration extension
        // methods were fully implemented but never called from here, so their services could
        // never be resolved (AuditReportFactory/AuditReportContentBuilder, AuditTimeline's
        // Application services, the Compliance RulesEngine Application services, and the Phase 5
        // predictive scheduling engine's rule/prediction/optimization services).
        services
            .AddAuditReportApplicationServices()
            .AddAuditTimelineApplicationServices()
            .AddComplianceRulesEngineApplicationServices()
            .AddPredictiveSchedulingServices();

        // Cross-module read-model composition providers (2026-08-03): registers every
        // Operations.* submodule's IReportSectionProvider<,> implementation, resolved via DI by
        // Operations.SystemHealth's rollup handler instead of dispatching sibling submodules'
        // Queries directly through ISender (Handbook Part IX §9.3.2).
        services.AddOperationsReadModelCompositionServices();

        // SafetyPrograms (2026-08-03): thin composition-layer module over the existing
        // SafetyPolicies/SafetyProcedures/SafetyCulture modules -- see
        // docs/Phase5/ui-gaps-scoping.md section 8.
        services.AddSafetyProgramsApplicationServices();

        // Phase 6.6 Enterprise Deployment & Release Architecture (2026-08-03), §6.2 Blue/Green
        // + Canary + Tenant Rollout: deterministic tenant-rollout evaluator layered on top of
        // the existing Operations.Configuration feature-flag system's new RolloutPercentage
        // column. Singleton: pure, stateless, side-effect-free (SHA-256 hash computation only).
        services.AddSingleton<Common.Rollout.ITenantRolloutPolicy, Common.Rollout.FeatureRolloutEvaluator>();

        // Phase 7 §7.1 Feature Flag System (2026-08-03): 4-level precedence resolver
        // (user > tenant > region > global), composing the above ITenantRolloutPolicy with
        // the new tenant/user/region override repositories. Singleton: pure, stateless,
        // side-effect-free composition logic (all I/O happens in the repositories the
        // ResolveFeatureFlagQueryHandler injects separately).
        services.AddSingleton<Operations.Configuration.Resolution.IFeatureFlagResolver, Operations.Configuration.Resolution.FeatureFlagResolver>();

        // Phase 7 §7.2 Release Pipeline (2026-08-03): pure threshold/sample-size rollback
        // trigger policy consumed by TriggerRollbackCommandHandler / a scheduled health-signal
        // job. Singleton: pure, stateless, side-effect-free.
        services.AddSingleton<Operations.Configuration.Release.IRollbackAutomationPolicy, Operations.Configuration.Release.RollbackAutomationPolicy>();

        // Phase 6 §3 Enterprise Event Mesh (2026-08-03, Eventing & Messaging Agent): cross-SYSTEM
        // (platform <-> external ERP/MES/CMMS) event exchange -- inbound translators + the
        // event-upgrade pipeline. See PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh.
        services.AddEnterpriseEventMeshApplicationServices();

        // Phase 7 §4.3 Global Scalability & Multi-Region Deployment (2026-08-03): deterministic
        // tenant-to-region routing (falls back to a stable SHA-256 hash across the known
        // regions when no explicit override is configured). Singleton: pure, stateless,
        // side-effect-free. See PlantSafety.Application.Enterprise.MultiRegion.
        // NOTE: this method has no IConfiguration parameter, so TenantRegionAffinityOptions is
        // bound in PlantSafety.Api's ServiceCollectionExtensions (where IConfiguration is
        // already available for RpoRtoOptions/GeoReplicationOptions), not here. Without an
        // explicit Configure<T> call, IOptions<TenantRegionAffinityOptions> still resolves via
        // its class defaults (an empty Overrides dictionary) if the Api layer's binding is ever
        // skipped, so this registration is safe standalone too.
        services.AddSingleton<Enterprise.MultiRegion.ITenantRegionRouter, Enterprise.MultiRegion.TenantRegionAffinityResolver>();

        // Phase 7 §5.3 Data Residency (2026-08-03, Security & Compliance Agent): real,
        // deterministic enforcement that a tenant's configured DataResidencyPolicy is satisfied
        // by the platform's current (single-region, in this sandbox) deployment region. Pure,
        // stateless, side-effect-free -- singletons, matching the sibling MultiRegion services
        // registered immediately above.
        services.AddSingleton<Enterprise.MultiRegion.DataResidency.ICurrentDeploymentRegionProvider, Enterprise.MultiRegion.DataResidency.ConfiguredCurrentDeploymentRegionProvider>();
        services.AddSingleton<Enterprise.MultiRegion.DataResidency.ITenantDataResidencyProfileStore, Enterprise.MultiRegion.DataResidency.ConfiguredTenantDataResidencyProfileStore>();
        services.AddSingleton<Enterprise.MultiRegion.DataResidency.IDataResidencyEnforcer, Enterprise.MultiRegion.DataResidency.DataResidencyEnforcer>();

        // Regression audit fix (2026-08-03, Billing agent): Tenancy.Tenant's own Application-layer
        // registration (ITenantFactory) was fully implemented but never called from here -- the
        // exact same class of gap flagged elsewhere in this file -- so ProvisionTenantCommandHandler
        // could never be constructed. Discovered while wiring this pass's new Billing module, which
        // depends on Tenant provisioning/plan assignment working end-to-end.
        services.AddTenantApplicationServices();

        // Phase 7 §3 Billing, Metering & Subscription Management (2026-08-03): SubscriptionPlan
        // catalog, usage metering composition, and the Invoice billing engine. See
        // BillingApplicationServiceRegistration remarks for why this method is currently a no-op
        // beyond the assembly-wide MediatR scan.
        services.AddBillingApplicationServices();

        // Phase 7 §6.3 Support Tooling (2026-08-03): tenant-impersonation session
        // start/end/query. See SupportApplicationServiceRegistration remarks for why this method
        // is currently a no-op beyond the assembly-wide MediatR scan.
        services.AddSupportApplicationServices();

        // Phase 8 §2/§4 Multi-Site Safety Intelligence Network (2026-08-03): GlobalSafetyInsights
        // domain-service singletons (Normalizer/KpiEngine/AnomalyDetector/RiskScorer/
        // RecommendationComposer/Factory). Command/query handlers are auto-registered via the
        // assembly-wide MediatR scan above.
        services.AddGlobalSafetyInsightsApplicationServices();

        // Phase 9 §5.6 Compliance Report Generation (2026-08-03): registers
        // TenantAuditStreamSectionProvider, the cross-module read-model composition provider the
        // new GetComplianceReportQuery uses to resolve the audit-trail/incident-lineage section.
        services.AddComplianceMonitoringApplicationServices();

        return services;
    }
}
