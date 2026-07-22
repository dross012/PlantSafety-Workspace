// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.SafetySuggestionSubmissions
//  Layer:          Infrastructure
//  File:           SafetySuggestionSubmissionsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetySuggestionSubmissions read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.SafetySuggestionSubmissions.Persistence;

/// <summary>Dapper-backed repository for SafetySuggestionSubmissions read-model queries.</summary>
public sealed class SafetySuggestionSubmissionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SafetySuggestionSubmissionsDapperRepository> logger)
    : ISafetySuggestionSubmissionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetySuggestionSubmissionsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for SafetySuggestionSubmissions tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SafetySuggestionSubmissionsItemDto>(
            "[SafetyCulture].[usp_SafetySuggestionSubmissions_GetItems]", parameters, cancellationToken);
    }
}
