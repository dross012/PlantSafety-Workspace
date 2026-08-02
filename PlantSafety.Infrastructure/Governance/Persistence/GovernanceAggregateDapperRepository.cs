// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Infrastructure
//  File:           GovernanceAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IGovernanceRepository (Domain repository
//                  interface for the Governance aggregate root). All DB access via stored
//                  procedures in the [Governance] schema, against
//                  [Governance].[GovernanceAggregate] (named to avoid ambiguity with the
//                  pre-existing, dead GovernanceStub.cs scaffold that lives in this same
//                  module). Aggregate reconstruction uses AggregateReflectionHydrator because
//                  Governance exposes only a private "for rehydration only" constructor and
//                  private-setter properties. Both AddAsync AND UpdateAsync enqueue domain
//                  events to the outbox after their write. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Governance.Aggregates;
using PlantSafety.Domain.Governance.Enums;
using PlantSafety.Domain.Governance.Events;
using PlantSafety.Domain.Governance.Repositories;
using PlantSafety.Domain.Governance.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Governance.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Governance"/> aggregate root.</summary>
public sealed class GovernanceAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<GovernanceAggregateDapperRepository> logger)
    : IGovernanceRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Governance.Aggregates.Governance?> GetByIdAsync(GovernanceId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Governance {GovernanceId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("GovernanceId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<GovernanceRow>(
            "[Governance].[usp_GovernanceAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Governance.Aggregates.Governance aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Governance {GovernanceId}", aggregate.GovernanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("GovernanceId", aggregate.GovernanceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("GovernanceArea", aggregate.GovernanceArea, 200)
            .AddGuid("PolicyReferenceId", aggregate.PolicyReferenceId)
            .AddInt("ReviewCycleMonths", aggregate.ReviewCycleMonths)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Governance].[usp_GovernanceAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Governance.Aggregates.Governance aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Governance {GovernanceId}", aggregate.GovernanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("GovernanceId", aggregate.GovernanceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("GovernanceArea", aggregate.GovernanceArea, 200)
            .AddGuid("PolicyReferenceId", aggregate.PolicyReferenceId)
            .AddInt("ReviewCycleMonths", aggregate.ReviewCycleMonths)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_GovernanceAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Governance].[usp_GovernanceAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Governance {aggregate.GovernanceId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="Governance"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Governance.Aggregates.Governance Hydrate(GovernanceRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Governance.Aggregates.Governance>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.GovernanceId), GovernanceId.FromGuid(row.GovernanceId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.Status), Enum.Parse<GovernanceStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.GovernanceArea), row.GovernanceArea);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.PolicyReferenceId), row.PolicyReferenceId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.ReviewCycleMonths), row.ReviewCycleMonths);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Governance.Aggregates.Governance.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IGovernanceDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_GovernanceAggregate_GetById</c>.</summary>
    private sealed record GovernanceRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_GovernanceAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid GovernanceId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string GovernanceArea { get; init; }
        public Guid? PolicyReferenceId { get; init; }
        public int ReviewCycleMonths { get; init; }
        public required Guid OwnerUserId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
