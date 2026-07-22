// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.SafetyMeetingParticipation
//  Layer:          Infrastructure
//  File:           SafetyMeetingParticipationDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyMeetingParticipation read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Persistence;

/// <summary>Dapper-backed repository for SafetyMeetingParticipation read-model queries.</summary>
public sealed class SafetyMeetingParticipationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SafetyMeetingParticipationDapperRepository> logger)
    : ISafetyMeetingParticipationRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyMeetingParticipationItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for SafetyMeetingParticipation tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SafetyMeetingParticipationItemDto>(
            "[SafetyCulture].[usp_SafetyMeetingParticipation_GetItems]", parameters, cancellationToken);
    }
}
