// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Dashboard aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;

/// <summary>Dapper-backed repository for <see cref="DashboardReadModel"/> queries.</summary>
public sealed class DashboardRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<DashboardRepository> logger)
{
    /// <summary>Returns the Dashboard read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<DashboardReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Dashboard {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("DashboardId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DashboardReadModel>(
            conn, $"[Analytics].[usp_Dashboard_GetById]", parameters);
    }

    /// <summary>Returns all Dashboard read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<DashboardReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Dashboard tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<DashboardReadModel>(
            conn, $"[Analytics].[usp_Dashboard_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
