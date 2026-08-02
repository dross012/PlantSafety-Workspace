// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IHumanFactorsRepository (Domain repository
//                  interface for the HumanFactors aggregate root). All DB access via stored
//                  procedures in the [HumanFactors] schema, against
//                  [HumanFactors].[HumanFactorsAggregate] (named to avoid ambiguity with the
//                  pre-existing, dead HumanFactorsStub.cs/Ergonomics/FatigueManagement
//                  scaffolding that lives in this same module). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because HumanFactors exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Both AddAsync
//                  AND UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.HumanFactors.Aggregates;
using PlantSafety.Domain.HumanFactors.Enums;
using PlantSafety.Domain.HumanFactors.Events;
using PlantSafety.Domain.HumanFactors.Repositories;
using PlantSafety.Domain.HumanFactors.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.HumanFactors.Persistence;

/// <summary>Dapper-backed repository for the <see cref="HumanFactors"/> aggregate root.</summary>
public sealed class HumanFactorsAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HumanFactorsAggregateDapperRepository> logger)
    : IHumanFactorsRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors?> GetByIdAsync(HumanFactorsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HumanFactors {HumanFactorsId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HumanFactorsId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HumanFactorsRow>(
            "[HumanFactors].[usp_HumanFactorsAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add HumanFactors {HumanFactorsId}", aggregate.HumanFactorsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HumanFactorsId", aggregate.HumanFactorsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FactorType", aggregate.FactorType, 100)
            .AddGuid("AffectedUserId", aggregate.AffectedUserId?.Value)
            .AddInt("SeverityRating", aggregate.SeverityRating)
            .AddString("Description", aggregate.Description, 2000)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[HumanFactors].[usp_HumanFactorsAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update HumanFactors {HumanFactorsId}", aggregate.HumanFactorsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HumanFactorsId", aggregate.HumanFactorsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FactorType", aggregate.FactorType, 100)
            .AddGuid("AffectedUserId", aggregate.AffectedUserId?.Value)
            .AddInt("SeverityRating", aggregate.SeverityRating)
            .AddString("Description", aggregate.Description, 2000)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_HumanFactorsAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[HumanFactors].[usp_HumanFactorsAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"HumanFactors {aggregate.HumanFactorsId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="HumanFactors"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors Hydrate(HumanFactorsRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.HumanFactorsId), HumanFactorsId.FromGuid(row.HumanFactorsId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.Status), Enum.Parse<HumanFactorsStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.FactorType), row.FactorType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.AffectedUserId), row.AffectedUserId.HasValue ? new UserId(row.AffectedUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.SeverityRating), row.SeverityRating);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IHumanFactorsDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_HumanFactorsAggregate_GetById</c>.</summary>
    private sealed record HumanFactorsRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_HumanFactorsAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid HumanFactorsId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string FactorType { get; init; }
        public Guid? AffectedUserId { get; init; }
        public int SeverityRating { get; init; }
        public required string Description { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
