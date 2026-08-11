// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.MachineGuardingAndLoto
//  Layer:          Infrastructure
//  File:           MachineGuardingAndLotoDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MachineGuardingAndLoto read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Persistence;

/// <summary>Dapper-backed repository for MachineGuardingAndLoto read-model queries.</summary>
public sealed class MachineGuardingAndLotoDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<MachineGuardingAndLotoDapperRepository> logger)
    : IMachineGuardingAndLotoRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<MachineGuardingAndLotoItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for MachineGuardingAndLoto tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachineGuardingAndLotoItemDto>(
            "[Checklists].[usp_MachineGuardingAndLoto_GetItems]", parameters, cancellationToken);
    }
}
