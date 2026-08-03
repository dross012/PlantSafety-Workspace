// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for Shared.SafetyData. Binds SafetyDataOptions
//                  from configuration section "Shared:SafetyData" and registers
//                  ISafetyDataRepository. Mirrors Shared.AuditLogging.
//                  Infrastructure.DependencyInjection.ServiceCollectionExtensions'
//                  AddSharedAuditLogging naming/shape.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.Options;
using Shared.SafetyData.Infrastructure.Persistence;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.SafetyData services with the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SafetyDataOptions"/> (bound from configuration section
    /// <see cref="SafetyDataOptions.SectionName"/>) and <see cref="ISafetyDataRepository"/>.
    /// Read-only -- never registers any type capable of mutating PlantSafety's tenant data.
    /// </summary>
    public static IServiceCollection AddSharedSafetyData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SafetyDataOptions>(configuration.GetSection(SafetyDataOptions.SectionName));
        services.AddScoped<ISafetyDataRepository, SafetyDataDapperRepository>();

        return services;
    }
}
