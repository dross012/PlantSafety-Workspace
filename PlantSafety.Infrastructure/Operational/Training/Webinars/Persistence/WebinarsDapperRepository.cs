// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Webinars
//  Layer:          Infrastructure
//  File:           WebinarsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Webinars read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Training.Webinars.Models;
using PlantSafety.Application.Operational.Training.Webinars.Repositories;

namespace PlantSafety.Infrastructure.Operational.Training.Webinars.Persistence;

/// <summary>Dapper-backed repository for <see cref="WebinarItemDto"/> queries.</summary>
public sealed class WebinarsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<WebinarsDapperRepository> logger)
    : IWebinarsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<WebinarItemDto>> GetWebinarsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWebinarsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<WebinarItemDto>(
            "[Operational].[usp_Webinars_List]", parameters, cancellationToken);
    }
}
