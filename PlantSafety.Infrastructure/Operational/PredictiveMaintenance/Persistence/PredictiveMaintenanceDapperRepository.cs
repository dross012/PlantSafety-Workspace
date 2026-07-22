// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PredictiveMaintenance read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for <see cref="PredictiveMaintenanceItemDto"/> queries.</summary>
public sealed class PredictiveMaintenanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveMaintenanceDapperRepository> logger)
    : IPredictiveMaintenanceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PredictiveMaintenanceItemDto>> GetPredictiveMaintenanceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPredictiveMaintenanceAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PredictiveMaintenanceItemDto>(
            "[Operational].[usp_PredictiveMaintenance_List]", parameters, cancellationToken);
    }
}
