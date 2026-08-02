// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PredictiveMaintenance read-model. All DB access via
//                  stored procedures in the [Operational] schema.
//                  Phase 4 full build-out (2026-07-30): additionally implements the new
//                  Application-layer by-id read interface
//                  (PlantSafety.Application.Operational.PredictiveMaintenance.Repositories
//                  .IPredictiveMaintenanceAlertRepository), backing
//                  GetPredictiveMaintenanceAlertByIdQuery via usp_PredictiveMaintenanceAlert_GetById.
//                  The pre-existing date-range list method (GetPredictiveMaintenanceAsync, backing
//                  IPredictiveMaintenanceRepository/GetPredictiveMaintenanceQuery) is left
//                  completely untouched. Distinct from
//                  PredictiveMaintenanceAlertDapperRepository.cs (the new write-side aggregate
//                  repository implementing the Domain-layer IPredictiveMaintenanceAlertRepository).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for <see cref="PredictiveMaintenanceItemDto"/> queries.</summary>
public sealed class PredictiveMaintenanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveMaintenanceDapperRepository> logger)
    : IPredictiveMaintenanceRepository,
      PlantSafety.Application.Operational.PredictiveMaintenance.Repositories.IPredictiveMaintenanceAlertRepository
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

    /// <inheritdoc/>
    public async Task<PredictiveMaintenanceAlertItemDto?> GetByIdAsync(Guid predictiveMaintenanceAlertId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId} Tenant {TenantId}", predictiveMaintenanceAlertId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveMaintenanceAlertId", predictiveMaintenanceAlertId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PredictiveMaintenanceAlertItemDto>(
            "[Operational].[usp_PredictiveMaintenanceAlert_GetById]", parameters, cancellationToken);
    }
}
