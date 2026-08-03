// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RouteVersionSnapshotAggregateDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRouteVersionSnapshotRepository (Domain
//                  repository interface for the RouteVersionSnapshot aggregate root). All DB
//                  access via stored procedures against
//                  [ProcessManagement].[RouteVersionSnapshot]. Genuinely new persistence -- no
//                  analog exists for ProcessDefinition. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

/// <summary>Dapper-backed repository for the <see cref="RouteVersionSnapshot"/> aggregate root.</summary>
public sealed class RouteVersionSnapshotAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ILogger<RouteVersionSnapshotAggregateDapperRepository> logger)
    : IRouteVersionSnapshotRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(RouteVersionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add RouteVersionSnapshot {RouteVersionId} for RouteDefinition {RouteDefinitionId}", snapshot.RouteVersionId.Value, snapshot.RouteDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteVersionId", snapshot.RouteVersionId.Value)
            .AddGuid("RouteDefinitionId", snapshot.RouteDefinitionId.Value)
            .AddGuid("TenantId", snapshot.TenantId)
            .AddInt("VersionNumber", snapshot.VersionNumber)
            .AddString("Status", snapshot.Status.ToString(), 50)
            .AddDateTime("EffectiveDateUtc", snapshot.EffectiveDateUtc.Value)
            .AddString("StepsJson", RouteStepJsonSerializer.Serialize(snapshot.Steps), size: -1)
            .AddDateTime("CreatedAtUtc", snapshot.CreatedAtUtc.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ProcessManagement].[usp_RouteVersionSnapshot_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<RouteVersionSnapshot?> GetLatestForRouteAsync(RouteDefinitionId routeDefinitionId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetLatest RouteVersionSnapshot for RouteDefinition {RouteDefinitionId}", routeDefinitionId.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", routeDefinitionId.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RouteVersionSnapshotRow>(
            "[ProcessManagement].[usp_RouteVersionSnapshot_GetLatestForRoute]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(RouteVersionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateStatus RouteVersionSnapshot {RouteVersionId}", snapshot.RouteVersionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteVersionId", snapshot.RouteVersionId.Value)
            .AddGuid("TenantId", snapshot.TenantId)
            .AddString("Status", snapshot.Status.ToString(), 50)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ProcessManagement].[usp_RouteVersionSnapshot_UpdateStatus]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    private static RouteVersionSnapshot Hydrate(RouteVersionSnapshotRow row) => RouteVersionSnapshot.Rehydrate(
        RouteVersionId.FromGuid(row.RouteVersionId),
        RouteDefinitionId.FromGuid(row.RouteDefinitionId),
        row.TenantId,
        row.VersionNumber,
        Enum.Parse<RouteVersionStatus>(row.Status),
        UtcTimestamp.From(row.EffectiveDateUtc),
        UtcTimestamp.From(row.CreatedAtUtc),
        RouteStepJsonSerializer.Deserialize(row.StepsJson));

    /// <summary>Flat read-model row shape returned by the RouteVersionSnapshot stored procedures.</summary>
    private sealed record RouteVersionSnapshotRow
    {
        public required Guid RouteVersionId { get; init; }
        public required Guid RouteDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public required int VersionNumber { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveDateUtc { get; init; }
        public string? StepsJson { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }
}
