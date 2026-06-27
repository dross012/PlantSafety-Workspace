// ============================================================================
// File: RedisHealthCheck.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: ASP.NET Core health check that verifies Azure Redis Cache connectivity
//              for the /health/ready readiness probe.
// ============================================================================
//
// Onboarding Notes:
// - Registered in Extensions/ServiceCollectionExtensions.cs and mapped to the
//   /health/ready readiness endpoint in Program.cs.
// - Verifies connectivity by executing a PING command against the Redis instance.
// - Redis is the distributed cache for KPI snapshots (TTL 5 min), user permission sets
//   (TTL 10 min), role definitions (TTL 30 min), and lookup tables (TTL 1 hr).
//   See Architecture Specification §7.4 (Caching Strategy).
// - The Redis connection string is retrieved from Azure Key Vault via IConfiguration.
//
// Rationale:
// - If Redis is unavailable, cache-aside reads fall through to the database but write-back
//   will fail. Detecting this in the readiness probe allows k8s to delay traffic until
//   the cache connection is restored.
//
// Future Extension Seams:
// - Add memory usage and key count metrics to the health check data dictionary.
// - Support Redis Sentinel health check for premium geo-replicated configurations.
//
// ============================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace PlantSafety.Api.HealthChecks;

/// <summary>
/// Health check that validates Azure Redis Cache connectivity by executing a PING command.
/// Used by the /health/ready readiness probe.
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private const string ConnectionStringKey = "ConnectionStrings:RedisCache";
    private readonly IConfiguration _configuration;
    private readonly ILogger<RedisHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RedisHealthCheck"/>.
    /// </summary>
    /// <param name="configuration">Configuration provider backed by Azure Key Vault in production.</param>
    /// <param name="logger">Logger for recording health check failures.</param>
    public RedisHealthCheck(IConfiguration configuration, ILogger<RedisHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Executes a PING command against the Azure Redis Cache instance to verify connectivity.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see cref="HealthCheckResult.Healthy"/> if Redis responds to PING;
    /// <see cref="HealthCheckResult.Unhealthy"/> otherwise.
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration[ConnectionStringKey];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return HealthCheckResult.Unhealthy("Redis connection string is not configured.");
            }

            await using var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var db = redis.GetDatabase();
            var pong = await db.PingAsync();

            return pong.TotalMilliseconds < 1000
                ? HealthCheckResult.Healthy($"Redis is healthy. Latency: {pong.TotalMilliseconds:F1}ms.")
                : HealthCheckResult.Degraded($"Redis responded but latency is elevated: {pong.TotalMilliseconds:F1}ms.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed.");
            return HealthCheckResult.Unhealthy("Redis connection failed.", ex);
        }
    }
}
