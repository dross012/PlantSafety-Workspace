// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Schedules
//  Layer:          Infrastructure
//  File:           SchedulesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Schedules read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Schedules.Models;
using PlantSafety.Application.Analytics.Schedules.Repositories;

namespace PlantSafety.Infrastructure.Analytics.Schedules.Persistence;

/// <summary>Dapper-backed repository for <see cref="SchedulesItemDto"/> queries.</summary>
public sealed class SchedulesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SchedulesDapperRepository> logger)
    : ISchedulesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SchedulesItemDto>> GetScheduleItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetScheduleItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SchedulesItemDto>(
            "[Analytics].[usp_Schedules_List]", parameters, cancellationToken);
    }
}
