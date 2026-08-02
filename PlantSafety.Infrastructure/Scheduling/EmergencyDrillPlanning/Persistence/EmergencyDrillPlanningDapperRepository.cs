// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Infrastructure
//  File:           EmergencyDrillPlanningDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEmergencyDrillPlanningRepository (Domain repository
//                  interface for the EmergencyDrillPlanning aggregate root). All DB access via stored
//                  procedures in the [Scheduling] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because EmergencyDrillPlanning exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync is now tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag, mirroring
//                  PermitsDapperRepository.cs. UpdateAsync must be invoked via ExecuteScalarAsync
//                  (not ExecuteNonQuery/ExecuteAsync) since usp_EmergencyDrillPlanning_Update's
//                  SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently
//                  disable the @ETag concurrency check. Both AddAsync and UpdateAsync already
//                  enqueued domain events to the outbox prior to this pass -- confirmed, not
//                  re-fixed.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Enums;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Events;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Specifications;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.EmergencyDrillPlanning.Persistence;

/// <summary>Dapper-backed repository for the <see cref="EmergencyDrillPlanning"/> aggregate root.</summary>
public sealed class EmergencyDrillPlanningDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EmergencyDrillPlanningDapperRepository> logger)
    : IEmergencyDrillPlanningRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning?> GetByIdAsync(EmergencyDrillPlanningId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyDrillPlanning {EmergencyDrillPlanningId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyDrillPlanningId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EmergencyDrillPlanningRow>(
            "[Scheduling].[usp_EmergencyDrillPlanning_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning>> ListAsync(EmergencyDrillPlanningSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List EmergencyDrillPlanning via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<EmergencyDrillPlanningRow>(
            "[Scheduling].[usp_EmergencyDrillPlanning_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add EmergencyDrillPlanning {EmergencyDrillPlanningId}", aggregate.EmergencyDrillPlanningId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyDrillPlanningId", aggregate.EmergencyDrillPlanningId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DrillType", aggregate.DrillType, 400)
            .AddString("ShiftCode", aggregate.ShiftCode, 400)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddInt("ParticipationCount", aggregate.ParticipationCount)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_EmergencyDrillPlanning_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update EmergencyDrillPlanning {EmergencyDrillPlanningId}", aggregate.EmergencyDrillPlanningId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyDrillPlanningId", aggregate.EmergencyDrillPlanningId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DrillType", aggregate.DrillType, 400)
            .AddString("ShiftCode", aggregate.ShiftCode, 400)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddInt("ParticipationCount", aggregate.ParticipationCount)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_EmergencyDrillPlanning_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Scheduling].[usp_EmergencyDrillPlanning_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"EmergencyDrillPlanning {aggregate.EmergencyDrillPlanningId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="EmergencyDrillPlanning"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning Hydrate(EmergencyDrillPlanningRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning>();

        AggregateReflectionHydrator.SetProperty(agg, "EmergencyDrillPlanningId", EmergencyDrillPlanningId.FromGuid(row.EmergencyDrillPlanningId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<EmergencyDrillPlanningStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "DrillType", row.DrillType);
        AggregateReflectionHydrator.SetProperty(agg, "ShiftCode", row.ShiftCode);
        AggregateReflectionHydrator.SetProperty(agg, "ScheduledAtUtc", row.ScheduledAtUtc.HasValue ? UtcTimestamp.From(row.ScheduledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ParticipationCount", row.ParticipationCount);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IEmergencyDrillPlanningDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_EmergencyDrillPlanning_GetById</c>/<c>usp_EmergencyDrillPlanning_GetAllActive</c>.</summary>
    private sealed record EmergencyDrillPlanningRow
    {
        public required Guid EmergencyDrillPlanningId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DrillType { get; init; }
        public required string ShiftCode { get; init; }
        public DateTime? ScheduledAtUtc { get; init; }
        public required int ParticipationCount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
