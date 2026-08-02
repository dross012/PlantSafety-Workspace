// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAiGovernanceRepository (Domain repository
//                  interface for the AiGovernance aggregate root). All DB access via stored
//                  procedures in the [AiGovernance] schema, against
//                  [AiGovernance].[AiGovernanceAggregate] (named to avoid ambiguity with the
//                  pre-existing, unrelated Features.AiGovernance "GovernedModel" registry that
//                  lives under the same [AiGovernance] schema and is live-wired into DI). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because AiGovernance exposes
//                  only a private constructor and private-setter properties. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. GetByIdAsync
//                  is tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag.
//                  Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.AiGovernance.Aggregates;
using PlantSafety.Domain.AiGovernance.Enums;
using PlantSafety.Domain.AiGovernance.Events;
using PlantSafety.Domain.AiGovernance.Repositories;
using PlantSafety.Domain.AiGovernance.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.AiGovernance.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance"/> aggregate root.</summary>
public sealed class AiGovernanceAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AiGovernanceAggregateDapperRepository> logger)
    : IAiGovernanceRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance?> GetByIdAsync(AiGovernanceId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AiGovernance {AiGovernanceId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiGovernanceId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AiGovernanceRow>(
            "[AiGovernance].[usp_AiGovernanceAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AiGovernance {AiGovernanceId}", aggregate.AiGovernanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiGovernanceId", aggregate.AiGovernanceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SystemName", aggregate.SystemName, 200)
            .AddString("AiUseCase", aggregate.AiUseCase, 2000)
            .AddString("RiskClassification", aggregate.RiskClassification, 50)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddBool("RequiresHumanOversight", aggregate.RequiresHumanOversight)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[AiGovernance].[usp_AiGovernanceAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AiGovernance {AiGovernanceId}", aggregate.AiGovernanceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiGovernanceId", aggregate.AiGovernanceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("SystemName", aggregate.SystemName, 200)
            .AddString("AiUseCase", aggregate.AiUseCase, 2000)
            .AddString("RiskClassification", aggregate.RiskClassification, 50)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddBool("RequiresHumanOversight", aggregate.RequiresHumanOversight)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_AiGovernanceAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[AiGovernance].[usp_AiGovernanceAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"AiGovernance {aggregate.AiGovernanceId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance Hydrate(AiGovernanceRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.AiGovernanceId), AiGovernanceId.FromGuid(row.AiGovernanceId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.Status), Enum.Parse<AiGovernanceStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.SystemName), row.SystemName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.AiUseCase), row.AiUseCase);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.RiskClassification), row.RiskClassification);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.RequiresHumanOversight), row.RequiresHumanOversight);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAiGovernanceDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AiGovernanceAggregate_GetById</c>.</summary>
    private sealed record AiGovernanceRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_AiGovernanceAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid AiGovernanceId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string SystemName { get; init; }
        public required string AiUseCase { get; init; }
        public required string RiskClassification { get; init; }
        public required Guid OwnerUserId { get; init; }
        public bool RequiresHumanOversight { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
