// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Infrastructure
//  File:           CAPADapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CAPA read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.CAPA.Models;
using PlantSafety.Application.ContinuousImprovement.CAPA.Repositories;

namespace PlantSafety.Infrastructure.ContinuousImprovement.CAPA.Persistence;

/// <summary>
/// Dapper-backed repository for CAPA read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CAPADapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CAPADapperRepository> logger)
    : ICAPARepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CAPAItemDto>> GetActionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetActionsAsync for CAPA tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CAPAItemDto>(
            "[ContinuousImprovement].[usp_CAPA_GetActions]", parameters, cancellationToken);
    }
}
