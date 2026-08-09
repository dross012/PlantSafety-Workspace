// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure.DependencyInjection
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-08-09
//  Description:    Registers ISqlGateway. Fails fast at registration time
//                  if no connection string is configured, rather than
//                  letting every caller discover it later as a runtime
//                  SqlException.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.SqlGateway.Abstractions.Gateway;
using Shared.SqlGateway.Infrastructure.Options;

namespace Shared.SqlGateway.Infrastructure.DependencyInjection;

/// <summary>DI registration for the SQL gateway.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers ISqlGateway, bound from the "Shared:SqlGateway" configuration section.</summary>
    public static IServiceCollection AddSqlGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SqlGatewayOptions>(configuration.GetSection(SqlGatewayOptions.SectionName));
        services.AddScoped<ISqlGateway, SqlServerGateway>();
        return services;
    }
}
