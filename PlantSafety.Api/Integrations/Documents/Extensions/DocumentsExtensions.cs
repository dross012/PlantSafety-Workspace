// ============================================================================
// File: DocumentsExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: DI registration extension for Integrations.Documents services.
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

namespace PlantSafety.Api.Integrations.Documents.Extensions;

/// <summary>DI registration extension for the Integrations.Documents module.</summary>
public static class DocumentsExtensions
{
    /// <summary>
    /// Registers Integrations.Documents services into the DI container.
    /// Application-layer handlers are registered in PlantSafety.Application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDocuments(this IServiceCollection services)
    {
        // Sub-area specific registrations (validators, mappers, cache policies) go here.
        return services;
    }
}
