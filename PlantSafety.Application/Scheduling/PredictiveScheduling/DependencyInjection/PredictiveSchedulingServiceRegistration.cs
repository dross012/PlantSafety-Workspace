// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Registers the Phase 5 predictive scheduling engine's Application-layer
// services: the rule-engine domain services, prediction models, and optimization strategies.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Orchestration;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.DependencyInjection;

/// <summary>Extension methods to register the predictive scheduling engine's Application-layer services.</summary>
public static class PredictiveSchedulingServiceRegistration
{
    /// <summary>
    /// Registers the rule engine, prediction models, and optimization strategies used by the
    /// predictive scheduling engine (Phase 5, Section 1). MediatR handlers/validators are
    /// auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPredictiveSchedulingServices(this IServiceCollection services)
    {
        // Domain rule-engine services (stateless -- safe as singletons).
        services.AddSingleton<ConstraintEvaluator>();
        services.AddSingleton<ResourceAvailabilityModel>();
        services.AddSingleton<PriorityWeightingService>();

        // Predictive model integration.
        services.AddSingleton<TaskDurationPredictor>();
        services.AddSingleton<ResourceConflictPredictor>();
        services.AddSingleton<OptimalSchedulingWindowPredictor>();
        services.AddSingleton<FailureLikelihoodPredictor>();

        // Optimization loop.
        services.AddSingleton<IOptimizationStrategy, GreedyOptimizationStrategy>();
        services.AddSingleton<IOptimizationStrategy, ConstraintSatisfactionStrategy>();
        services.AddSingleton<IOptimizationStrategy, GeneticAlgorithmOptimizationStrategy>();
        services.AddSingleton<MultiObjectiveScheduleOptimizer>();

        // Event-driven reprioritization orchestration.
        services.AddSingleton<SchedulingReprioritizationOrchestrator>();

        return services;
    }
}
