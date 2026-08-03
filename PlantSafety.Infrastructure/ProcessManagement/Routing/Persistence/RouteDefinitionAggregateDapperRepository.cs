// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RouteDefinitionAggregateDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRouteDefinitionRepository (Domain repository
//                  interface for the RouteDefinition aggregate root). All DB access via stored
//                  procedures in the [ProcessManagement] schema, against
//                  [ProcessManagement].[RouteDefinitionAggregate]. The current, editable ordered
//                  step list is persisted as a JSON array column (see RouteStepJsonModel) rather
//                  than a normalized child table -- a pragmatic, defensible choice for an
//                  aggregate-owned child collection with no independent query surface of its own
//                  (RouteVersionSnapshot is the independently-queried historical record). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because RouteDefinition exposes
//                  only a private "for rehydration only" constructor and private-setter
//                  properties, mirroring
//                  PlantSafety.Infrastructure.ProcessManagement.Persistence.ProcessDefinitionAggregateDapperRepository.
//                  Both AddAsync AND UpdateAsync enqueue domain events to the outbox after their
//                  write. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.Entities;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.Events;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

/// <summary>Dapper-backed repository for the <see cref="RouteDefinition"/> aggregate root.</summary>
public sealed class RouteDefinitionAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RouteDefinitionAggregateDapperRepository> logger)
    : IRouteDefinitionRepository
{
    /// <inheritdoc/>
    public async Task<RouteDefinition?> GetByIdAsync(RouteDefinitionId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RouteDefinition {RouteDefinitionId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RouteDefinitionRow>(
            "[ProcessManagement].[usp_RouteDefinitionAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(RouteDefinition aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add RouteDefinition {RouteDefinitionId}", aggregate.RouteDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", aggregate.RouteDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("PlantName", aggregate.PlantName, 200)
            .AddString("Name", aggregate.Name, 200)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddInt("Version", aggregate.Version)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddString("OwnerName", aggregate.OwnerName, 200)
            .AddDateTime("EffectiveFromUtc", aggregate.EffectiveFromUtc.Value)
            .AddDateTime("EffectiveToUtc", aggregate.EffectiveToUtc?.Value)
            .AddString("StepsJson", RouteStepJsonSerializer.Serialize(aggregate.ToStepSnapshots()), size: -1)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .AddDateTime("LastAuditedAtUtc", aggregate.LastAuditedAtUtc.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ProcessManagement].[usp_RouteDefinitionAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(RouteDefinition aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update RouteDefinition {RouteDefinitionId}", aggregate.RouteDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", aggregate.RouteDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddInt("Version", aggregate.Version)
            .AddString("StepsJson", RouteStepJsonSerializer.Serialize(aggregate.ToStepSnapshots()), size: -1)
            .AddDateTime("LastAuditedAtUtc", aggregate.LastAuditedAtUtc.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[ProcessManagement].[usp_RouteDefinitionAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"RouteDefinition {aggregate.RouteDefinitionId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="RouteDefinition"/> aggregate from its flat read-model row.</summary>
    private static RouteDefinition Hydrate(RouteDefinitionRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<RouteDefinition>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.RouteDefinitionId), RouteDefinitionId.FromGuid(row.RouteDefinitionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.PlantName), row.PlantName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.Status), Enum.Parse<RouteDefinitionStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.Version), row.Version);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.OwnerName), row.OwnerName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.EffectiveFromUtc), UtcTimestamp.From(row.EffectiveFromUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.EffectiveToUtc), row.EffectiveToUtc.HasValue ? UtcTimestamp.From(row.EffectiveToUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(RouteDefinition.LastAuditedAtUtc), UtcTimestamp.From(row.LastAuditedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IRouteDefinitionDomainEvent>());

        var steps = RouteStepJsonSerializer.Deserialize(row.StepsJson)
            .Select(s => RouteStep.Rehydrate(s.StepId, s.StepNumber, s.Name, s.Instructions, s.Resources, s.EstimatedDurationMinutes))
            .ToList();
        AggregateReflectionHydrator.SetField(agg, "_steps", steps);

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_RouteDefinitionAggregate_GetById</c>.</summary>
    private sealed record RouteDefinitionRow
    {
        public string? ETag { get; init; }

        public required Guid RouteDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string PlantName { get; init; }
        public required string Name { get; init; }
        public required string Status { get; init; }
        public required int Version { get; init; }
        public required Guid OwnerId { get; init; }
        public required string OwnerName { get; init; }
        public required DateTime EffectiveFromUtc { get; init; }
        public DateTime? EffectiveToUtc { get; init; }
        public string? StepsJson { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public required DateTime LastAuditedAtUtc { get; init; }
    }
}
