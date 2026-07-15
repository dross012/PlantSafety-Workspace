// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Tracking
//  Layer:          Infrastructure
//  File:           TrackingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Tracking read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Tracking.Models;
using PlantSafety.Application.Compliance.Tracking.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Tracking.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrackingItemDto"/> queries.</summary>
public sealed class TrackingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<TrackingDapperRepository> logger)
    : ITrackingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrackingItemDto>> GetTrackingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrackingAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrackingItemDto>(
            "[Compliance].[usp_Tracking_List]", parameters, cancellationToken);
    }
}