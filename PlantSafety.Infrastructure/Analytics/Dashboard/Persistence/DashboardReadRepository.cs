// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Dashboard Application-layer root-aggregate read-model.
//                  Backs GetDashboardByIdQuery. Reuses usp_Dashboard_GetById (the same proc
//                  DashboardAggregateDapperRepository uses to hydrate the write-side aggregate)
//                  rather than introducing a duplicate stored procedure. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.Models;
using PlantSafety.Application.Analytics.Dashboard.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;

/// <summary>Dapper-backed repository for Dashboard Application-layer read-model queries.</summary>
public sealed class DashboardReadRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<DashboardReadRepository> logger)
    : IDashboardReadRepository
{
    /// <inheritdoc/>
    public async Task<DashboardItemDto?> GetByIdAsync(Guid dashboardId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for Dashboard {DashboardId} Tenant {TenantId}", dashboardId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("DashboardId", dashboardId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DashboardItemDto>(
            "[Analytics].[usp_Dashboard_GetById]", parameters, cancellationToken);
    }
}
