// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Infrastructure
//  File:           FinancialRecordAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IFinancialRecordRepository (Domain repository
//                  interface for the new FinancialRecord aggregate root). All DB access via
//                  stored procedures in the [Financials] schema, against
//                  [Financials].[FinancialRecord] / [Financials].[FinancialRecordLineItem] (named
//                  to avoid any ambiguity with the pre-existing, unrelated
//                  [Financials].[Financials] read-only table and [Financials].[RoiCalculation]
//                  table). Aggregate reconstruction uses AggregateReflectionHydrator because
//                  FinancialRecord exposes only a private "for rehydration only" constructor and
//                  private-setter properties, including the private _lineItems backing field.
//                  LineItems are synchronized via a delete-then-reinsert strategy on every
//                  UpdateAsync (simplest correct approach; no TVP/SplitOn precedent exists
//                  elsewhere in this codebase). Both AddAsync AND UpdateAsync enqueue domain
//                  events to the outbox after their write. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence
//                  .AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Financials.Aggregates;
using PlantSafety.Domain.Financials.Entities;
using PlantSafety.Domain.Financials.Enums;
using PlantSafety.Domain.Financials.Events;
using PlantSafety.Domain.Financials.Interfaces;
using PlantSafety.Domain.Financials.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Financials.Persistence;

/// <summary>Dapper-backed repository for the <see cref="FinancialRecord"/> aggregate root.</summary>
public sealed class FinancialRecordAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<FinancialRecordAggregateDapperRepository> logger)
    : IFinancialRecordRepository
{
    /// <inheritdoc/>
    public async Task<FinancialRecord?> GetByIdAsync(FinancialsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for FinancialRecord {FinancialRecordId} Tenant {TenantId}", id.Value, tenantId);

        var headerParameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<FinancialRecordRow>(
            "[Financials].[usp_FinancialRecord_GetById]", headerParameters, cancellationToken);

        if (row is null) return null;

        var lineItemParameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", id.Value)
            .Build();
        var lineItemRows = await queryExecutor.QueryAsync<FinancialRecordLineItemRow>(
            "[Financials].[usp_FinancialRecordLineItem_GetByFinancialRecordId]", lineItemParameters, cancellationToken);

        return Hydrate(row, lineItemRows);
    }

    /// <inheritdoc/>
    public async Task AddAsync(FinancialRecord aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add FinancialRecord {FinancialRecordId}", aggregate.FinancialsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", aggregate.FinancialsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Category", aggregate.Category, 200)
            .AddDecimal("Amount", aggregate.Amount)
            .AddDateTime("RecordedAtUtc", aggregate.RecordedAtUtc.Value)
            .AddGuid("RecordedBy", aggregate.RecordedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Financials].[usp_FinancialRecord_Insert]", parameters, tx, cancellationToken: cancellationToken);

        // Create() never seeds LineItems (they are appended afterwards via AddLineItem), but
        // synchronize defensively in case a future caller constructs an aggregate with items
        // pre-populated before the first AddAsync.
        await SyncLineItemsAsync(conn, tx, aggregate.FinancialsId.Value, aggregate.LineItems, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FinancialRecord aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update FinancialRecord {FinancialRecordId}", aggregate.FinancialsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", aggregate.FinancialsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Category", aggregate.Category, 200)
            .AddDecimal("Amount", aggregate.Amount)
            .AddGuid("UpdatedBy", aggregate.RecordedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_FinancialRecord_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Financials].[usp_FinancialRecord_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"FinancialRecord {aggregate.FinancialsId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        await SyncLineItemsAsync(conn, tx, aggregate.FinancialsId.Value, aggregate.LineItems, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>
    /// Full-replace synchronization of the FinancialRecordLineItem child table: deletes all
    /// existing rows for the aggregate, then re-inserts the current in-memory collection. Simple
    /// and correct given the LineItems collection is small and append-only while Draft.
    /// </summary>
    private async Task SyncLineItemsAsync(
        IDbConnection conn,
        IDbTransaction tx,
        Guid financialRecordId,
        IReadOnlyCollection<FinancialLineItem> lineItems,
        CancellationToken cancellationToken)
    {
        var deleteParameters = new StoredProcedureBuilder()
            .AddGuid("FinancialRecordId", financialRecordId)
            .Build();
        await commandExecutor.ExecuteAsync(conn, "[Financials].[usp_FinancialRecordLineItem_DeleteAll]", deleteParameters, tx, cancellationToken: cancellationToken);

        foreach (var lineItem in lineItems)
        {
            var insertParameters = new StoredProcedureBuilder()
                .AddGuid("LineItemId", lineItem.LineItemId)
                .AddGuid("FinancialRecordId", financialRecordId)
                .AddString("Description", lineItem.Description, 400)
                .AddDecimal("Amount", lineItem.Amount)
                .Build();
            await commandExecutor.ExecuteAsync(conn, "[Financials].[usp_FinancialRecordLineItem_Insert]", insertParameters, tx, cancellationToken: cancellationToken);
        }
    }

    /// <summary>Reconstructs a <see cref="FinancialRecord"/> aggregate from its flat read-model row and line-item rows.</summary>
    private static FinancialRecord Hydrate(FinancialRecordRow row, IReadOnlyList<FinancialRecordLineItemRow> lineItemRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<FinancialRecord>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.FinancialsId), FinancialsId.FromGuid(row.FinancialRecordId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.Status), Enum.Parse<FinancialStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.Amount), row.Amount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.RecordedAtUtc), UtcTimestamp.From(row.RecordedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(FinancialRecord.RecordedBy), new UserId(row.RecordedBy));

        var lineItems = lineItemRows
            .Select(li => new FinancialLineItem(li.LineItemId, li.Description, li.Amount))
            .ToList();
        AggregateReflectionHydrator.SetField(agg, "_lineItems", lineItems);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IFinancialsDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_FinancialRecord_GetById</c>.</summary>
    private sealed record FinancialRecordRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_FinancialRecord_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid FinancialRecordId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string Category { get; init; }
        public required decimal Amount { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required Guid RecordedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }

    /// <summary>Row shape returned by <c>usp_FinancialRecordLineItem_GetByFinancialRecordId</c>.</summary>
    private sealed record FinancialRecordLineItemRow
    {
        public required Guid LineItemId { get; init; }
        public required Guid FinancialRecordId { get; init; }
        public required string Description { get; init; }
        public required decimal Amount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
    }
}
