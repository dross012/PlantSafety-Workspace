// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ISustainabilityRepository (Domain repository
//                  interface for the Sustainability aggregate root). All DB access via stored
//                  procedures in the [Sustainability] schema, against
//                  [Sustainability].[SustainabilityAggregate] (named to avoid ambiguity with the
//                  pre-existing, unrelated Features-vertical [Sustainability].[Metric]
//                  table/ISustainabilityRepository that lives in this same module folder but is a
//                  different, unrelated concept -- named "SustainabilityAggregateDapperRepository"
//                  rather than bare "SustainabilityDapperRepository" per the module's
//                  aggregate/namespace name-collision precedent). Uses the aggregate type's fully
//                  qualified name throughout (rather than a bare "Sustainability" reference or a
//                  using-alias) because the enclosing Infrastructure namespace segment
//                  "PlantSafety.Infrastructure.Sustainability" itself binds the simple name
//                  "Sustainability" to a namespace, not the aggregate type, before any using-alias
//                  is considered (CS0118). Aggregate reconstruction uses AggregateReflectionHydrator
//                  because Sustainability exposes only a private "for rehydration only"
//                  constructor and private-setter properties. Both AddAsync AND UpdateAsync
//                  enqueue domain events to the outbox after their write. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag.
//                  Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Sustainability.Enums;
using PlantSafety.Domain.Sustainability.Events;
using PlantSafety.Domain.Sustainability.Repositories;
using PlantSafety.Domain.Sustainability.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Sustainability.Persistence;

/// <summary>Dapper-backed repository for the Sustainability aggregate root.</summary>
public sealed class SustainabilityAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SustainabilityAggregateDapperRepository> logger)
    : ISustainabilityRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Sustainability.Aggregates.Sustainability?> GetByIdAsync(SustainabilityId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Sustainability {SustainabilityId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SustainabilityId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<SustainabilityRow>(
            "[Sustainability].[usp_SustainabilityAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Sustainability {SustainabilityId}", aggregate.SustainabilityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SustainabilityId", aggregate.SustainabilityId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("InitiativeName", aggregate.InitiativeName, 400)
            .AddString("Category", aggregate.Category, 100)
            .AddDecimal("TargetValue", aggregate.TargetValue)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Sustainability].[usp_SustainabilityAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Sustainability {SustainabilityId}", aggregate.SustainabilityId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SustainabilityId", aggregate.SustainabilityId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("InitiativeName", aggregate.InitiativeName, 400)
            .AddString("Category", aggregate.Category, 100)
            .AddDecimal("TargetValue", aggregate.TargetValue)
            .AddGuid("OwnerUserId", aggregate.OwnerUserId.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_SustainabilityAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Sustainability].[usp_SustainabilityAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Sustainability {aggregate.SustainabilityId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a Sustainability aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Sustainability.Aggregates.Sustainability Hydrate(SustainabilityRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Sustainability.Aggregates.Sustainability>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.SustainabilityId), SustainabilityId.FromGuid(row.SustainabilityId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.Status), Enum.Parse<SustainabilityStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.InitiativeName), row.InitiativeName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.TargetValue), row.TargetValue);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.OwnerUserId), new UserId(row.OwnerUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Sustainability.Aggregates.Sustainability.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ISustainabilityDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_SustainabilityAggregate_GetById</c>.</summary>
    private sealed record SustainabilityRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_SustainabilityAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid SustainabilityId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string InitiativeName { get; init; }
        public required string Category { get; init; }
        public decimal? TargetValue { get; init; }
        public required Guid OwnerUserId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
