// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.BehavioralObservationCompletion
//  Layer:          Infrastructure
//  File:           BehavioralObservationCompletionDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the BehavioralObservationCompletion read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Persistence;

/// <summary>Dapper-backed repository for BehavioralObservationCompletion read-model queries.</summary>
public sealed class BehavioralObservationCompletionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<BehavioralObservationCompletionDapperRepository> logger)
    : IBehavioralObservationCompletionRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BehavioralObservationCompletionItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for BehavioralObservationCompletion tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<BehavioralObservationCompletionItemDto>(
            "[SafetyCulture].[usp_BehavioralObservationCompletion_GetItems]", parameters, cancellationToken);
    }
}
