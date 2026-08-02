// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Infrastructure
//  File:           CrisisManagementAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer (write-side)
//                  ICrisisManagementRepository (PlantSafety.Domain.SafetyEvents.CrisisManagement
//                  .Interfaces.ICrisisManagementRepository) for the CrisisManagement aggregate
//                  root. All DB access via stored procedures in the [SafetyEvents] schema.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because
//                  CrisisManagement exposes only a private "for rehydration only" constructor and
//                  private-setter properties. GetByIdAsync is tenant-scoped and UpdateAsync
//                  enforces optimistic concurrency via @ETag from day one. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. Named
//                  distinctly from the pre-existing CrisisManagementDapperRepository (the
//                  read-only list/by-id repository implementing the Application-layer
//                  ICrisisManagementRepository) to avoid a same-simple-name class collision within
//                  this same namespace. Mirrors PredictiveMaintenanceAlertDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Enums;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
// Type alias required: this file's own namespace (PlantSafety.Infrastructure.SafetyEvents
// .CrisisManagement.Persistence) contains a "CrisisManagement" segment, so an unqualified
// "CrisisManagement" identifier resolves to that namespace segment, not the aggregate type,
// producing CS0118. Mirrors the same disambiguation need documented on the Domain-layer
// ICrisisManagementRepository interface.
using CrisisManagementAggregate = PlantSafety.Domain.SafetyEvents.CrisisManagement.Aggregates.CrisisManagement;

namespace PlantSafety.Infrastructure.SafetyEvents.CrisisManagement.Persistence;

/// <summary>Dapper-backed repository for the <see cref="CrisisManagementAggregate"/> aggregate root (write side).</summary>
public sealed class CrisisManagementAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CrisisManagementAggregateDapperRepository> logger)
    : ICrisisManagementRepository
{
    /// <inheritdoc/>
    public async Task<CrisisManagementAggregate?> GetByIdAsync(CrisisManagementId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CrisisManagement {CrisisManagementId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CrisisManagementId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<CrisisManagementRow>(
            "[SafetyEvents].[usp_CrisisManagement_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(CrisisManagementAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add CrisisManagement {CrisisManagementId}", aggregate.CrisisManagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CrisisManagementId", aggregate.CrisisManagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Title", aggregate.Title, 200)
            .AddString("Severity", aggregate.Severity, 50)
            .AddDateTime("DeclaredAtUtc", aggregate.DeclaredAtUtc.Value)
            .AddGuid("RespondedBy", aggregate.RespondedBy?.Value)
            .AddDateTime("RespondedAtUtc", aggregate.RespondedAtUtc?.Value)
            .AddDateTime("StabilizedAtUtc", aggregate.StabilizedAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_CrisisManagement_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(CrisisManagementAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update CrisisManagement {CrisisManagementId}", aggregate.CrisisManagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CrisisManagementId", aggregate.CrisisManagementId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("Title", aggregate.Title, 200)
            .AddString("Severity", aggregate.Severity, 50)
            .AddDateTime("DeclaredAtUtc", aggregate.DeclaredAtUtc.Value)
            .AddGuid("RespondedBy", aggregate.RespondedBy?.Value)
            .AddDateTime("RespondedAtUtc", aggregate.RespondedAtUtc?.Value)
            .AddDateTime("StabilizedAtUtc", aggregate.StabilizedAtUtc?.Value)
            .AddGuid("ResolvedBy", aggregate.ResolvedBy?.Value)
            .AddDateTime("ResolvedAtUtc", aggregate.ResolvedAtUtc?.Value)
            .AddGuid("ClosedBy", aggregate.ClosedBy?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_CrisisManagement_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_CrisisManagement_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"CrisisManagement {aggregate.CrisisManagementId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="CrisisManagementAggregate"/> from its flat read-model row.</summary>
    private static CrisisManagementAggregate Hydrate(CrisisManagementRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CrisisManagementAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.CrisisManagementId), CrisisManagementId.FromGuid(row.CrisisManagementId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.Status), Enum.Parse<CrisisManagementStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.Title), row.Title);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.Severity), row.Severity);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.DeclaredAtUtc), UtcTimestamp.From(row.DeclaredAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.RespondedBy), row.RespondedBy.HasValue ? new UserId(row.RespondedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.RespondedAtUtc), row.RespondedAtUtc.HasValue ? UtcTimestamp.From(row.RespondedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.StabilizedAtUtc), row.StabilizedAtUtc.HasValue ? UtcTimestamp.From(row.StabilizedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.ResolvedBy), row.ResolvedBy.HasValue ? new UserId(row.ResolvedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.ResolvedAtUtc), row.ResolvedAtUtc.HasValue ? UtcTimestamp.From(row.ResolvedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.ClosedBy), row.ClosedBy.HasValue ? new UserId(row.ClosedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(CrisisManagementAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICrisisManagementDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_CrisisManagement_GetById</c>.</summary>
    private sealed record CrisisManagementRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_CrisisManagement_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer CrisisManagementDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid CrisisManagementId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string Title { get; init; }
        public required string Severity { get; init; }
        public required DateTime DeclaredAtUtc { get; init; }
        public Guid? RespondedBy { get; init; }
        public DateTime? RespondedAtUtc { get; init; }
        public DateTime? StabilizedAtUtc { get; init; }
        public Guid? ResolvedBy { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
