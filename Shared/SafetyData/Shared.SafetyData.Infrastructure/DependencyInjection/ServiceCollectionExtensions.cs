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
//
//                  Phase 4.2: also registers Shared.SqlGateway's ISqlGateway
//                  (AddSqlGateway) -- SafetyDataDapperRepository now goes
//                  through it instead of owning its own SqlConnection. The
//                  caller's host must already have Shared.Deterministic's
//                  IClock and Shared.Telemetry's ILogEventWriter/
//                  ICorrelationContext registered (the same prerequisite
//                  AddSqlGateway always had), and must configure
//                  Shared:SqlGateway:ConnectionString to point at
//                  PlantSafety.CoreDB.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Repositories;
using Shared.SafetyData.Infrastructure.Options;
using Shared.SafetyData.Infrastructure.Persistence;
using Shared.SqlGateway.Infrastructure.DependencyInjection;

namespace Shared.SafetyData.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.SafetyData services with the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SafetyDataOptions"/> (bound from configuration section
    /// <see cref="SafetyDataOptions.SectionName"/>), Shared.SqlGateway's ISqlGateway, and
    /// <see cref="ISafetyDataRepository"/>. Read-only -- never registers any type capable
    /// of mutating PlantSafety's tenant data.
    /// </summary>
    public static IServiceCollection AddSharedSafetyData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SafetyDataOptions>(configuration.GetSection(SafetyDataOptions.SectionName));
        services.AddSqlGateway(configuration);
        services.AddScoped<ISafetyDataRepository, SafetyDataDapperRepository>();

        return services;
    }
}
