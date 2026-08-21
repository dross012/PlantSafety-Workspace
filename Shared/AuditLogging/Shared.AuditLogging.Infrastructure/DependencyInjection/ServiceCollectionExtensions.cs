// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Infrastructure.DependencyInjection
//  Layer:          Infrastructure
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration for Shared.AuditLogging. Binds AuditLoggingOptions
//                  from configuration section "Shared:AuditLogging" and registers
//                  IAuditLogRepository. Naming mirrors the house convention of a single
//                  `Add<Module>` extension per module found across PlantSafety.
//                  Infrastructure's DependencyInjection\InfrastructureServiceRegistration
//                  and Reporting.Infrastructure's DataSources\Extensions\
//                  DataSourceServiceCollectionExtensions.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.AuditLogging.Abstractions.Repositories;
using Shared.AuditLogging.Infrastructure.Options;
using Shared.AuditLogging.Infrastructure.Persistence;

namespace Shared.AuditLogging.Infrastructure.DependencyInjection;

/// <summary>Registers Shared.AuditLogging services with the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AuditLoggingOptions"/> (bound from configuration section
    /// <see cref="AuditLoggingOptions.SectionName"/>) and <see cref="IAuditLogRepository"/>.
    /// Read-only -- never registers any type capable of mutating dbo.AuditLog.
    /// </summary>
    public static IServiceCollection AddSharedAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuditLoggingOptions>(configuration.GetSection(AuditLoggingOptions.SectionName));
        services.AddScoped<IAuditLogRepository, AuditLogDapperRepository>();

        return services;
    }
}
