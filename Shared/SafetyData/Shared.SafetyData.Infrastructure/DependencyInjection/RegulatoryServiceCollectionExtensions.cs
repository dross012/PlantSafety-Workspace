// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           RegulatoryServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Regulatory Subsystem. Registers
//                  IRegulatoryRequirementRegistry -> RegulatoryRequirement
//                  RegistryProvider as a singleton; the provider is stateless
//                  and delegates every call to the static
//                  RegulatorRequirementCatalog/RegulatorCategoryCatalog/
//                  RegulatorRequirementMapping, so a single shared instance is
//                  safe. Mirrors IndustryServiceCollectionExtensions.
//                  AddIndustries()'s naming/shape exactly.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Regulatory Subsystem's services with the DI container.</summary>
public static class RegulatoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IRegulatoryRequirementRegistry"/> (backed by
    /// <see cref="RegulatoryRequirementRegistryProvider"/>) as a singleton over the static
    /// Regulatory Subsystem catalogs.
    /// </summary>
    public static IServiceCollection AddRegulatoryRequirements(this IServiceCollection services)
    {
        services.AddSingleton<IRegulatoryRequirementRegistry, RegulatoryRequirementRegistryProvider>();

        return services;
    }
}
