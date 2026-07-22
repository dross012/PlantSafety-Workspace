// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.LeadershipEngagementFrequency
//  Layer:          Infrastructure
//  File:           LeadershipEngagementFrequencyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LeadershipEngagementFrequency read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Persistence;

/// <summary>Dapper-backed repository for LeadershipEngagementFrequency read-model queries.</summary>
public sealed class LeadershipEngagementFrequencyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<LeadershipEngagementFrequencyDapperRepository> logger)
    : ILeadershipEngagementFrequencyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<LeadershipEngagementFrequencyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for LeadershipEngagementFrequency tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LeadershipEngagementFrequencyItemDto>(
            "[SafetyCulture].[usp_LeadershipEngagementFrequency_GetItems]", parameters, cancellationToken);
    }
}
