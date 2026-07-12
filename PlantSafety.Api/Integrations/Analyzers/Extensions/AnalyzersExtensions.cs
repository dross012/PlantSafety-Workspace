// ============================================================================
// File: AnalyzersExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: DI registration extension for Integrations.Analyzers services.
// ============================================================================
//
// Onboarding Notes:
// - Called from ServiceCollectionExtensions or a module-level DI aggregator.
//
// Rationale:
// - Isolating per-sub-area DI ensures validators, mappers, or cache policies can be added without touching global ServiceCollectionExtensions.
//
// Future Extension Seams:
// - Register sub-area-specific IOutputCachePolicy, FluentValidation validators, or Mapster profiles here when they diverge from module defaults.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analyzers.Extensions;

/// <summary>DI registration extension for the Integrations.Analyzers module.</summary>
public static class AnalyzersExtensions
{
    /// <summary>
    /// Registers Integrations.Analyzers services into the DI container.
    /// Application-layer handlers are registered in PlantSafety.Application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAnalyzers(this IServiceCollection services)
    {
        // Sub-area specific registrations (validators, mappers, cache policies) go here.
        return services;
    }
}
