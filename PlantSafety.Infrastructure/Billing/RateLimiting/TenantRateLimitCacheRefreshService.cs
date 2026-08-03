// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.RateLimiting
//  Layer:          Infrastructure
//  File:           TenantRateLimitCacheRefreshService.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.4 License Enforcement)
//  SchemaVersion:  2026.1
//  Description:    Background hosted service periodically loading every registered tenant's
//                  active SubscriptionPlan rate limit into ITenantRateLimitCache, so the
//                  synchronous ASP.NET Core rate-limiter partition factory never needs to perform
//                  I/O on the hot request path. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.RateLimiting;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using Microsoft.Extensions.Hosting;

namespace PlantSafety.Infrastructure.Billing.RateLimiting;

/// <summary>Periodically refreshes <see cref="ITenantRateLimitCache"/> from every tenant's active SubscriptionPlan.</summary>
public sealed class TenantRateLimitCacheRefreshService(
    IServiceScopeFactory scopeFactory, ITenantRateLimitCache cache, ILogger<TenantRateLimitCacheRefreshService> logger)
    : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TenantRateLimitCacheRefreshService: failed to refresh tenant rate limits.");
            }

            try
            {
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantReadRepository = scope.ServiceProvider.GetRequiredService<ITenantReadRepository>();

        var tenants = await tenantReadRepository.ListAllAsync(cancellationToken);
        foreach (var tenant in tenants)
        {
            if (!SubscriptionPlanCode.TryParse(tenant.PlanCode, out var planCode))
                continue;

            var plan = SubscriptionPlanCatalog.Resolve(planCode);
            cache.SetRateLimitPerMinute(tenant.TenantId, plan.Limits.ApiRateLimitPerMinute);
        }

        logger.LogDebug("TenantRateLimitCacheRefreshService: refreshed rate limits for {TenantCount} tenants.", tenants.Count);
    }
}
