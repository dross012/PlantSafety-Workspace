// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Infrastructure
//  File:           AuditingAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAuditingRepository (Domain repository
//                  interface for the Auditing aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema, against [Compliance].[AuditingAggregate]
//                  (named to avoid ambiguity with the pre-existing read-only category slices --
//                  AuditFindings/CAPATriggers/ComplianceAudit/InternalSafetyAudit -- and the
//                  pre-existing dead AuditingRepository/AuditingReadModel scaffold that lives in
//                  this same namespace but is a different, unrelated class). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because Auditing exposes only a
//                  private "for rehydration only" constructor and private-setter properties. Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Auditing.Aggregates;
using PlantSafety.Domain.Compliance.Auditing.Enums;
using PlantSafety.Domain.Compliance.Auditing.Events;
using PlantSafety.Domain.Compliance.Auditing.Interfaces;
using PlantSafety.Domain.Compliance.Auditing.Specifications;
using PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.Auditing.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Auditing"/> aggregate root.</summary>
public sealed class AuditingAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AuditingAggregateDapperRepository> logger)
    : IAuditingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing?> GetByIdAsync(AuditingId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Auditing {AuditingId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditingId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AuditingRow>(
            "[Compliance].[usp_AuditingAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing>> ListAsync(AuditingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Auditing via specification {Specification}", specification.GetType().Name);

        // No bulk read stored procedure exists for this aggregate yet (only single-record
        // GetById per this build-out's reduced scope); ListAsync returns an empty result until a
        // bulk-read proc is introduced. Mirrors the interface contract without fabricating a
        // stored procedure this build-out was not asked to create.
        await Task.CompletedTask;
        return Array.Empty<PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing>();
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Auditing {AuditingId}", aggregate.AuditingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditingId", aggregate.AuditingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("AuditTitle", aggregate.AuditTitle, 400)
            .AddString("AuditType", aggregate.AuditType, 100)
            .AddString("Scope", aggregate.Scope, 2000)
            .AddGuid("AuditorId", aggregate.AuditorId.Value)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddInt("FindingsCount", aggregate.FindingsCount)
            .AddDecimal("Score", aggregate.Score)
            .AddString("ComplianceRating", aggregate.ComplianceRating, 50)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_AuditingAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Auditing {AuditingId}", aggregate.AuditingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditingId", aggregate.AuditingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("AuditTitle", aggregate.AuditTitle, 400)
            .AddString("AuditType", aggregate.AuditType, 100)
            .AddString("Scope", aggregate.Scope, 2000)
            .AddGuid("AuditorId", aggregate.AuditorId.Value)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddInt("FindingsCount", aggregate.FindingsCount)
            .AddDecimal("Score", aggregate.Score)
            .AddString("ComplianceRating", aggregate.ComplianceRating, 50)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_AuditingAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_AuditingAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Auditing {aggregate.AuditingId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="Auditing"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing Hydrate(AuditingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.AuditingId), AuditingId.FromGuid(row.AuditingId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.Status), Enum.Parse<AuditingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.AuditTitle), row.AuditTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.AuditType), row.AuditType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.Scope), row.Scope);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.AuditorId), new UserId(row.AuditorId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.ScheduledAtUtc), row.ScheduledAtUtc.HasValue ? UtcTimestamp.From(row.ScheduledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.FindingsCount), row.FindingsCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.Score), row.Score);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.ComplianceRating), row.ComplianceRating);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAuditingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AuditingAggregate_GetById</c>.</summary>
    private sealed record AuditingRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_AuditingAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid AuditingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string AuditTitle { get; init; }
        public required string AuditType { get; init; }
        public required string Scope { get; init; }
        public required Guid AuditorId { get; init; }
        public DateTime? ScheduledAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public int FindingsCount { get; init; }
        public decimal? Score { get; init; }
        public required string ComplianceRating { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
