// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.DeadlinesEPA
//  Layer:          Infrastructure
//  File:           EPADapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DeadlinesEPA read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Deadlines.EPA.Persistence;

/// <summary>Dapper-backed repository for <see cref="EPAItemDto"/> queries.</summary>
public sealed class EPADapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EPADapperRepository> logger)
    : IEPARepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EPAItemDto>> GetDeadlinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDeadlinesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EPAItemDto>(
            "[Compliance].[usp_DeadlinesEPA_List]", parameters, cancellationToken);
    }
}