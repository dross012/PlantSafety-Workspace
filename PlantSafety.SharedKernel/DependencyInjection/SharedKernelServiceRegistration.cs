// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.DependencyInjection
//  Layer:          SharedKernel
//  File:           SharedKernelServiceRegistration.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical named alias for AddSharedKernel(). Provides a
//                  discoverable entry point aligned with the SafetyCLI
//                  scaffolding token pattern. Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.SharedKernel.Serialization.Converters;
using PlantSafety.SharedKernel.Serialization.Json;

namespace PlantSafety.SharedKernel.DependencyInjection;

/// <summary>
/// Canonical SharedKernel service registration.
/// <para>
/// Delegates to <see cref="DependencyInjection.AddSharedKernel"/> for all
/// core registrations and additionally wires up JSON serialization options.
/// </para>
/// </summary>
public static class SharedKernelServiceRegistration
{
    /// <summary>
    /// Registers all SharedKernel services: primitives, serialization,
    /// and cross-cutting infrastructure contracts.
    /// </summary>
    public static IServiceCollection AddSharedKernelServices(this IServiceCollection services)
    {
        // Core SharedKernel (no-op for abstractions, stable extension seam)
        services.AddSharedKernel();

        // Singleton JSON options used by the serialization pipeline
        services.AddSingleton(JsonOptionsFactory.Create());

        return services;
    }
}
