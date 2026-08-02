// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Infrastructure
//  File:           ShiftFatigueDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IShiftFatigueRepository (Domain repository
//                  interface for the ShiftFatigue aggregate root). All DB access via stored
//                  procedures in the [Scheduling] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because ShiftFatigue exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync is now tenant-scoped and
//                  UpdateAsync now takes an optimistic-concurrency ETag and enforces it via
//                  ExecuteScalarAsync + @@ROWCOUNT (usp_ShiftFatigue_Update.sql), throwing
//                  ConcurrencyConflictException on zero rows affected. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write (already
//                  correct pre-existing behavior; verified, not changed). Mirrors
//                  PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Enums;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Events;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Interfaces;
using PlantSafety.Domain.Scheduling.ShiftFatigue.Specifications;
using PlantSafety.Domain.Scheduling.ShiftFatigue.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.ShiftFatigue.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ShiftFatigue"/> aggregate root.</summary>
public sealed class ShiftFatigueDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ShiftFatigueDapperRepository> logger)
    : IShiftFatigueRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue?> GetByIdAsync(ShiftFatigueId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ShiftFatigue {ShiftFatigueId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ShiftFatigueId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ShiftFatigueRow>(
            "[Scheduling].[usp_ShiftFatigue_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue>> ListAsync(ShiftFatigueSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List ShiftFatigue via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<ShiftFatigueRow>(
            "[Scheduling].[usp_ShiftFatigue_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ShiftFatigue {ShiftFatigueId}", aggregate.ShiftFatigueId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ShiftFatigueId", aggregate.ShiftFatigueId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EmployeeId", aggregate.EmployeeId)
            .AddString("ShiftPattern", aggregate.ShiftPattern, 400)
            .AddInt("ConsecutiveNightShifts", aggregate.ConsecutiveNightShifts)
            .AddDecimal("OvertimeHours", aggregate.OvertimeHours)
            .AddDecimal("FatigueRiskScore", aggregate.FatigueRiskScore)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_ShiftFatigue_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ShiftFatigue {ShiftFatigueId}", aggregate.ShiftFatigueId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ShiftFatigueId", aggregate.ShiftFatigueId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EmployeeId", aggregate.EmployeeId)
            .AddString("ShiftPattern", aggregate.ShiftPattern, 400)
            .AddInt("ConsecutiveNightShifts", aggregate.ConsecutiveNightShifts)
            .AddDecimal("OvertimeHours", aggregate.OvertimeHours)
            .AddDecimal("FatigueRiskScore", aggregate.FatigueRiskScore)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_ShiftFatigue_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Scheduling].[usp_ShiftFatigue_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ShiftFatigue {aggregate.ShiftFatigueId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ShiftFatigue"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue Hydrate(ShiftFatigueRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue>();

        AggregateReflectionHydrator.SetProperty(agg, "ShiftFatigueId", ShiftFatigueId.FromGuid(row.ShiftFatigueId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<ShiftFatigueStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "EmployeeId", row.EmployeeId);
        AggregateReflectionHydrator.SetProperty(agg, "ShiftPattern", row.ShiftPattern);
        AggregateReflectionHydrator.SetProperty(agg, "ConsecutiveNightShifts", row.ConsecutiveNightShifts);
        AggregateReflectionHydrator.SetProperty(agg, "OvertimeHours", row.OvertimeHours);
        AggregateReflectionHydrator.SetProperty(agg, "FatigueRiskScore", row.FatigueRiskScore);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IShiftFatigueDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ShiftFatigue_GetById</c>/<c>usp_ShiftFatigue_GetAllActive</c>.</summary>
    private sealed record ShiftFatigueRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated by
        /// <c>usp_ShiftFatigue_GetById</c>. Not consumed by aggregate hydration -- callers obtain
        /// the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer ShiftFatigueItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ShiftFatigueId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid EmployeeId { get; init; }
        public required string ShiftPattern { get; init; }
        public required int ConsecutiveNightShifts { get; init; }
        public required decimal OvertimeHours { get; init; }
        public required decimal FatigueRiskScore { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
