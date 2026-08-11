// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture.Behavior
//  Layer:          Infrastructure
//  File:           BehaviorDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Behavior read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Behavior.Models;
using PlantSafety.Application.Analytics.Culture.Behavior.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Culture.Behavior.Persistence;

/// <summary>Dapper-backed repository for <see cref="BehaviorItemDto"/> queries.</summary>
public sealed class BehaviorDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<BehaviorDapperRepository> logger)
    : IBehaviorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BehaviorItemDto>> GetBehaviorObservationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetBehaviorObservationsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<BehaviorItemDto>(
            "[Analytics].[usp_Behavior_List]", parameters, cancellationToken);
    }
}
