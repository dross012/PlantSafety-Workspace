// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Analytics.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper read-only repository for Safety Dashboard analytics. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.Analytics.Handlers.Dashboard;
using DashboardReadModel = PlantSafety.Application.Features.Analytics.Handlers.Dashboard.DashboardReadModel;

namespace PlantSafety.Infrastructure.Features.Analytics.Dashboard.Repositories;

/// <summary>
/// Dapper-backed read-only repository for the <c>Safety Dashboard</c> analytics.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DashboardDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<DashboardDapperRepository> logger)
    : IDashboardRepository
{
    /// <inheritdoc/>
    public async Task<DashboardReadModel?> GetDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Dashboard snapshot for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DashboardReadModel>(
            "[Analytics].[usp_Dashboard_GetSnapshot]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SafetyReportReadModel?> GetReportAsync(
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Safety Report for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", fromUtc.UtcDateTime)
            .AddDateTime("ToUtc", toUtc.UtcDateTime)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<SafetyReportReadModel>(
            "[Analytics].[usp_Dashboard_GetReport]", parameters, cancellationToken);
    }
}
