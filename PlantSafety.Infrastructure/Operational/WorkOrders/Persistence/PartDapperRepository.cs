// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           PartDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPartRepository (Domain repository interface
//                  for the Part aggregate root). All DB access via stored procedures in the
//                  [Operational] schema. Mirrors WorkOrderDapperRepository's conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Part"/> aggregate root.</summary>
public sealed class PartDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PartDapperRepository> logger)
    : IPartRepository
{
    /// <inheritdoc/>
    public async Task<Part?> GetByIdAsync(PartId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Part {PartId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PartId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PartRow>(
            "[Operational].[usp_Part_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Part aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Part {PartId}", aggregate.PartId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PartId", aggregate.PartId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("PartNumber", aggregate.PartNumber, 50)
            .AddString("PartName", aggregate.PartName, 200)
            .AddInt("QuantityOnHand", aggregate.QuantityOnHand)
            .AddInt("ReorderThreshold", aggregate.ReorderThreshold)
            .AddString("Location", aggregate.Location, 200)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_Part_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Part aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Part {PartId}", aggregate.PartId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PartId", aggregate.PartId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddInt("QuantityOnHand", aggregate.QuantityOnHand)
            .AddInt("ReorderThreshold", aggregate.ReorderThreshold)
            .AddString("Location", aggregate.Location, 200)
            .AddDateTime("UpdatedAtUtc", aggregate.UpdatedAtUtc.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Operational].[usp_Part_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Part {aggregate.PartId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="Part"/> aggregate from its flat read-model row.</summary>
    private static Part Hydrate(PartRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<Part>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.PartId), PartId.FromGuid(row.PartId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.PartNumber), row.PartNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.PartName), row.PartName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.QuantityOnHand), row.QuantityOnHand);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.ReorderThreshold), row.ReorderThreshold);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Part.UpdatedAtUtc), UtcTimestamp.From(row.UpdatedAtUtc ?? row.CreatedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPartDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Part_GetById</c>.</summary>
    private sealed record PartRow
    {
        public string? ETag { get; init; }

        public required Guid PartId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string PartNumber { get; init; }
        public required string PartName { get; init; }
        public required int QuantityOnHand { get; init; }
        public required int ReorderThreshold { get; init; }
        public required string Location { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
