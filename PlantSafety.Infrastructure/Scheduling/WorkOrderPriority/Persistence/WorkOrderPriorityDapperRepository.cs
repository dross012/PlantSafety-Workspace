// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Infrastructure
//  File:           WorkOrderPriorityDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IWorkOrderPriorityRepository (Domain repository
//                  interface for the WorkOrderPriority aggregate root). All DB access via stored
//                  procedures in the [Scheduling] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because WorkOrderPriority exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Enums;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Events;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Specifications;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.WorkOrderPriority.Persistence;

/// <summary>Dapper-backed repository for the <see cref="WorkOrderPriority"/> aggregate root.</summary>
public sealed class WorkOrderPriorityDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WorkOrderPriorityDapperRepository> logger)
    : IWorkOrderPriorityRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority?> GetByIdAsync(WorkOrderPriorityId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WorkOrderPriority {WorkOrderPriorityId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("WorkOrderPriorityId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkOrderPriorityRow>(
            "[Scheduling].[usp_WorkOrderPriority_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority>> ListAsync(WorkOrderPrioritySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List WorkOrderPriority via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<WorkOrderPriorityRow>(
            "[Scheduling].[usp_WorkOrderPriority_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add WorkOrderPriority {WorkOrderPriorityId}", aggregate.WorkOrderPriorityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderPriorityId", aggregate.WorkOrderPriorityId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PriorityCode", aggregate.PriorityCode, 400)
            .AddString("HazardDescription", aggregate.HazardDescription, 400)
            .AddDateTime("DueByUtc", aggregate.DueByUtc?.Value)
            .AddDateTime("EscalatedAtUtc", aggregate.EscalatedAtUtc?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_WorkOrderPriority_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update WorkOrderPriority {WorkOrderPriorityId}", aggregate.WorkOrderPriorityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderPriorityId", aggregate.WorkOrderPriorityId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PriorityCode", aggregate.PriorityCode, 400)
            .AddString("HazardDescription", aggregate.HazardDescription, 400)
            .AddDateTime("DueByUtc", aggregate.DueByUtc?.Value)
            .AddDateTime("EscalatedAtUtc", aggregate.EscalatedAtUtc?.Value)
            .AddString("ResolutionNotes", aggregate.ResolutionNotes, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_WorkOrderPriority_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="WorkOrderPriority"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority Hydrate(WorkOrderPriorityRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority>();

        AggregateReflectionHydrator.SetProperty(agg, "WorkOrderPriorityId", WorkOrderPriorityId.FromGuid(row.WorkOrderPriorityId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<WorkOrderPriorityStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PriorityCode", row.PriorityCode);
        AggregateReflectionHydrator.SetProperty(agg, "HazardDescription", row.HazardDescription);
        AggregateReflectionHydrator.SetProperty(agg, "DueByUtc", row.DueByUtc.HasValue ? UtcTimestamp.From(row.DueByUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "EscalatedAtUtc", row.EscalatedAtUtc.HasValue ? UtcTimestamp.From(row.EscalatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ResolutionNotes", row.ResolutionNotes);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IWorkOrderPriorityDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_WorkOrderPriority_GetById</c>/<c>usp_WorkOrderPriority_GetAllActive</c>.</summary>
    private sealed record WorkOrderPriorityRow
    {
        public required Guid WorkOrderPriorityId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PriorityCode { get; init; }
        public required string HazardDescription { get; init; }
        public DateTime? DueByUtc { get; init; }
        public DateTime? EscalatedAtUtc { get; init; }
        public string? ResolutionNotes { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
