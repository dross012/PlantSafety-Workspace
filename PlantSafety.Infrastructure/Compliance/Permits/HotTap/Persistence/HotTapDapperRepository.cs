// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.HotTap
//  Layer:          Infrastructure
//  File:           HotTapDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.HotTap read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.HotTap.Models;
using PlantSafety.Application.Compliance.Permits.HotTap.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.HotTap.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.HotTap read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class HotTapDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HotTapDapperRepository> logger)
    : IHotTapRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HotTapItemDto>> GetHotTapAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHotTap for Permits.HotTap tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HotTapItemDto>(
            "[Compliance].[usp_PermitHotTap_GetHotTap]", parameters, cancellationToken);
    }
}
