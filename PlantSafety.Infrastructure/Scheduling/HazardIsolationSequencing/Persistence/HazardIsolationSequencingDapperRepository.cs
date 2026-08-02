// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Infrastructure
//  File:           HazardIsolationSequencingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IHazardIsolationSequencingRepository (Domain
//                  repository interface for the HazardIsolationSequencing aggregate root). All DB
//                  access via stored procedures in the [Scheduling] schema. Aggregate reconstruction
//                  uses AggregateReflectionHydrator because HazardIsolationSequencing exposes only a
//                  private 'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the full active
//                  candidate set, since the interface carries no tenant/site scoping parameter to
//                  push down to SQL.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync is now tenant-scoped and
//                  UpdateAsync now takes an optimistic-concurrency ETag and enforces it via
//                  ExecuteScalarAsync + @@ROWCOUNT (usp_HazardIsolationSequencing_Update.sql),
//                  throwing ConcurrencyConflictException on zero rows affected. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write (already
//                  correct pre-existing behavior; verified, not changed). Mirrors
//                  PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Enums;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Events;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Specifications;
using PlantSafety.Domain.Scheduling.HazardIsolationSequencing.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.HazardIsolationSequencing.Persistence;

/// <summary>Dapper-backed repository for the <see cref="HazardIsolationSequencing"/> aggregate root.</summary>
public sealed class HazardIsolationSequencingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazardIsolationSequencingDapperRepository> logger)
    : IHazardIsolationSequencingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing?> GetByIdAsync(HazardIsolationSequencingId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazardIsolationSequencing {HazardIsolationSequencingId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazardIsolationSequencingId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardIsolationSequencingRow>(
            "[Scheduling].[usp_HazardIsolationSequencing_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing>> ListAsync(HazardIsolationSequencingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List HazardIsolationSequencing via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<HazardIsolationSequencingRow>(
            "[Scheduling].[usp_HazardIsolationSequencing_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add HazardIsolationSequencing {HazardIsolationSequencingId}", aggregate.HazardIsolationSequencingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazardIsolationSequencingId", aggregate.HazardIsolationSequencingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("HazardZone", aggregate.HazardZone, 400)
            .AddString("TaskType", aggregate.TaskType, 400)
            .AddDateTime("WindowStartUtc", aggregate.WindowStartUtc?.Value)
            .AddDateTime("WindowEndUtc", aggregate.WindowEndUtc?.Value)
            .AddBool("ConflictDetected", aggregate.ConflictDetected)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_HazardIsolationSequencing_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update HazardIsolationSequencing {HazardIsolationSequencingId}", aggregate.HazardIsolationSequencingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazardIsolationSequencingId", aggregate.HazardIsolationSequencingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("HazardZone", aggregate.HazardZone, 400)
            .AddString("TaskType", aggregate.TaskType, 400)
            .AddDateTime("WindowStartUtc", aggregate.WindowStartUtc?.Value)
            .AddDateTime("WindowEndUtc", aggregate.WindowEndUtc?.Value)
            .AddBool("ConflictDetected", aggregate.ConflictDetected)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_HazardIsolationSequencing_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Scheduling].[usp_HazardIsolationSequencing_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"HazardIsolationSequencing {aggregate.HazardIsolationSequencingId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="HazardIsolationSequencing"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing Hydrate(HazardIsolationSequencingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing>();

        AggregateReflectionHydrator.SetProperty(agg, "HazardIsolationSequencingId", HazardIsolationSequencingId.FromGuid(row.HazardIsolationSequencingId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<HazardIsolationSequencingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "HazardZone", row.HazardZone);
        AggregateReflectionHydrator.SetProperty(agg, "TaskType", row.TaskType);
        AggregateReflectionHydrator.SetProperty(agg, "WindowStartUtc", row.WindowStartUtc.HasValue ? UtcTimestamp.From(row.WindowStartUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "WindowEndUtc", row.WindowEndUtc.HasValue ? UtcTimestamp.From(row.WindowEndUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ConflictDetected", row.ConflictDetected);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IHazardIsolationSequencingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_HazardIsolationSequencing_GetById</c>/<c>usp_HazardIsolationSequencing_GetAllActive</c>.</summary>
    private sealed record HazardIsolationSequencingRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated by
        /// <c>usp_HazardIsolationSequencing_GetById</c>. Not consumed by aggregate hydration --
        /// callers obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer HazardIsolationSequencingItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid HazardIsolationSequencingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string HazardZone { get; init; }
        public required string TaskType { get; init; }
        public DateTime? WindowStartUtc { get; init; }
        public DateTime? WindowEndUtc { get; init; }
        public required bool ConflictDetected { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
