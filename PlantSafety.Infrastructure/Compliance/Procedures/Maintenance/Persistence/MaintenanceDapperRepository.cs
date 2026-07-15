// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresMaintenance
//  Layer:          Infrastructure
//  File:           MaintenanceDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresMaintenance read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.Maintenance.Models;
using PlantSafety.Application.Compliance.Procedures.Maintenance.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.Maintenance.Persistence;

/// <summary>Dapper-backed repository for <see cref="MaintenanceItemDto"/> queries.</summary>
public sealed class MaintenanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<MaintenanceDapperRepository> logger)
    : IMaintenanceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MaintenanceItemDto>> GetMaintenanceProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMaintenanceProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MaintenanceItemDto>(
            "[Compliance].[usp_ProceduresMaintenance_List]", parameters, cancellationToken);
    }
}