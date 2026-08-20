// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Wellness
//  Layer:          Infrastructure
//  File:           WellnessDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Wellness read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Wellness.Models;
using PlantSafety.Application.Operational.Wellness.Repositories;

namespace PlantSafety.Infrastructure.Operational.Wellness.Persistence;

/// <summary>Dapper-backed repository for <see cref="WellnessItemDto"/> queries.</summary>
public sealed class WellnessDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<WellnessDapperRepository> logger)
    : IWellnessRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WellnessItemDto>> GetWellnessAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWellnessAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WellnessItemDto>(
            "[Operational].[usp_Wellness_List]", parameters, cancellationToken);
    }
}