// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           ManagementOfChangeDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IManagementOfChangeRepository (Domain repository
//                  interface for the ManagementOfChange aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because ManagementOfChange exposes only a private
//                  "for rehydration only" constructor and private-setter properties. This file
//                  fully-qualifies every reference to the ManagementOfChange aggregate type because
//                  the namespace segment "ManagementOfChange" would otherwise be ambiguous with the
//                  type of the same name -- mirrors PermitsDapperRepository.cs's identical
//                  precedent. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag from day one (this aggregate had no prior write
//                  path to retrofit).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.ManagementOfChange.Enums;
using PlantSafety.Domain.Compliance.ManagementOfChange.Events;
using PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;
using PlantSafety.Domain.Compliance.ManagementOfChange.Specifications;
using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.Persistence;

/// <summary>Dapper-backed repository for the ManagementOfChange aggregate root.</summary>
public sealed class ManagementOfChangeDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ManagementOfChangeDapperRepository> logger)
    : IManagementOfChangeRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange?> GetByIdAsync(ManagementOfChangeId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ManagementOfChange {ManagementOfChangeId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ManagementOfChangeId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ManagementOfChangeRow>(
            "[Compliance].[usp_ManagementOfChange_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange>> ListAsync(ManagementOfChangeSpecification specification, CancellationToken cancellationToken = default)
    {
        // No bulk "get all active" proc exists yet for this aggregate -- ListAsync is not
        // exercised by any Application-layer command/query in this pass (only GetByIdAsync is).
        logger.LogDebug("List ManagementOfChange via specification {Specification}", specification.GetType().Name);
        return Task.FromResult<IReadOnlyList<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange>>(Array.Empty<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange>());
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ManagementOfChange {ManagementOfChangeId}", aggregate.ManagementOfChangeId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ManagementOfChangeId", aggregate.ManagementOfChangeId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ChangeTitle", aggregate.ChangeTitle, 400)
            .AddString("ChangeType", aggregate.ChangeType, 100)
            .AddString("Justification", aggregate.Justification, 2000)
            .AddString("RiskLevel", aggregate.RiskLevel, 50)
            .AddGuid("RequestedByUserId", aggregate.RequestedByUserId.Value)
            .AddGuid("ApprovedByUserId", aggregate.ApprovedByUserId?.Value)
            .AddString("ApprovalStatus", aggregate.ApprovalStatus, 50)
            .AddDateTime("ImplementationAtUtc", aggregate.ImplementationAtUtc?.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_ManagementOfChange_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ManagementOfChange {ManagementOfChangeId}", aggregate.ManagementOfChangeId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ManagementOfChangeId", aggregate.ManagementOfChangeId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ChangeTitle", aggregate.ChangeTitle, 400)
            .AddString("ChangeType", aggregate.ChangeType, 100)
            .AddString("Justification", aggregate.Justification, 2000)
            .AddString("RiskLevel", aggregate.RiskLevel, 50)
            .AddGuid("RequestedByUserId", aggregate.RequestedByUserId.Value)
            .AddGuid("ApprovedByUserId", aggregate.ApprovedByUserId?.Value)
            .AddString("ApprovalStatus", aggregate.ApprovalStatus, 50)
            .AddDateTime("ImplementationAtUtc", aggregate.ImplementationAtUtc?.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_ManagementOfChange_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_ManagementOfChange_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ManagementOfChange {aggregate.ManagementOfChangeId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a ManagementOfChange aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange Hydrate(ManagementOfChangeRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ManagementOfChangeId), ManagementOfChangeId.FromGuid(row.ManagementOfChangeId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.Status), Enum.Parse<ManagementOfChangeStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ChangeTitle), row.ChangeTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ChangeType), row.ChangeType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.Justification), row.Justification);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.RiskLevel), row.RiskLevel);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.RequestedByUserId), new UserId(row.RequestedByUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ApprovedByUserId), row.ApprovedByUserId.HasValue ? new UserId(row.ApprovedByUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ApprovalStatus), row.ApprovalStatus);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ImplementationAtUtc), row.ImplementationAtUtc.HasValue ? UtcTimestamp.From(row.ImplementationAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.ReviewDueAtUtc), row.ReviewDueAtUtc.HasValue ? UtcTimestamp.From(row.ReviewDueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IManagementOfChangeDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ManagementOfChange_GetById</c>.</summary>
    private sealed record ManagementOfChangeRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated by
        /// <c>usp_ManagementOfChange_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer ManagementOfChangeItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ManagementOfChangeId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ChangeTitle { get; init; }
        public required string ChangeType { get; init; }
        public required string Justification { get; init; }
        public required string RiskLevel { get; init; }
        public required Guid RequestedByUserId { get; init; }
        public Guid? ApprovedByUserId { get; init; }
        public required string ApprovalStatus { get; init; }
        public DateTime? ImplementationAtUtc { get; init; }
        public DateTime? ReviewDueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
