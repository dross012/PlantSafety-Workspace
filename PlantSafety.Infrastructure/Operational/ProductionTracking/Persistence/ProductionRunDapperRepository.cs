// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Infrastructure
//  File:           ProductionRunDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProductionRunRepository (Domain repository
//                  interface for the ProductionRun aggregate root). All DB access via stored
//                  procedures in the [Operational] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator since ProductionRun exposes only a private
//                  "for rehydration only" constructor and private-setter properties. Every
//                  AddAsync/UpdateAsync call additionally persists one
//                  [Operational].[ProductionRunEvents]/[ProductionBatches]/
//                  [ProductionDowntimeEvents] row per domain event raised, mirroring
//                  WorkOrderDapperRepository's MaintenanceEvents-per-domain-event precedent, so
//                  GetRunTimelineQuery/GetRunBatchesQuery/the bottleneck-detection read path are
//                  backed by genuine data. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Operational.ProductionTracking.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ProductionRun"/> aggregate root.</summary>
public sealed class ProductionRunDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ProductionRunDapperRepository> logger)
    : IProductionRunRepository
{
    /// <inheritdoc/>
    public async Task<ProductionRun?> GetByIdAsync(ProductionRunId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ProductionRun {ProductionRunId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionRunId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ProductionRunRow>(
            "[Operational].[usp_ProductionRun_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ProductionRun aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ProductionRun {ProductionRunId}", aggregate.ProductionRunId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionRunId", aggregate.ProductionRunId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("RunCode", aggregate.RunCode, 50)
            .AddGuid("LineId", aggregate.LineId.Value)
            .AddString("LineName", aggregate.LineName, 200)
            .AddGuid("MachineId", aggregate.MachineId)
            .AddString("OperatorName", aggregate.OperatorName, 200)
            .AddDouble("IdealCycleTimeSeconds", aggregate.IdealCycleTimeSeconds)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionRun_Insert]", parameters, tx, cancellationToken: cancellationToken);

        await PersistDomainEventsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProductionRun aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ProductionRun {ProductionRunId}", aggregate.ProductionRunId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProductionRunId", aggregate.ProductionRunId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddInt("GoodUnitCount", aggregate.GoodUnitCount)
            .AddInt("ScrapUnitCount", aggregate.ScrapUnitCount)
            .AddDouble("DowntimeMinutesTotal", aggregate.DowntimeMinutesTotal)
            .AddDateTime("EndedAtUtc", aggregate.EndedAtUtc?.Value)
            .AddDateTime("UpdatedAtUtc", aggregate.UpdatedAtUtc.Value)
            .AddString("ETag", eTag, size: 20)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. Mirrors usp_WorkOrder_Update's pattern.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Operational].[usp_ProductionRun_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ProductionRun {aggregate.ProductionRunId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        await PersistDomainEventsAsync(conn, tx, aggregate, cancellationToken);

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>
    /// Persists one timeline/batch/downtime row per domain event raised by <paramref name="aggregate"/>
    /// during this unit of work, within the same transaction as the aggregate write.
    /// </summary>
    private async Task PersistDomainEventsAsync(IDbConnection conn, IDbTransaction tx, ProductionRun aggregate, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            switch (domainEvent)
            {
                case ProductionRunStartedEvent started:
                    await InsertTimelineEventAsync(conn, tx, aggregate, "Start", "Production run started.", started.StartedAtUtc.Value, cancellationToken);
                    break;

                case ProductionRunStatusChangedEvent statusChanged:
                    await InsertTimelineEventAsync(
                        conn, tx, aggregate,
                        statusChanged.NewStatus.ToString(),
                        DescribeStatusChange(statusChanged),
                        statusChanged.ChangedAtUtc.Value,
                        cancellationToken);
                    break;

                case ProductionRunEventRecordedEvent recorded:
                    await InsertTimelineEventAsync(conn, tx, aggregate, recorded.RunEventType, recorded.Description, recorded.OccurredAtUtc.Value, cancellationToken);

                    if (recorded.IsDowntime)
                    {
                        var downtimeParameters = new StoredProcedureBuilder()
                            .AddGuid("EventId", Guid.NewGuid())
                            .AddGuid("TenantId", aggregate.TenantId)
                            .AddGuid("ProductionRunId", aggregate.ProductionRunId.Value)
                            .AddGuid("MachineId", aggregate.MachineId)
                            .AddString("ReasonCode", recorded.DowntimeReasonCode ?? "UNSPECIFIED", 50)
                            .AddDateTime("StartUtc", recorded.OccurredAtUtc.Value)
                            .AddDateTime("EndUtc", recorded.OccurredAtUtc.Value.AddMinutes(recorded.DowntimeMinutes))
                            .AddBool("IsPlanned", false)
                            .Build();

                        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionDowntimeEvent_Insert]", downtimeParameters, tx, cancellationToken: cancellationToken);
                    }
                    break;

                case ProductionBatchAddedEvent batchAdded:
                    var batchParameters = new StoredProcedureBuilder()
                        .AddGuid("BatchId", batchAdded.BatchId.Value)
                        .AddGuid("TenantId", aggregate.TenantId)
                        .AddGuid("ProductionRunId", aggregate.ProductionRunId.Value)
                        .AddString("BatchCode", batchAdded.BatchCode, 50)
                        .AddString("Status", batchAdded.Status.ToString(), 20)
                        .AddDateTime("StartedAtUtc", batchAdded.StartedAtUtc.Value)
                        .AddDateTime("CreatedAtUtc", batchAdded.StartedAtUtc.Value)
                        .Build();

                    await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionBatch_Insert]", batchParameters, tx, cancellationToken: cancellationToken);
                    break;

                case ProductionRunCompletedEvent completed:
                    await InsertTimelineEventAsync(
                        conn, tx, aggregate, completed.FinalStatus.ToString(),
                        $"Production run ended ({completed.FinalStatus}). Good units: {completed.GoodUnitCount}, scrap units: {completed.ScrapUnitCount}.",
                        completed.EndedAtUtc.Value, cancellationToken);
                    break;
            }
        }
    }

    private async Task InsertTimelineEventAsync(
        IDbConnection conn, IDbTransaction tx, ProductionRun aggregate, string eventType, string description, DateTime occurredAtUtc, CancellationToken cancellationToken)
    {
        var eventParameters = new StoredProcedureBuilder()
            .AddGuid("EventId", Guid.NewGuid())
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("ProductionRunId", aggregate.ProductionRunId.Value)
            .AddString("RunCode", aggregate.RunCode, 50)
            .AddString("RunEventType", eventType, 50)
            .AddString("Description", description, 2000)
            .AddDateTime("OccurredAtUtc", occurredAtUtc)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_ProductionRunEvent_Insert]", eventParameters, tx, cancellationToken: cancellationToken);
    }

    private static string DescribeStatusChange(ProductionRunStatusChangedEvent e) =>
        string.IsNullOrWhiteSpace(e.Reason)
            ? $"Status changed from {e.PreviousStatus} to {e.NewStatus}."
            : $"Status changed from {e.PreviousStatus} to {e.NewStatus} ({e.Reason}).";

    /// <summary>Reconstructs a <see cref="ProductionRun"/> aggregate from its flat read-model row.</summary>
    private static ProductionRun Hydrate(ProductionRunRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ProductionRun>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.ProductionRunId), ProductionRunId.FromGuid(row.ProductionRunId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.Status), Enum.Parse<ProductionRunStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.RunCode), row.RunCode);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.LineId), ProductionLineId.FromGuid(row.LineId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.LineName), row.LineName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.MachineId), row.MachineId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.OperatorName), row.OperatorName ?? string.Empty);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.IdealCycleTimeSeconds), row.IdealCycleTimeSeconds);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.GoodUnitCount), row.GoodUnitCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.ScrapUnitCount), row.ScrapUnitCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.DowntimeMinutesTotal), row.DowntimeMinutesTotal);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.StartedAtUtc), UtcTimestamp.From(row.StartedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.EndedAtUtc), row.EndedAtUtc.HasValue ? UtcTimestamp.From(row.EndedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProductionRun.UpdatedAtUtc), UtcTimestamp.From(row.UpdatedAtUtc ?? row.CreatedAtUtc));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IProductionTrackingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ProductionRun_GetById</c>.</summary>
    private sealed record ProductionRunRow
    {
        public string? ETag { get; init; }

        public required Guid ProductionRunId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string RunCode { get; init; }
        public required Guid LineId { get; init; }
        public required string LineName { get; init; }
        public required Guid MachineId { get; init; }
        public string? OperatorName { get; init; }
        public required double IdealCycleTimeSeconds { get; init; }
        public required int GoodUnitCount { get; init; }
        public required int ScrapUnitCount { get; init; }
        public required double DowntimeMinutesTotal { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? EndedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
