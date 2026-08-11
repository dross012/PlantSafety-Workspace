// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Predictive.IncidentPrediction
//  Layer:          Infrastructure
//  File:           IncidentPredictionDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IncidentPrediction read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Models;
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Predictive.IncidentPrediction.Persistence;

/// <summary>Dapper-backed repository for <see cref="IncidentPredictionItemDto"/> queries.</summary>
public sealed class IncidentPredictionDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentPredictionDapperRepository> logger)
    : IIncidentPredictionRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentPredictionItemDto>> GetIncidentPredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentPredictionsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IncidentPredictionItemDto>(
            "[Analytics].[usp_IncidentPrediction_List]", parameters, cancellationToken);
    }
}
