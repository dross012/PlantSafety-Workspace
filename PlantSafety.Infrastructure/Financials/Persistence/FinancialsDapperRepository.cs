// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials.Financials
//  Layer:          Infrastructure
//  File:           FinancialsDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Financials read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Financials.Financials.Models;
using PlantSafety.Application.Financials.Financials.Repositories;

namespace PlantSafety.Infrastructure.Financials.Persistence;

/// <summary>
/// Dapper-backed repository for Financials read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class FinancialsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<FinancialsDapperRepository> logger)
    : IFinancialsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FinancialsItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRecordsAsync for Financials tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<FinancialsItemDto>(
            "[Financials].[usp_Financials_GetRecords]", parameters, cancellationToken);
    }
}
