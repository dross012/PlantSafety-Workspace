// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IDigitalTwinRepository (Domain repository
//                  interface for the DigitalTwin aggregate root). All DB access via stored
//                  procedures in the [DigitalTwin] schema, against
//                  [DigitalTwin].[DigitalTwinAggregate] (named to avoid ambiguity with the
//                  pre-existing, unrelated Features.DigitalTwin asset-registry table
//                  [DigitalTwin].[Twin] that lives under the same [DigitalTwin] schema and is
//                  live-wired into DI). Aggregate reconstruction uses AggregateReflectionHydrator
//                  because DigitalTwin exposes only a private constructor and private-setter
//                  properties. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.DigitalTwin.Aggregates;
using PlantSafety.Domain.DigitalTwin.Enums;
using PlantSafety.Domain.DigitalTwin.Events;
using PlantSafety.Domain.DigitalTwin.Repositories;
using PlantSafety.Domain.DigitalTwin.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.DigitalTwin.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin"/> aggregate root.</summary>
public sealed class DigitalTwinAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DigitalTwinAggregateDapperRepository> logger)
    : IDigitalTwinRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin?> GetByIdAsync(DigitalTwinId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for DigitalTwin {DigitalTwinId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DigitalTwinId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DigitalTwinRow>(
            "[DigitalTwin].[usp_DigitalTwinAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add DigitalTwin {DigitalTwinId}", aggregate.DigitalTwinId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DigitalTwinId", aggregate.DigitalTwinId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("TwinName", aggregate.TwinName, 200)
            .AddGuid("AssetReferenceId", aggregate.AssetReferenceId)
            .AddString("ModelType", aggregate.ModelType, 100)
            .AddDateTime("LastSyncedAtUtc", aggregate.LastSyncedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[DigitalTwin].[usp_DigitalTwinAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update DigitalTwin {DigitalTwinId}", aggregate.DigitalTwinId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DigitalTwinId", aggregate.DigitalTwinId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("TwinName", aggregate.TwinName, 200)
            .AddGuid("AssetReferenceId", aggregate.AssetReferenceId)
            .AddString("ModelType", aggregate.ModelType, 100)
            .AddDateTime("LastSyncedAtUtc", aggregate.LastSyncedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_DigitalTwinAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[DigitalTwin].[usp_DigitalTwinAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"DigitalTwin {aggregate.DigitalTwinId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin Hydrate(DigitalTwinRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.DigitalTwinId), DigitalTwinId.FromGuid(row.DigitalTwinId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.Status), Enum.Parse<DigitalTwinStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.TwinName), row.TwinName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.AssetReferenceId), row.AssetReferenceId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.ModelType), row.ModelType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.LastSyncedAtUtc), row.LastSyncedAtUtc.HasValue ? UtcTimestamp.From(row.LastSyncedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IDigitalTwinDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_DigitalTwinAggregate_GetById</c>.</summary>
    private sealed record DigitalTwinRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_DigitalTwinAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid DigitalTwinId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string TwinName { get; init; }
        public Guid? AssetReferenceId { get; init; }
        public required string ModelType { get; init; }
        public DateTime? LastSyncedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
