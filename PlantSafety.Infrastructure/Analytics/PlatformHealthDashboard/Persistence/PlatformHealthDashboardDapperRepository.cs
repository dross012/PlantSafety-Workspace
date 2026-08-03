// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PlatformHealthDashboard
//  Layer:          Infrastructure
//  File:           PlatformHealthDashboardDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository implementing IPlatformHealthDashboardRepository, backed by
//                  usp_PlatformHealthDashboard_GetSummary over the new
//                  [PlatformHealth].[RequestMetricEvent] table. Mirrors the established
//                  UnifiedModelDashboardDapperRepository pattern (Handbook Phase 6 §5.3).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Repositories;

namespace PlantSafety.Infrastructure.Analytics.PlatformHealthDashboard.Persistence;

/// <summary>
/// Dapper-backed repository implementing <see cref="IPlatformHealthDashboardRepository"/>, backed
/// by <c>[PlatformHealth].[usp_PlatformHealthDashboard_GetSummary]</c>.
/// </summary>
public sealed class PlatformHealthDashboardDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PlatformHealthDashboardDapperRepository> logger) : IPlatformHealthDashboardRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlatformHealthDashboardItemDto>> GetPlatformHealthDashboardAsync(
        Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPlatformHealthDashboardAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PlatformHealthDashboardItemDto>(
            "[PlatformHealth].[usp_PlatformHealthDashboard_GetSummary]", parameters, cancellationToken);
    }
}
