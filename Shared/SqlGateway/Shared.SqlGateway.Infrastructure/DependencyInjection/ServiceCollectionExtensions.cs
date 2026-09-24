// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure.DependencyInjection
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-08-09
//  Description:    Registers ISqlGateway. Phase 3.6: ValidateOnStart now
//                  actually enforces the "fails fast if no connection
//                  string is configured" claim this file always documented
//                  but never implemented -- previously a missing connection
//                  string was silently accepted at registration time and
//                  only surfaced as a SqlException on a host's first real
//                  query. Also registers SqlCircuitBreaker, the dedicated
//                  per-dependency circuit breaker SqlServerGateway uses
//                  (Phase 3.6), distinct from any other dependency's.
//                  2026-09-24: also TryAdds SqlServerGateway's remaining
//                  dependencies (IClock, ILogEventWriter,
//                  ICorrelationContext) so AddSqlGateway alone yields a
//                  resolvable gateway -- see PlantSafety.RegressionTests
//                  PLNT-REG-DI-001..004. TryAdd keeps any host-supplied
//                  implementation.
// ============================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SqlGateway.Abstractions.Gateway;
using Shared.SqlGateway.Infrastructure.Options;
using Shared.SqlGateway.Infrastructure.Resilience;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SqlGateway.Infrastructure.DependencyInjection;

/// <summary>Validates SqlGatewayOptions at host startup.</summary>
internal sealed class SqlGatewayOptionsValidator : IValidateOptions<SqlGatewayOptions>
{
    public ValidateOptionsResult Validate(string? name, SqlGatewayOptions options) =>
        string.IsNullOrWhiteSpace(options.ConnectionString)
            ? ValidateOptionsResult.Fail($"{SqlGatewayOptions.SectionName}:ConnectionString is required but was missing or empty.")
            : ValidateOptionsResult.Success;
}

/// <summary>DI registration for the SQL gateway.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers ISqlGateway, bound from the "Shared:SqlGateway" configuration section. Fails at host startup (not first query) if no connection string is configured. Also registers default IClock, ILogEventWriter and ICorrelationContext implementations unless the host already registered its own.</summary>
    public static IServiceCollection AddSqlGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SqlGatewayOptions>()
            .Bind(configuration.GetSection(SqlGatewayOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SqlGatewayOptions>, SqlGatewayOptionsValidator>();
        services.AddSingleton<SqlCircuitBreaker>();

        // SqlServerGateway's other constructor dependencies. TryAdd so a host's own
        // registration (e.g. a FixedClock in deterministic mode) always wins.
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<ILogEventWriter, LogEventWriter>();
        services.TryAddScoped<ICorrelationContext, CorrelationContext>();

        services.AddScoped<ISqlGateway, SqlServerGateway>();
        return services;
    }
}
