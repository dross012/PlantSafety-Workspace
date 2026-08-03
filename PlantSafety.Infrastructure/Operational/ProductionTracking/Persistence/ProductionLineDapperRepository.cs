// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Infrastructure
//  File:           ProductionLineDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProductionLineRepository (Domain repository
//                  interface for the ProductionLine master-data aggregate root). All DB access via
//                  stored procedures in the [Operational] schema. Mirrors
//                  PlantSafety.Infrastructure.Operational.WorkOrders.Persistence.PartDapperRepository's
//                  conventions. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.ProductionTracking.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ProductionLine"/> aggregate root.</summary>
public sealed class ProductionLineDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ProductionLineDapperRepository> logger)
    : IProductionLineRepository
{
    /// <inheritdoc/>
    public async Task<ProductionLine?> GetByIdAsync(ProductionLineId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ProductionLine {ProductionLineId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionLineId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ProductionLineRow>(
            "[Operational].[usp_ProductionLine_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ProductionLine aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ProductionLine {ProductionLineId}", aggregate.ProductionLineId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionLineId", aggregate.ProductionLineId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("LineCode", aggregate.LineCode, 50)
            .AddString("LineName", aggregate.LineName, 200)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionLine_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProductionLine aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ProductionLine {ProductionLineId}", aggregate.ProductionLineId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionLineId", aggregate.ProductionLineId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("UpdatedAtUtc", aggregate.UpdatedAtUtc.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionLine_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ProductionLine"/> aggregate from its flat read-model row.</summary>
    private static ProductionLine Hydrate(ProductionLineRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ProductionLine>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.ProductionLineId), ProductionLineId.FromGuid(row.ProductionLineId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.LineCode), row.LineCode);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.LineName), row.LineName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.Status), Enum.Parse<ProductionLineStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionLine.UpdatedAtUtc), UtcTimestamp.From(row.UpdatedAtUtc ?? row.CreatedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IProductionTrackingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ProductionLine_GetById</c>.</summary>
    private sealed record ProductionLineRow
    {
        public required Guid ProductionLineId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string LineCode { get; init; }
        public required string LineName { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
