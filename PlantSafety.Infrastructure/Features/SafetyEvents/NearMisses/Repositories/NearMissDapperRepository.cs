// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.NearMisses
//  Layer:          Infrastructure
//  File:           NearMissDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the NearMiss aggregate. All DB access via
//                  stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.NearMisses.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>NearMiss</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class NearMissDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<NearMissDapperRepository> logger)
    : INearMissRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid nearMissId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        DateTimeOffset occurredAtUtc,
        string potentialSeverity,
        string hazardCategory,
        Guid reportedByWorkerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create NearMiss {NearMissId} for tenant {TenantId}", nearMissId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("NearMissId", nearMissId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddDateTime("OccurredAtUtc", occurredAtUtc.UtcDateTime)
            .AddString("PotentialSeverity", potentialSeverity)
            .AddString("HazardCategory", hazardCategory)
            .AddGuid("ReportedByWorkerId", reportedByWorkerId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_NearMiss_Create]", parameters, tx);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        Guid tenantId,
        Guid nearMissId,
        string? title,
        string? description,
        string? potentialSeverity,
        string? hazardCategory,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Update NearMiss {NearMissId} for tenant {TenantId}", nearMissId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("NearMissId", nearMissId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddString("PotentialSeverity", potentialSeverity)
            .AddString("HazardCategory", hazardCategory)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_NearMiss_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_NearMiss_Update]", parameters, tx);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"NearMiss {nearMissId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<NearMissReadModel?> GetByIdAsync(Guid tenantId, Guid nearMissId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get NearMiss {NearMissId} for tenant {TenantId}", nearMissId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("NearMissId", nearMissId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<NearMissReadModel>(
            "[SafetyEvents].[usp_NearMiss_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<NearMissReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? potentialSeverity,
        string? hazardCategory,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List NearMisses for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("PotentialSeverity", potentialSeverity)
            .AddString("HazardCategory", hazardCategory)
            .AddDateTime("OccurredFromUtc", occurredFromUtc?.UtcDateTime)
            .AddDateTime("OccurredToUtc", occurredToUtc?.UtcDateTime)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<NearMissReadModel>(
            "[SafetyEvents].[usp_NearMiss_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("PotentialSeverity", potentialSeverity)
            .AddString("HazardCategory", hazardCategory)
            .AddDateTime("OccurredFromUtc", occurredFromUtc?.UtcDateTime)
            .AddDateTime("OccurredToUtc", occurredToUtc?.UtcDateTime)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_NearMiss_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
