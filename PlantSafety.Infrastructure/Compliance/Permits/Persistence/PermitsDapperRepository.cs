// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Infrastructure
//  File:           PermitsDapperRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPermitsRepository (Domain repository
//                  interface for the Permits aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Permits exposes only a private "for
//                  rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied PermitsSpecification in-memory over the full active
//                  candidate set, since the interface carries no tenant/site scoping parameter to
//                  push down to SQL (mirrors PermitWorkflowDapperRepository / AccidentDapperRepository).
//                  Both AddAsync AND UpdateAsync enqueue domain events to the outbox after their
//                  write -- a platform-wide bug fixed elsewhere in this program was exactly
//                  UpdateAsync forgetting to do this, so it is deliberately present here from day
//                  one. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag from day one (this aggregate had no prior write path to
//                  retrofit). Mirrors PermitWorkflowDapperRepository.cs / AccidentDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Permits.Aggregates;
using PlantSafety.Domain.Compliance.Permits.Enums;
using PlantSafety.Domain.Compliance.Permits.Events;
using PlantSafety.Domain.Compliance.Permits.Interfaces;
using PlantSafety.Domain.Compliance.Permits.Specifications;
using PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.Permits.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Permits"/> aggregate root.</summary>
public sealed class PermitsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitsDapperRepository> logger)
    : IPermitsRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits?> GetByIdAsync(PermitsId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Permits {PermitsId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitsId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitsRow>(
            "[Compliance].[usp_Permits_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits>> ListAsync(PermitsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Permits via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PermitsRow>(
            "[Compliance].[usp_Permits_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Permits {PermitsId}", aggregate.PermitsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitsId", aggregate.PermitsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitType", aggregate.PermitType, 400)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddString("WorkDescription", aggregate.WorkDescription, 2000)
            .AddString("Location", aggregate.Location, 400)
            .AddString("HazardsIdentified", aggregate.HazardsIdentified, 2000)
            .AddGuid("RequestedByUserId", aggregate.RequestedByUserId.Value)
            .AddGuid("ApprovedByUserId", aggregate.ApprovedByUserId?.Value)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddString("RejectedReason", aggregate.RejectedReason, 2000)
            .AddDateTime("RejectedAtUtc", aggregate.RejectedAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddDateTime("ExpiredAtUtc", aggregate.ExpiredAtUtc?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_Permits_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Permits {PermitsId}", aggregate.PermitsId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitsId", aggregate.PermitsId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitType", aggregate.PermitType, 400)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddString("WorkDescription", aggregate.WorkDescription, 2000)
            .AddString("Location", aggregate.Location, 400)
            .AddString("HazardsIdentified", aggregate.HazardsIdentified, 2000)
            .AddGuid("RequestedByUserId", aggregate.RequestedByUserId.Value)
            .AddGuid("ApprovedByUserId", aggregate.ApprovedByUserId?.Value)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddString("RejectedReason", aggregate.RejectedReason, 2000)
            .AddDateTime("RejectedAtUtc", aggregate.RejectedAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddDateTime("ExpiredAtUtc", aggregate.ExpiredAtUtc?.Value)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_Permits_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_Permits_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Permits {aggregate.PermitsId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Approve, Reject, Complete, Expire, Close,
        // Archive) would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="Permits"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.Permits.Aggregates.Permits Hydrate(PermitsRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.PermitsId), PermitsId.FromGuid(row.PermitsId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.Status), Enum.Parse<PermitsStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.PermitType), row.PermitType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.PermitNumber), row.PermitNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.WorkDescription), row.WorkDescription);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.HazardsIdentified), row.HazardsIdentified);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.RequestedByUserId), new UserId(row.RequestedByUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.ApprovedByUserId), row.ApprovedByUserId.HasValue ? new UserId(row.ApprovedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.IssuedAtUtc), row.IssuedAtUtc.HasValue ? UtcTimestamp.From(row.IssuedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.ExpiresAtUtc), row.ExpiresAtUtc.HasValue ? UtcTimestamp.From(row.ExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.RejectedReason), row.RejectedReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.RejectedAtUtc), row.RejectedAtUtc.HasValue ? UtcTimestamp.From(row.RejectedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.ExpiredAtUtc), row.ExpiredAtUtc.HasValue ? UtcTimestamp.From(row.ExpiredAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.ClosedAtUtc), row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPermitsDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Permits_GetById</c>/<c>usp_Permits_GetAllActive</c>.</summary>
    private sealed record PermitsRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_Permits_GetById</c>/<c>usp_Permits_GetAllActive</c>. Not consumed by
        /// aggregate hydration -- callers obtain the ETag for a subsequent write from a prior
        /// single-record read (see the Application-layer PermitsItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PermitsId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PermitType { get; init; }
        public required string PermitNumber { get; init; }
        public required string WorkDescription { get; init; }
        public required string Location { get; init; }
        public required string HazardsIdentified { get; init; }
        public required Guid RequestedByUserId { get; init; }
        public Guid? ApprovedByUserId { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public string? RejectedReason { get; init; }
        public DateTime? RejectedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public DateTime? ExpiredAtUtc { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
