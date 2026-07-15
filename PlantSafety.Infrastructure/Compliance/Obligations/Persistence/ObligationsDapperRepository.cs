// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Obligations
//  Layer:          Infrastructure
//  File:           ObligationsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Obligations read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Obligations.Models;
using PlantSafety.Application.Compliance.Obligations.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Obligations.Persistence;

/// <summary>
/// Dapper-backed repository for Obligations read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ObligationsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ObligationsDapperRepository> logger)
    : IObligationsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ObligationsItemDto>> GetObligationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetObligations for Obligations tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ObligationsItemDto>(
            "[Compliance].[usp_Obligations_GetObligations]", parameters, cancellationToken);
    }
}
