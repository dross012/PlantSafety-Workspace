// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    IServiceCollection extension methods for Infrastructure layer registration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for registering Infrastructure services.
/// Called from <c>InfrastructureServiceRegistration</c>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Validates that all required Infrastructure options sections are populated.</summary>
    public static IServiceCollection AddInfrastructureOptionsValidation(
        this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MessagingOptions>()
            .BindConfiguration(MessagingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CacheOptions>()
            .BindConfiguration(CacheOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
