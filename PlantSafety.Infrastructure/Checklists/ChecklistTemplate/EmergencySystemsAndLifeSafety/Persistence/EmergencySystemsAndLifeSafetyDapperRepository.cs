// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety
//  Layer:          Infrastructure
//  File:           EmergencySystemsAndLifeSafetyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencySystemsAndLifeSafety read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Models;
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Repositories;

namespace PlantSafety.Infrastructure.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Persistence;

/// <summary>Dapper-backed repository for EmergencySystemsAndLifeSafety read-model queries.</summary>
public sealed class EmergencySystemsAndLifeSafetyDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencySystemsAndLifeSafetyDapperRepository> logger)
    : IEmergencySystemsAndLifeSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencySystemsAndLifeSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for EmergencySystemsAndLifeSafety tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencySystemsAndLifeSafetyItemDto>(
            "[Checklists].[usp_EmergencySystemsAndLifeSafety_GetItems]", parameters, cancellationToken);
    }
}
