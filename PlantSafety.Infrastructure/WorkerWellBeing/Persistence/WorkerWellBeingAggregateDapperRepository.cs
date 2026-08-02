// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IWorkerWellBeingRepository (Domain repository
//                  interface for the WorkerWellBeing aggregate root). All DB access via stored
//                  procedures in the [WorkerWellBeing] schema, against
//                  [WorkerWellBeing].[WorkerWellBeingAggregate] (named to avoid ambiguity with the
//                  pre-existing, unrelated Features-vertical
//                  [WorkerWellBeing].[WorkerWellBeingRecords]
//                  table/IWorkerWellBeingRecordRepository that lives in this same module folder
//                  but is a different, unrelated concept -- named
//                  "WorkerWellBeingAggregateDapperRepository" rather than bare
//                  "WorkerWellBeingDapperRepository" per the module's aggregate/namespace
//                  name-collision precedent). Uses the aggregate type's fully qualified name
//                  throughout (rather than a bare "WorkerWellBeing" reference or a using-alias)
//                  because the enclosing Infrastructure namespace segment
//                  "PlantSafety.Infrastructure.WorkerWellBeing" itself binds the simple name
//                  "WorkerWellBeing" to a namespace, not the aggregate type, before any
//                  using-alias is considered (CS0118). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because WorkerWellBeing exposes only a private
//                  "for rehydration only" constructor and private-setter properties. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their
//                  write. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.WorkerWellBeing.Enums;
using PlantSafety.Domain.WorkerWellBeing.Events;
using PlantSafety.Domain.WorkerWellBeing.Repositories;
using PlantSafety.Domain.WorkerWellBeing.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.WorkerWellBeing.Persistence;

/// <summary>Dapper-backed repository for the WorkerWellBeing aggregate root.</summary>
public sealed class WorkerWellBeingAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WorkerWellBeingAggregateDapperRepository> logger)
    : IWorkerWellBeingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing?> GetByIdAsync(WorkerWellBeingId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WorkerWellBeing {WorkerWellBeingId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkerWellBeingId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkerWellBeingRow>(
            "[WorkerWellBeing].[usp_WorkerWellBeingAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add WorkerWellBeing {WorkerWellBeingId}", aggregate.WorkerWellBeingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkerWellBeingId", aggregate.WorkerWellBeingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("WorkerId", aggregate.WorkerId.Value)
            .AddString("WellBeingRiskLevel", aggregate.WellBeingRiskLevel, 50)
            .AddString("CheckInType", aggregate.CheckInType, 100)
            .AddInt("WellBeingScore", aggregate.WellBeingScore)
            .AddBool("FollowUpRequired", aggregate.FollowUpRequired)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("RecordedAtUtc", aggregate.RecordedAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[WorkerWellBeing].[usp_WorkerWellBeingAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update WorkerWellBeing {WorkerWellBeingId}", aggregate.WorkerWellBeingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkerWellBeingId", aggregate.WorkerWellBeingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("WellBeingRiskLevel", aggregate.WellBeingRiskLevel, 50)
            .AddString("CheckInType", aggregate.CheckInType, 100)
            .AddInt("WellBeingScore", aggregate.WellBeingScore)
            .AddBool("FollowUpRequired", aggregate.FollowUpRequired)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_WorkerWellBeingAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[WorkerWellBeing].[usp_WorkerWellBeingAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"WorkerWellBeing {aggregate.WorkerWellBeingId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a WorkerWellBeing aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing Hydrate(WorkerWellBeingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.WorkerWellBeingId), WorkerWellBeingId.FromGuid(row.WorkerWellBeingId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.WorkerId), new UserId(row.WorkerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.WellBeingRiskLevel), row.WellBeingRiskLevel);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.CheckInType), row.CheckInType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.WellBeingScore), row.WellBeingScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.FollowUpRequired), row.FollowUpRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.Status), Enum.Parse<WorkerWellBeingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.RecordedAtUtc), UtcTimestamp.From(row.RecordedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IWorkerWellBeingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_WorkerWellBeingAggregate_GetById</c>.</summary>
    private sealed record WorkerWellBeingRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_WorkerWellBeingAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid WorkerWellBeingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required Guid WorkerId { get; init; }
        public required string WellBeingRiskLevel { get; init; }
        public required string CheckInType { get; init; }
        public required int WellBeingScore { get; init; }
        public required bool FollowUpRequired { get; init; }
        public required string Status { get; init; }
        public required DateTime RecordedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
