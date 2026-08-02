// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Infrastructure
//  File:           DataLakeActivityAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  PlantSafety.Domain.Intelligence.DataLake.Interfaces.IDataLakeActivityRepository
//                  for the DataLakeActivity aggregate root. All DB access via stored procedures in
//                  the [Intelligence] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because DataLakeActivity exposes only a private "for
//                  rehydration only" constructor and private-setter properties. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from day
//                  one. Both AddAsync AND UpdateAsync enqueue domain events to the outbox after
//                  their write. Named "DataLakeActivityAggregateDapperRepository" (rather than
//                  "DataLakeDapperRepository") to avoid colliding with the pre-existing read-list
//                  DataLakeDapperRepository already declared in the sibling Persistence namespace
//                  (backs the unrelated, untouched date-range
//                  GetDataLakeQuery/usp_DataLake_GetDataLakeRecords read path). Mirrors
//                  PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence
//                  .RCAAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.DataLake.Enums;
using PlantSafety.Domain.Intelligence.DataLake.Events;
using PlantSafety.Domain.Intelligence.DataLake.Interfaces;
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using DataLakeActivityAggregate = PlantSafety.Domain.Intelligence.DataLake.Aggregates.DataLakeActivity;

namespace PlantSafety.Infrastructure.Intelligence.DataLake.Persistence;

/// <summary>Dapper-backed repository for the <see cref="DataLakeActivityAggregate"/> aggregate root.</summary>
public sealed class DataLakeActivityAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DataLakeActivityAggregateDapperRepository> logger)
    : IDataLakeActivityRepository
{
    /// <inheritdoc/>
    public async Task<DataLakeActivityAggregate?> GetByIdAsync(DataLakeActivityId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for DataLakeActivity {DataLakeActivityId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DataLakeActivityId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DataLakeActivityRow>(
            "[Intelligence].[usp_DataLakeActivity_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(DataLakeActivityAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add DataLakeActivity {DataLakeActivityId}", aggregate.DataLakeActivityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DataLakeActivityId", aggregate.DataLakeActivityId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("DatasetName", aggregate.DatasetName, 200)
            .AddString("Zone", aggregate.Zone, 50)
            .AddLong("RecordCount", aggregate.RecordCount)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("ProcessedAtUtc", aggregate.ProcessedAtUtc.Value)
            .AddGuid("ProcessedBy", aggregate.ProcessedBy.Value)
            .AddGuid("ProcessingStartedBy", aggregate.ProcessingStartedBy?.Value)
            .AddDateTime("ProcessingStartedAtUtc", aggregate.ProcessingStartedAtUtc?.Value)
            .AddDateTime("CuratedAtUtc", aggregate.CuratedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, size: -1)
            .AddDateTime("FailedAtUtc", aggregate.FailedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_DataLakeActivity_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(DataLakeActivityAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update DataLakeActivity {DataLakeActivityId}", aggregate.DataLakeActivityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DataLakeActivityId", aggregate.DataLakeActivityId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("DatasetName", aggregate.DatasetName, 200)
            .AddString("Zone", aggregate.Zone, 50)
            .AddLong("RecordCount", aggregate.RecordCount)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("ProcessingStartedBy", aggregate.ProcessingStartedBy?.Value)
            .AddDateTime("ProcessingStartedAtUtc", aggregate.ProcessingStartedAtUtc?.Value)
            .AddDateTime("CuratedAtUtc", aggregate.CuratedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, size: -1)
            .AddDateTime("FailedAtUtc", aggregate.FailedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("ModifiedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_DataLakeActivity_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_DataLakeActivity_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"DataLakeActivity {aggregate.DataLakeActivityId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="DataLakeActivityAggregate"/> aggregate from its flat read-model row.</summary>
    private static DataLakeActivityAggregate Hydrate(DataLakeActivityRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<DataLakeActivityAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.DataLakeActivityId), DataLakeActivityId.FromGuid(row.DataLakeActivityId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.Status), Enum.Parse<DataLakeActivityStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.DatasetName), row.DatasetName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.Zone), row.Zone);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.RecordCount), row.RecordCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.ProcessedAtUtc), UtcTimestamp.From(row.ProcessedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.ProcessedBy), new UserId(row.ProcessedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.ProcessingStartedBy), row.ProcessingStartedBy.HasValue ? new UserId(row.ProcessingStartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.ProcessingStartedAtUtc), row.ProcessingStartedAtUtc.HasValue ? UtcTimestamp.From(row.ProcessingStartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.CuratedAtUtc), row.CuratedAtUtc.HasValue ? UtcTimestamp.From(row.CuratedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.FailureReason), row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.FailedAtUtc), row.FailedAtUtc.HasValue ? UtcTimestamp.From(row.FailedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DataLakeActivityAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IDataLakeActivityDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_DataLakeActivity_GetById</c>.</summary>
    private sealed record DataLakeActivityRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_DataLakeActivity_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer DataLakeActivityDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid DataLakeActivityId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string DatasetName { get; init; }
        public required string Zone { get; init; }
        public long? RecordCount { get; init; }
        public required string Status { get; init; }
        public required DateTime ProcessedAtUtc { get; init; }
        public required Guid ProcessedBy { get; init; }
        public Guid? ProcessingStartedBy { get; init; }
        public DateTime? ProcessingStartedAtUtc { get; init; }
        public DateTime? CuratedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? FailedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
