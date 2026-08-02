// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligenceAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAiSafetyIntelligenceRepository (Domain
//                  repository interface for the AiSafetyIntelligence aggregate root). All DB
//                  access via stored procedures in the [AiSafetyIntelligence] schema, against
//                  [AiSafetyIntelligence].[AiSafetyIntelligenceAggregate]. Aggregate reconstruction
//                  uses AggregateReflectionHydrator because AiSafetyIntelligence exposes only a
//                  private constructor and private-setter properties. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. GetByIdAsync
//                  is tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag.
//                  Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.AiSafetyIntelligence.Aggregates;
using PlantSafety.Domain.AiSafetyIntelligence.Enums;
using PlantSafety.Domain.AiSafetyIntelligence.Events;
using PlantSafety.Domain.AiSafetyIntelligence.Repositories;
using PlantSafety.Domain.AiSafetyIntelligence.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.AiSafetyIntelligence.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence"/> aggregate root.</summary>
public sealed class AiSafetyIntelligenceAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AiSafetyIntelligenceAggregateDapperRepository> logger)
    : IAiSafetyIntelligenceRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence?> GetByIdAsync(AiSafetyIntelligenceId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AiSafetyIntelligence {AiSafetyIntelligenceId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiSafetyIntelligenceId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AiSafetyIntelligenceRow>(
            "[AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AiSafetyIntelligence {AiSafetyIntelligenceId}", aggregate.AiSafetyIntelligenceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiSafetyIntelligenceId", aggregate.AiSafetyIntelligenceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("InsightType", aggregate.InsightType, 50)
            .AddString("Description", aggregate.Description, 2000)
            .AddDecimal("ConfidenceScore", aggregate.ConfidenceScore)
            .AddString("SourceModelName", aggregate.SourceModelName, 200)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AiSafetyIntelligence {AiSafetyIntelligenceId}", aggregate.AiSafetyIntelligenceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AiSafetyIntelligenceId", aggregate.AiSafetyIntelligenceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("InsightType", aggregate.InsightType, 50)
            .AddString("Description", aggregate.Description, 2000)
            .AddDecimal("ConfidenceScore", aggregate.ConfidenceScore)
            .AddString("SourceModelName", aggregate.SourceModelName, 200)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_AiSafetyIntelligenceAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[AiSafetyIntelligence].[usp_AiSafetyIntelligenceAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"AiSafetyIntelligence {aggregate.AiSafetyIntelligenceId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence Hydrate(AiSafetyIntelligenceRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.AiSafetyIntelligenceId), AiSafetyIntelligenceId.FromGuid(row.AiSafetyIntelligenceId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.Status), Enum.Parse<AiSafetyIntelligenceStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.InsightType), row.InsightType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.ConfidenceScore), row.ConfidenceScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.SourceModelName), row.SourceModelName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAiSafetyIntelligenceDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AiSafetyIntelligenceAggregate_GetById</c>.</summary>
    private sealed record AiSafetyIntelligenceRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_AiSafetyIntelligenceAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid AiSafetyIntelligenceId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string InsightType { get; init; }
        public required string Description { get; init; }
        public decimal ConfidenceScore { get; init; }
        public required string SourceModelName { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
