// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.Observation
//  Layer:          Infrastructure
//  File:           ObservationDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Observation aggregate. All DB access via
//                  stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;
using ObservationReadModel = PlantSafety.Application.Features.SafetyEvents.Handlers.Observation.ObservationReadModel;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.Observation.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>Observation</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ObservationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<ObservationDapperRepository> logger)
    : IObservationRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid observationId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string observationType,
        DateTimeOffset observedAtUtc,
        Guid observedByWorkerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Observation {ObservationId} for tenant {TenantId}", observationId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ObservationId", observationId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddString("ObservationType", observationType)
            .AddDateTime("ObservedAtUtc", observedAtUtc.UtcDateTime)
            .AddGuid("ObservedByWorkerId", observedByWorkerId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Observation_Create]", parameters, tx);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        Guid tenantId,
        Guid observationId,
        string? title,
        string? description,
        string? observationType,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Update Observation {ObservationId} for tenant {TenantId}", observationId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ObservationId", observationId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddString("ObservationType", observationType)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Observation_Update]", parameters, tx);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Observation {observationId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<ObservationReadModel?> GetByIdAsync(Guid tenantId, Guid observationId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Observation {ObservationId} for tenant {TenantId}", observationId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ObservationId", observationId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ObservationReadModel>(
            "[SafetyEvents].[usp_Observation_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ObservationReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? observationType,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Observations for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ObservationType", observationType)
            .AddDateTime("FromUtc", fromUtc?.UtcDateTime)
            .AddDateTime("ToUtc", toUtc?.UtcDateTime)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<ObservationReadModel>(
            "[SafetyEvents].[usp_Observation_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("ObservationType", observationType)
            .AddDateTime("FromUtc", fromUtc?.UtcDateTime)
            .AddDateTime("ToUtc", toUtc?.UtcDateTime)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_Observation_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
