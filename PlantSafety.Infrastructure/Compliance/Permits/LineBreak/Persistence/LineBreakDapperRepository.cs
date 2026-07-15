// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.LineBreak
//  Layer:          Infrastructure
//  File:           LineBreakDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.LineBreak read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.LineBreak.Models;
using PlantSafety.Application.Compliance.Permits.LineBreak.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.LineBreak.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.LineBreak read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class LineBreakDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<LineBreakDapperRepository> logger)
    : ILineBreakRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LineBreakItemDto>> GetLineBreakAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLineBreak for Permits.LineBreak tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LineBreakItemDto>(
            "[Compliance].[usp_PermitLineBreak_GetLineBreak]", parameters, cancellationToken);
    }
}
