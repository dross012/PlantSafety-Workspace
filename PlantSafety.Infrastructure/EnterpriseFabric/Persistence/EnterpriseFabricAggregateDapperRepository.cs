// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEnterpriseFabricRepository (Domain repository
//                  interface for the EnterpriseFabric aggregate root). All DB access via stored
//                  procedures in the [EnterpriseFabric] schema, against
//                  [EnterpriseFabric].[EnterpriseFabricAggregate] (named to avoid ambiguity with
//                  the pre-existing, unrelated [EnterpriseFabric].[FabricNode] table). Named
//                  "EnterpriseFabricAggregateDapperRepository" (not "EnterpriseFabricRepository")
//                  because the aggregate class itself is named "EnterpriseFabric", identical to
//                  its own module/namespace segment -- fully-qualified references are used
//                  throughout to resolve the ambiguity, mirroring
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence
//                  .AuditingAggregateDapperRepository's identical precedent. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because EnterpriseFabric
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.EnterpriseFabric.Enums;
using PlantSafety.Domain.EnterpriseFabric.Events;
using PlantSafety.Domain.EnterpriseFabric.Repositories;
using PlantSafety.Domain.EnterpriseFabric.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.EnterpriseFabric.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric"/> aggregate root.</summary>
public sealed class EnterpriseFabricAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EnterpriseFabricAggregateDapperRepository> logger)
    : IEnterpriseFabricRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric?> GetByIdAsync(EnterpriseFabricId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EnterpriseFabric {EnterpriseFabricId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnterpriseFabricId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EnterpriseFabricRow>(
            "[EnterpriseFabric].[usp_EnterpriseFabricAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add EnterpriseFabric {EnterpriseFabricId}", aggregate.EnterpriseFabricId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnterpriseFabricId", aggregate.EnterpriseFabricId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FabricArea", aggregate.FabricArea, 200)
            .AddString("SystemReference", aggregate.SystemReference, 200)
            .AddString("IntegrationType", aggregate.IntegrationType, 100)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[EnterpriseFabric].[usp_EnterpriseFabricAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update EnterpriseFabric {EnterpriseFabricId}", aggregate.EnterpriseFabricId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnterpriseFabricId", aggregate.EnterpriseFabricId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FabricArea", aggregate.FabricArea, 200)
            .AddString("SystemReference", aggregate.SystemReference, 200)
            .AddString("IntegrationType", aggregate.IntegrationType, 100)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_EnterpriseFabricAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[EnterpriseFabric].[usp_EnterpriseFabricAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"EnterpriseFabric {aggregate.EnterpriseFabricId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric Hydrate(EnterpriseFabricRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.EnterpriseFabricId), EnterpriseFabricId.FromGuid(row.EnterpriseFabricId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.Status), Enum.Parse<EnterpriseFabricStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.FabricArea), row.FabricArea);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.SystemReference), row.SystemReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.IntegrationType), row.IntegrationType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEnterpriseFabricDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EnterpriseFabricAggregate_GetById</c>.</summary>
    private sealed record EnterpriseFabricRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_EnterpriseFabricAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid EnterpriseFabricId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string FabricArea { get; init; }
        public required string SystemReference { get; init; }
        public required string IntegrationType { get; init; }
        public required Guid OwnerUserId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
