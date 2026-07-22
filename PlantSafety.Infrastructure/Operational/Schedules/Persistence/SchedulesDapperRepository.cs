// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Infrastructure
//  File:           SchedulesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Schedules read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Schedules.Models;
using PlantSafety.Application.Operational.Schedules.Repositories;

namespace PlantSafety.Infrastructure.Operational.Schedules.Persistence;

/// <summary>Dapper-backed repository for <see cref="SchedulesItemDto"/> queries.</summary>
public sealed class SchedulesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SchedulesDapperRepository> logger)
    : ISchedulesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SchedulesItemDto>> GetSchedulesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSchedulesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SchedulesItemDto>(
            "[Operational].[usp_Schedules_List]", parameters, cancellationToken);
    }
}
