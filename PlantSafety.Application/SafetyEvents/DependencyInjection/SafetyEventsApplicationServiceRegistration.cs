// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers all Application-layer services specific to the SafetyEvents module.
//   Called from PlantSafety.Application's root ApplicationServiceRegistration.
//   All handlers, validators, and mapping profiles for this module are auto-registered by MediatR.
// SchemaVersion: 1

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Services;

namespace PlantSafety.Application.SafetyEvents.DependencyInjection;

/// <summary>
/// Extension methods to register SafetyEvents Application-layer services.
/// </summary>
public static class SafetyEventsApplicationServiceRegistration
{
    /// <summary>
    /// Registers SafetyEvents-specific Application services into the DI container.
    /// MediatR handlers and FluentValidation validators are registered via assembly scan
    /// in the root <c>AddApplicationServices</c> call; this method adds supplementary registrations.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddSafetyEventsApplicationServices(this IServiceCollection services)
    {
        // Mapping profiles (explicit, single-instance)
        services.AddSingleton<IncidentToIncidentDtoMappingProfile>();
        services.AddSingleton<IncidentToIncidentSummaryDtoMappingProfile>();

        // 2026-08-03 (Claude Code modular-creation-agent): SLA Monitor engine (UI Gaps Scoping,
        // IncidentManagement Part 2) -- policy resolver used by the three sla-monitor query handlers.
        services.AddScoped<SlaPolicyResolver>();

        return services;
    }
}
