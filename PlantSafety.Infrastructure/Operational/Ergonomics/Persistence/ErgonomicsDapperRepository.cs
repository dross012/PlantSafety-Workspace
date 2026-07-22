// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Ergonomics
//  Layer:          Infrastructure
//  File:           ErgonomicsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Ergonomics read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Ergonomics.Models;
using PlantSafety.Application.Operational.Ergonomics.Repositories;

namespace PlantSafety.Infrastructure.Operational.Ergonomics.Persistence;

/// <summary>Dapper-backed repository for <see cref="ErgonomicsItemDto"/> queries.</summary>
public sealed class ErgonomicsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ErgonomicsDapperRepository> logger)
    : IErgonomicsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ErgonomicsItemDto>> GetErgonomicsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetErgonomicsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ErgonomicsItemDto>(
            "[Operational].[usp_Ergonomics_List]", parameters, cancellationToken);
    }
}
