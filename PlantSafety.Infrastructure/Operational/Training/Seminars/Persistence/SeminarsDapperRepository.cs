// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Seminars
//  Layer:          Infrastructure
//  File:           SeminarsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Seminars read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Training.Seminars.Models;
using PlantSafety.Application.Operational.Training.Seminars.Repositories;

namespace PlantSafety.Infrastructure.Operational.Training.Seminars.Persistence;

/// <summary>Dapper-backed repository for <see cref="SeminarItemDto"/> queries.</summary>
public sealed class SeminarsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SeminarsDapperRepository> logger)
    : ISeminarsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SeminarItemDto>> GetSeminarsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSeminarsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SeminarItemDto>(
            "[Operational].[usp_Seminars_List]", parameters, cancellationToken);
    }
}
