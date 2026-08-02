// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Infrastructure
//  File:           DrillsAndExercisesAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer (write-side)
//                  IDrillsAndExercisesRepository (PlantSafety.Domain.SafetyEvents
//                  .DrillsAndExercises.Interfaces.IDrillsAndExercisesRepository) for the
//                  DrillsAndExercises aggregate root. All DB access via stored procedures in the
//                  [SafetyEvents] schema. Aggregate reconstruction uses AggregateReflectionHydrator
//                  because DrillsAndExercises exposes only a private "for rehydration only"
//                  constructor and private-setter properties. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag from day one. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write.
//                  Named distinctly from the pre-existing DrillsAndExercisesDapperRepository (the
//                  read-only list/by-id repository implementing the Application-layer
//                  IDrillsAndExercisesRepository) to avoid a same-simple-name class collision
//                  within this same namespace. Mirrors PredictiveMaintenanceAlertDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Enums;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Events;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
// Type alias required: this file's own namespace (PlantSafety.Infrastructure.SafetyEvents
// .DrillsAndExercises.Persistence) contains a "DrillsAndExercises" segment, so an unqualified
// "DrillsAndExercises" identifier resolves to that namespace segment, not the aggregate type,
// producing CS0118. Mirrors the same disambiguation need documented on the Domain-layer
// IDrillsAndExercisesRepository interface.
using DrillsAndExercisesAggregate = PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises;

namespace PlantSafety.Infrastructure.SafetyEvents.DrillsAndExercises.Persistence;

/// <summary>Dapper-backed repository for the <see cref="DrillsAndExercisesAggregate"/> aggregate root (write side).</summary>
public sealed class DrillsAndExercisesAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DrillsAndExercisesAggregateDapperRepository> logger)
    : IDrillsAndExercisesRepository
{
    /// <inheritdoc/>
    public async Task<DrillsAndExercisesAggregate?> GetByIdAsync(DrillsAndExercisesId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for DrillsAndExercises {DrillsAndExercisesId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DrillsAndExercisesId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DrillsAndExercisesRow>(
            "[SafetyEvents].[usp_DrillsAndExercises_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(DrillsAndExercisesAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add DrillsAndExercises {DrillsAndExercisesId}", aggregate.DrillsAndExercisesId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DrillsAndExercisesId", aggregate.DrillsAndExercisesId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DrillType", aggregate.DrillType, 50)
            .AddInt("ParticipantCount", aggregate.ParticipantCount)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddDateTime("ConductedAtUtc", aggregate.ConductedAtUtc?.Value)
            .AddGuid("ConductedBy", aggregate.ConductedBy?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddString("DebriefNotes", aggregate.DebriefNotes, 2000)
            .AddGuid("CancelledBy", aggregate.CancelledBy?.Value)
            .AddString("CancelledReason", aggregate.CancelledReason, 2000)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_DrillsAndExercises_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(DrillsAndExercisesAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update DrillsAndExercises {DrillsAndExercisesId}", aggregate.DrillsAndExercisesId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DrillsAndExercisesId", aggregate.DrillsAndExercisesId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DrillType", aggregate.DrillType, 50)
            .AddInt("ParticipantCount", aggregate.ParticipantCount)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddDateTime("ConductedAtUtc", aggregate.ConductedAtUtc?.Value)
            .AddGuid("ConductedBy", aggregate.ConductedBy?.Value)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddString("DebriefNotes", aggregate.DebriefNotes, 2000)
            .AddGuid("CancelledBy", aggregate.CancelledBy?.Value)
            .AddString("CancelledReason", aggregate.CancelledReason, 2000)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_DrillsAndExercises_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_DrillsAndExercises_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"DrillsAndExercises {aggregate.DrillsAndExercisesId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="DrillsAndExercisesAggregate"/> from its flat read-model row.</summary>
    private static DrillsAndExercisesAggregate Hydrate(DrillsAndExercisesRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<DrillsAndExercisesAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.DrillsAndExercisesId), DrillsAndExercisesId.FromGuid(row.DrillsAndExercisesId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.Status), Enum.Parse<DrillsAndExercisesStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.DrillType), row.DrillType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ParticipantCount), row.ParticipantCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ScheduledAtUtc), row.ScheduledAtUtc.HasValue ? UtcTimestamp.From(row.ScheduledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ConductedAtUtc), row.ConductedAtUtc.HasValue ? UtcTimestamp.From(row.ConductedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ConductedBy), row.ConductedBy.HasValue ? new UserId(row.ConductedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CompletedBy), row.CompletedBy.HasValue ? new UserId(row.CompletedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.DebriefNotes), row.DebriefNotes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CancelledBy), row.CancelledBy.HasValue ? new UserId(row.CancelledBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CancelledReason), row.CancelledReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CancelledAtUtc), row.CancelledAtUtc.HasValue ? UtcTimestamp.From(row.CancelledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(DrillsAndExercisesAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IDrillsAndExercisesDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_DrillsAndExercises_GetById</c>.</summary>
    private sealed record DrillsAndExercisesRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_DrillsAndExercises_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer DrillsAndExercisesDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid DrillsAndExercisesId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DrillType { get; init; }
        public required int ParticipantCount { get; init; }
        public DateTime? ScheduledAtUtc { get; init; }
        public DateTime? ConductedAtUtc { get; init; }
        public Guid? ConductedBy { get; init; }
        public Guid? CompletedBy { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public string? DebriefNotes { get; init; }
        public Guid? CancelledBy { get; init; }
        public string? CancelledReason { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
