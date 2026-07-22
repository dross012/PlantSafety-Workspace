// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Infrastructure
//  File:           PermitSchedulingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPermitSchedulingRepository (Domain repository
//                  interface for the PermitScheduling aggregate root). All DB access via stored
//                  procedures in the [PermitToWork] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PermitScheduling exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates;
using PlantSafety.Domain.PermitToWork.PermitScheduling.Enums;
using PlantSafety.Domain.PermitToWork.PermitScheduling.Events;
using PlantSafety.Domain.PermitToWork.PermitScheduling.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitScheduling.Specifications;
using PlantSafety.Domain.PermitToWork.PermitScheduling.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PermitToWork.PermitScheduling.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PermitScheduling"/> aggregate root.</summary>
public sealed class PermitSchedulingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitSchedulingDapperRepository> logger)
    : IPermitSchedulingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling?> GetByIdAsync(PermitSchedulingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PermitScheduling {PermitSchedulingId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PermitSchedulingId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitSchedulingRow>(
            "[PermitToWork].[usp_PermitScheduling_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling>> ListAsync(PermitSchedulingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PermitScheduling via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PermitSchedulingRow>(
            "[PermitToWork].[usp_PermitScheduling_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PermitScheduling {PermitSchedulingId}", aggregate.PermitSchedulingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitSchedulingId", aggregate.PermitSchedulingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddDateTime("ScheduledStartUtc", aggregate.ScheduledStartUtc?.Value)
            .AddDateTime("ScheduledEndUtc", aggregate.ScheduledEndUtc?.Value)
            .AddBool("ConflictDetected", aggregate.ConflictDetected)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitScheduling_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PermitScheduling {PermitSchedulingId}", aggregate.PermitSchedulingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitSchedulingId", aggregate.PermitSchedulingId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddDateTime("ScheduledStartUtc", aggregate.ScheduledStartUtc?.Value)
            .AddDateTime("ScheduledEndUtc", aggregate.ScheduledEndUtc?.Value)
            .AddBool("ConflictDetected", aggregate.ConflictDetected)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitScheduling_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="PermitScheduling"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling Hydrate(PermitSchedulingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling>();

        AggregateReflectionHydrator.SetProperty(agg, "PermitSchedulingId", PermitSchedulingId.FromGuid(row.PermitSchedulingId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PermitSchedulingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PermitNumber", row.PermitNumber);
        AggregateReflectionHydrator.SetProperty(agg, "ScheduledStartUtc", row.ScheduledStartUtc.HasValue ? UtcTimestamp.From(row.ScheduledStartUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ScheduledEndUtc", row.ScheduledEndUtc.HasValue ? UtcTimestamp.From(row.ScheduledEndUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ConflictDetected", row.ConflictDetected);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPermitSchedulingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PermitScheduling_GetById</c>/<c>usp_PermitScheduling_GetAllActive</c>.</summary>
    private sealed record PermitSchedulingRow
    {
        public required Guid PermitSchedulingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PermitNumber { get; init; }
        public DateTime? ScheduledStartUtc { get; init; }
        public DateTime? ScheduledEndUtc { get; init; }
        public required bool ConflictDetected { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
