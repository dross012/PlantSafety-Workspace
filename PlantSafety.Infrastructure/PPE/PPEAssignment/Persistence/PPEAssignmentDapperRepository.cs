// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Infrastructure
//  File:           PPEAssignmentDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPEAssignmentRepository (Domain repository
//                  interface for the PPEAssignment aggregate root). All DB access via stored
//                  procedures in the [PPE] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPEAssignment exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PPE.PPEAssignment.Aggregates;
using PlantSafety.Domain.PPE.PPEAssignment.Enums;
using PlantSafety.Domain.PPE.PPEAssignment.Events;
using PlantSafety.Domain.PPE.PPEAssignment.Interfaces;
using PlantSafety.Domain.PPE.PPEAssignment.Specifications;
using PlantSafety.Domain.PPE.PPEAssignment.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PPE.PPEAssignment.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPEAssignment"/> aggregate root.</summary>
public sealed class PPEAssignmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPEAssignmentDapperRepository> logger)
    : IPPEAssignmentRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment?> GetByIdAsync(PPEAssignmentId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPEAssignment {PPEAssignmentId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPEAssignmentId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPEAssignmentRow>(
            "[PPE].[usp_PPEAssignment_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment>> ListAsync(PPEAssignmentSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPEAssignment via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPEAssignmentRow>(
            "[PPE].[usp_PPEAssignment_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPEAssignment {PPEAssignmentId}", aggregate.PPEAssignmentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEAssignmentId", aggregate.PPEAssignmentId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId)
            .AddString("JobRole", aggregate.JobRole, 400)
            .AddString("HazardZone", aggregate.HazardZone, 400)
            .AddString("AssignedPpeItems", aggregate.AssignedPpeItems, 400)
            .AddDateTime("FitTestExpiresAtUtc", aggregate.FitTestExpiresAtUtc?.Value)
            .AddString("ReplacementReason", aggregate.ReplacementReason, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEAssignment_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPEAssignment {PPEAssignmentId}", aggregate.PPEAssignmentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEAssignmentId", aggregate.PPEAssignmentId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("WorkerId", aggregate.WorkerId)
            .AddString("JobRole", aggregate.JobRole, 400)
            .AddString("HazardZone", aggregate.HazardZone, 400)
            .AddString("AssignedPpeItems", aggregate.AssignedPpeItems, 400)
            .AddDateTime("FitTestExpiresAtUtc", aggregate.FitTestExpiresAtUtc?.Value)
            .AddString("ReplacementReason", aggregate.ReplacementReason, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEAssignment_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PPEAssignment"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment Hydrate(PPEAssignmentRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment>();

        AggregateReflectionHydrator.SetProperty(agg, "PPEAssignmentId", PPEAssignmentId.FromGuid(row.PPEAssignmentId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPEAssignmentStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "WorkerId", row.WorkerId);
        AggregateReflectionHydrator.SetProperty(agg, "JobRole", row.JobRole);
        AggregateReflectionHydrator.SetProperty(agg, "HazardZone", row.HazardZone);
        AggregateReflectionHydrator.SetProperty(agg, "AssignedPpeItems", row.AssignedPpeItems);
        AggregateReflectionHydrator.SetProperty(agg, "FitTestExpiresAtUtc", row.FitTestExpiresAtUtc.HasValue ? UtcTimestamp.From(row.FitTestExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ReplacementReason", row.ReplacementReason);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPEAssignmentDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPEAssignment_GetById</c>/<c>usp_PPEAssignment_GetAllActive</c>.</summary>
    private sealed record PPEAssignmentRow
    {
        public required Guid PPEAssignmentId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid WorkerId { get; init; }
        public required string JobRole { get; init; }
        public required string HazardZone { get; init; }
        public required string AssignedPpeItems { get; init; }
        public DateTime? FitTestExpiresAtUtc { get; init; }
        public string? ReplacementReason { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
