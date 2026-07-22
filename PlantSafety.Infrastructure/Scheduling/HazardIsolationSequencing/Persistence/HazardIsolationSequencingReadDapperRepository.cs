// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Infrastructure
//  File:           HazardIsolationSequencingReadDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardIsolationSequencing Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;
using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Repositories;

namespace PlantSafety.Infrastructure.Scheduling.HazardIsolationSequencing.Persistence;

/// <summary>Dapper-backed repository for HazardIsolationSequencing read-model queries.</summary>
public sealed class HazardIsolationSequencingReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<HazardIsolationSequencingReadDapperRepository> logger)
    : IHazardIsolationSequencingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazardIsolationSequencingItemDto>> GetHazardIsolationSequencingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRange for HazardIsolationSequencing tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardIsolationSequencingItemDto>(
            "[Scheduling].[usp_HazardIsolationSequencing_GetRange]", parameters, cancellationToken);
    }
}
