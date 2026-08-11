// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentTimelineReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper read-side repository for Incident timeline entries. All DB access via
//                  stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;

/// <summary>
/// Dapper-backed read-side repository for <c>IncidentTimelineEntry</c> rows.
/// Executes read-optimised stored procedures against the read-replica connection.
/// </summary>
public sealed class IncidentTimelineReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentTimelineReadDapperRepository> logger)
    : IIncidentTimelineReadRepository
{
    /// <inheritdoc/>
    public async Task<(IReadOnlyList<IncidentTimelineEntryReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid incidentId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List timeline entries for Incident {IncidentId} tenant {TenantId}", incidentId, tenantId);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<IncidentTimelineEntryReadModel>(
            "[SafetyEvents].[usp_IncidentTimelineEntry_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_IncidentTimelineEntry_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
