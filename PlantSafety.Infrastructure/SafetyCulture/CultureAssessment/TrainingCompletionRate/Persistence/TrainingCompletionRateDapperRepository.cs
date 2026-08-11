// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.TrainingCompletionRate
//  Layer:          Infrastructure
//  File:           TrainingCompletionRateDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TrainingCompletionRate read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.TrainingCompletionRate.Persistence;

/// <summary>Dapper-backed repository for TrainingCompletionRate read-model queries.</summary>
public sealed class TrainingCompletionRateDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<TrainingCompletionRateDapperRepository> logger)
    : ITrainingCompletionRateRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrainingCompletionRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for TrainingCompletionRate tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrainingCompletionRateItemDto>(
            "[SafetyCulture].[usp_TrainingCompletionRate_GetItems]", parameters, cancellationToken);
    }
}
