// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Infrastructure
//  File:           ShiftFatigueReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ShiftFatigue Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.ShiftFatigue.Models;
using PlantSafety.Application.Scheduling.ShiftFatigue.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.ShiftFatigue.Persistence;

/// <summary>Dapper-backed repository for ShiftFatigue read-model queries.</summary>
public sealed class ShiftFatigueReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ShiftFatigueReadDapperRepository> logger)
    : IShiftFatigueRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShiftFatigueItemDto>> GetShiftFatigueAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for ShiftFatigue tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ShiftFatigueItemDto>(
            "[Scheduling].[usp_ShiftFatigue_GetRange]", parameters, cancellationToken);
    }
}
