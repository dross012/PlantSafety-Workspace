// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           IndustryServiceCollectionExtensions.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for the Industry subsystem. Registers
//                  IIndustryRegistry -> IndustryRegistryProvider as a singleton;
//                  IndustryRegistryProvider is stateless and delegates every
//                  call to the static IndustryRegistry (Abstractions.Models.
//                  Industries.IndustryRegistry.All), so a single shared
//                  instance is safe. Mirrors ServiceCollectionExtensions'
//                  AddSharedSafetyData naming/shape.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Infrastructure.Industries;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers the Industry subsystem's services with the DI container.</summary>
public static class IndustryServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IIndustryRegistry"/> (backed by <see cref="IndustryRegistryProvider"/>)
    /// as a singleton over the static <see cref="IndustryRegistry"/> taxonomy.
    /// </summary>
    public static IServiceCollection AddIndustries(this IServiceCollection services)
    {
        services.AddSingleton<IIndustryRegistry, IndustryRegistryProvider>();

        return services;
    }
}
