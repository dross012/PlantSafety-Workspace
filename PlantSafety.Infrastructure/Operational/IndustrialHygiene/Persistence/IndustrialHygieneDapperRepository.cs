// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.IndustrialHygiene
//  Layer:          Infrastructure
//  File:           IndustrialHygieneDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IndustrialHygiene read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.IndustrialHygiene.Models;
using PlantSafety.Application.Operational.IndustrialHygiene.Repositories;

namespace PlantSafety.Infrastructure.Operational.IndustrialHygiene.Persistence;

/// <summary>Dapper-backed repository for <see cref="IndustrialHygieneItemDto"/> queries.</summary>
public sealed class IndustrialHygieneDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<IndustrialHygieneDapperRepository> logger)
    : IIndustrialHygieneRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IndustrialHygieneItemDto>> GetIndustrialHygieneAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIndustrialHygieneAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IndustrialHygieneItemDto>(
            "[Operational].[usp_IndustrialHygiene_List]", parameters, cancellationToken);
    }
}
