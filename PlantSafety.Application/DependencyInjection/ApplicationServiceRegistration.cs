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
using PlantSafety.Application.Compliance.RulesEngine.DependencyInjection;
using PlantSafety.Application.Scheduling.PredictiveScheduling.DependencyInjection;

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

        // SafetyPrograms (2026-08-03): thin composition-layer module over the existing
        // SafetyPolicies/SafetyProcedures/SafetyCulture modules -- see
        // docs/Phase5/ui-gaps-scoping.md section 8.
        services.AddSafetyProgramsApplicationServices();

        return services;
    }
}
