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
    /// 4. AuthorizationBehavior
    /// 5. CachingBehavior
    /// 6. ValidationBehavior
    /// 7. ConcurrencyBehavior
    /// 8. TransactionBehavior (innermost — wraps the handler only)
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
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ConcurrencyBehavior<,>));
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
            .AddEnterpriseFabricApplicationServices();

        return services;
    }
}
