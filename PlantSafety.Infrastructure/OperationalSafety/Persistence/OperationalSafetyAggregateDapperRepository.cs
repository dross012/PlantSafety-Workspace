// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IOperationalSafetyRepository (Domain
//                  repository interface for the OperationalSafety aggregate root). All DB
//                  access via stored procedures in the [OperationalSafety] schema, against
//                  [OperationalSafety].[OperationalSafetyAggregate] (named to avoid ambiguity
//                  with the pre-existing, dead OperationalSafetyStub.cs scaffold and the
//                  pre-existing, unrelated, LIVE OperationalSafety.Alerts/Checklists/
//                  PPEManagement/RiskManagement/Schedules repositories that live under this
//                  same module folder). Aggregate reconstruction uses AggregateReflectionHydrator
//                  because OperationalSafety exposes only a private "for rehydration only"
//                  constructor and private-setter properties. Both AddAsync AND UpdateAsync
//                  enqueue domain events to the outbox after their write. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag.
//                  Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.OperationalSafety.Aggregates;
using PlantSafety.Domain.OperationalSafety.Enums;
using PlantSafety.Domain.OperationalSafety.Events;
using PlantSafety.Domain.OperationalSafety.Repositories;
using PlantSafety.Domain.OperationalSafety.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.OperationalSafety.Persistence;

/// <summary>Dapper-backed repository for the <see cref="OperationalSafety"/> aggregate root.</summary>
public sealed class OperationalSafetyAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<OperationalSafetyAggregateDapperRepository> logger)
    : IOperationalSafetyRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety?> GetByIdAsync(OperationalSafetyId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for OperationalSafety {OperationalSafetyId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OperationalSafetyId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<OperationalSafetyRow>(
            "[OperationalSafety].[usp_OperationalSafetyAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add OperationalSafety {OperationalSafetyId}", aggregate.OperationalSafetyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OperationalSafetyId", aggregate.OperationalSafetyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SafetyDomain", aggregate.SafetyDomain, 200)
            .AddString("RiskLevel", aggregate.RiskLevel, 50)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[OperationalSafety].[usp_OperationalSafetyAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update OperationalSafety {OperationalSafetyId}", aggregate.OperationalSafetyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OperationalSafetyId", aggregate.OperationalSafetyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SafetyDomain", aggregate.SafetyDomain, 200)
            .AddString("RiskLevel", aggregate.RiskLevel, 50)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_OperationalSafetyAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[OperationalSafety].[usp_OperationalSafetyAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"OperationalSafety {aggregate.OperationalSafetyId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="OperationalSafety"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety Hydrate(OperationalSafetyRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.OperationalSafetyId), OperationalSafetyId.FromGuid(row.OperationalSafetyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.Status), Enum.Parse<OperationalSafetyStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.SafetyDomain), row.SafetyDomain);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.RiskLevel), row.RiskLevel);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.ReviewDueAtUtc), row.ReviewDueAtUtc.HasValue ? UtcTimestamp.From(row.ReviewDueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IOperationalSafetyDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_OperationalSafetyAggregate_GetById</c>.</summary>
    private sealed record OperationalSafetyRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_OperationalSafetyAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid OperationalSafetyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string SafetyDomain { get; init; }
        public required string RiskLevel { get; init; }
        public required Guid OwnerUserId { get; init; }
        public DateTime? ReviewDueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
