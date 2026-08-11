// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard.KPISnapshots
//  Layer:          Infrastructure
//  File:           KPISnapshotsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the KPISnapshots read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Models;
using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Dashboard.KPISnapshots.Persistence;

/// <summary>Dapper-backed repository for <see cref="KPISnapshotsItemDto"/> queries.</summary>
public sealed class KPISnapshotsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<KPISnapshotsDapperRepository> logger)
    : IKPISnapshotsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<KPISnapshotsItemDto>> GetKpiSnapshotsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetKpiSnapshotsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<KPISnapshotsItemDto>(
            "[Analytics].[usp_KPISnapshots_List]", parameters, cancellationToken);
    }
}
