// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Infrastructure
//  File:           PPEInventoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPPEInventoryRepository (Domain repository
//                  interface for the PPEInventory aggregate root). All DB access via stored
//                  procedures in the [PPE] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PPEInventory exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PPE.PPEInventory.Aggregates;
using PlantSafety.Domain.PPE.PPEInventory.Enums;
using PlantSafety.Domain.PPE.PPEInventory.Events;
using PlantSafety.Domain.PPE.PPEInventory.Interfaces;
using PlantSafety.Domain.PPE.PPEInventory.Specifications;
using PlantSafety.Domain.PPE.PPEInventory.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PPE.PPEInventory.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PPEInventory"/> aggregate root.</summary>
public sealed class PPEInventoryDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PPEInventoryDapperRepository> logger)
    : IPPEInventoryRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory?> GetByIdAsync(PPEInventoryId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PPEInventory {PPEInventoryId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PPEInventoryId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PPEInventoryRow>(
            "[PPE].[usp_PPEInventory_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory>> ListAsync(PPEInventorySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PPEInventory via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PPEInventoryRow>(
            "[PPE].[usp_PPEInventory_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PPEInventory {PPEInventoryId}", aggregate.PPEInventoryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEInventoryId", aggregate.PPEInventoryId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PpeItemCode", aggregate.PpeItemCode, 400)
            .AddInt("StockQuantity", aggregate.StockQuantity)
            .AddInt("IssuedQuantity", aggregate.IssuedQuantity)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddInt("ReorderThreshold", aggregate.ReorderThreshold)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEInventory_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PPEInventory {PPEInventoryId}", aggregate.PPEInventoryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PPEInventoryId", aggregate.PPEInventoryId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PpeItemCode", aggregate.PpeItemCode, 400)
            .AddInt("StockQuantity", aggregate.StockQuantity)
            .AddInt("IssuedQuantity", aggregate.IssuedQuantity)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddInt("ReorderThreshold", aggregate.ReorderThreshold)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PPE].[usp_PPEInventory_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="PPEInventory"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory Hydrate(PPEInventoryRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory>();

        AggregateReflectionHydrator.SetProperty(agg, "PPEInventoryId", PPEInventoryId.FromGuid(row.PPEInventoryId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PPEInventoryStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PpeItemCode", row.PpeItemCode);
        AggregateReflectionHydrator.SetProperty(agg, "StockQuantity", row.StockQuantity);
        AggregateReflectionHydrator.SetProperty(agg, "IssuedQuantity", row.IssuedQuantity);
        AggregateReflectionHydrator.SetProperty(agg, "ExpiresAtUtc", row.ExpiresAtUtc.HasValue ? UtcTimestamp.From(row.ExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ReorderThreshold", row.ReorderThreshold);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPPEInventoryDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PPEInventory_GetById</c>/<c>usp_PPEInventory_GetAllActive</c>.</summary>
    private sealed record PPEInventoryRow
    {
        public required Guid PPEInventoryId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PpeItemCode { get; init; }
        public required int StockQuantity { get; init; }
        public required int IssuedQuantity { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public required int ReorderThreshold { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
