// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Infrastructure
//  File:           PreventiveMaintenanceDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PreventiveMaintenance read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

namespace PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for <see cref="PreventiveMaintenanceItemDto"/> queries.</summary>
public sealed class PreventiveMaintenanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PreventiveMaintenanceDapperRepository> logger)
    : IPreventiveMaintenanceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PreventiveMaintenanceItemDto>> GetPreventiveMaintenanceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPreventiveMaintenanceAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PreventiveMaintenanceItemDto>(
            "[Operational].[usp_PreventiveMaintenance_List]", parameters, cancellationToken);
    }
}
