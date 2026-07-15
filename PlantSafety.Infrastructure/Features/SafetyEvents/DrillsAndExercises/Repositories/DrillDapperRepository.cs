// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Infrastructure
//  File:           DrillDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Drill aggregate. All DB access via
//                  stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.DrillsAndExercises.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>Drill</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DrillDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<DrillDapperRepository> logger)
    : IDrillRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid drillId,
        Guid tenantId,
        Guid siteId,
        string title,
        string drillType,
        DateTimeOffset scheduledAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Drill {DrillId} for tenant {TenantId}", drillId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("DrillId", drillId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title)
            .AddString("DrillType", drillType)
            .AddDateTime("ScheduledAtUtc", scheduledAtUtc.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Drill_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<DrillReadModel?> GetByIdAsync(Guid tenantId, Guid drillId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Drill {DrillId} for tenant {TenantId}", drillId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("DrillId", drillId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DrillReadModel>(
            "[SafetyEvents].[usp_Drill_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<DrillReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? drillType,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Drills for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("DrillType", drillType, size: 64)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<DrillReadModel>(
            "[SafetyEvents].[usp_Drill_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("DrillType", drillType, size: 64)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_Drill_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
