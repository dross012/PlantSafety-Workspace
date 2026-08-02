// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Infrastructure
//  File:           PreventiveMaintenanceTaskReadDapperRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PreventiveMaintenanceTask Application-layer by-id
//                  read-model. Backs GetPreventiveMaintenanceTaskByIdQuery. Reuses
//                  usp_PreventiveMaintenanceTask_GetById (the same proc
//                  PreventiveMaintenanceTaskDapperRepository.GetByIdAsync uses to hydrate the
//                  write-side aggregate) rather than introducing a duplicate stored procedure.
//                  Distinct from the pre-existing PreventiveMaintenanceDapperRepository in this
//                  same module (which implements the date-range list query
//                  IPreventiveMaintenanceRepository and is left untouched). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

namespace PlantSafety.Infrastructure.Operational.PreventiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for PreventiveMaintenanceTask by-id read-model queries.</summary>
public sealed class PreventiveMaintenanceTaskReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PreventiveMaintenanceTaskReadDapperRepository> logger)
    : IPreventiveMaintenanceTaskRepository
{
    /// <inheritdoc/>
    public async Task<PreventiveMaintenanceTaskItemDto?> GetByIdAsync(Guid preventiveMaintenanceTaskId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for PreventiveMaintenanceTask {PreventiveMaintenanceTaskId} Tenant {TenantId}", preventiveMaintenanceTaskId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PreventiveMaintenanceTaskId", preventiveMaintenanceTaskId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PreventiveMaintenanceTaskItemDto>(
            "[Operational].[usp_PreventiveMaintenanceTask_GetById]", parameters, cancellationToken);
    }
}
