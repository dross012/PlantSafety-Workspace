// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Infrastructure
//  File:           PredictiveSafetyDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PredictiveSafety read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;
using PlantSafety.Application.Intelligence.PredictiveSafety.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.PredictiveSafety.Persistence;

/// <summary>
/// Dapper-backed repository for PredictiveSafety read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class PredictiveSafetyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveSafetyDapperRepository> logger)
    : IPredictiveSafetyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PredictiveSafetyItemDto>> GetPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPredictionsAsync for PredictiveSafety tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PredictiveSafetyItemDto>(
            "[Intelligence].[usp_PredictiveSafety_GetPredictions]", parameters, cancellationToken);
    }
}
