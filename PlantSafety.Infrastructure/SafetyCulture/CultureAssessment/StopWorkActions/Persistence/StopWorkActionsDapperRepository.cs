// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.StopWorkActions
//  Layer:          Infrastructure
//  File:           StopWorkActionsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the StopWorkActions read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.StopWorkActions.Persistence;

/// <summary>Dapper-backed repository for StopWorkActions read-model queries.</summary>
public sealed class StopWorkActionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<StopWorkActionsDapperRepository> logger)
    : IStopWorkActionsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<StopWorkActionsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for StopWorkActions tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<StopWorkActionsItemDto>(
            "[SafetyCulture].[usp_StopWorkActions_GetItems]", parameters, cancellationToken);
    }
}
