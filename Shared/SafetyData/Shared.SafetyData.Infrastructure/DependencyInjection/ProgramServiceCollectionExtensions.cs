// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           ProgramServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Safety Programs subsystem.
//                  Registers IProgramRegistry -> ProgramRegistryProvider as a
//                  singleton; the provider is stateless and delegates every
//                  call to the static ProgramCatalog/ProgramRequirement
//                  Mapping, so a single shared instance is safe. Mirrors
//                  RegulatoryServiceCollectionExtensions.
//                  AddRegulatoryRequirements()'s and
//                  IndustryServiceCollectionExtensions.AddIndustries()'s
//                  naming/shape exactly.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Safety Programs subsystem's services with the DI container.</summary>
public static class ProgramServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IProgramRegistry"/> (backed by <see cref="ProgramRegistryProvider"/>)
    /// as a singleton over the static Safety Programs subsystem catalogs.
    /// </summary>
    public static IServiceCollection AddSafetyPrograms(this IServiceCollection services)
    {
        services.AddSingleton<IProgramRegistry, ProgramRegistryProvider>();

        return services;
    }
}
