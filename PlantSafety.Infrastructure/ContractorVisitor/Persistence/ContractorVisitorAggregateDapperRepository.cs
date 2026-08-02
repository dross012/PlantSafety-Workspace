// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IContractorVisitorRepository (Domain repository
//                  interface for the ContractorVisitor aggregate root). All DB access via stored
//                  procedures in the [ContractorVisitor] schema, against
//                  [ContractorVisitor].[ContractorVisitorAggregate]. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because ContractorVisitor exposes only a private
//                  constructor and private-setter properties. Both AddAsync AND UpdateAsync
//                  enqueue domain events to the outbox after their write. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.ContractorVisitor.Aggregates;
using PlantSafety.Domain.ContractorVisitor.Enums;
using PlantSafety.Domain.ContractorVisitor.Events;
using PlantSafety.Domain.ContractorVisitor.Repositories;
using PlantSafety.Domain.ContractorVisitor.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.ContractorVisitor.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor"/> aggregate root.</summary>
public sealed class ContractorVisitorAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ContractorVisitorAggregateDapperRepository> logger)
    : IContractorVisitorRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor?> GetByIdAsync(ContractorVisitorId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ContractorVisitor {ContractorVisitorId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ContractorVisitorId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ContractorVisitorRow>(
            "[ContractorVisitor].[usp_ContractorVisitorAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ContractorVisitor {ContractorVisitorId}", aggregate.ContractorVisitorId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ContractorVisitorId", aggregate.ContractorVisitorId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FullName", aggregate.FullName, 200)
            .AddString("CompanyName", aggregate.CompanyName, 200)
            .AddString("VisitorType", aggregate.VisitorType, 50)
            .AddString("PurposeOfVisit", aggregate.PurposeOfVisit, 2000)
            .AddGuid("HostUserId", aggregate.HostUserId.Value)
            .AddDateTime("ScheduledArrivalUtc", aggregate.ScheduledArrivalUtc?.Value)
            .AddDateTime("ActualArrivalUtc", aggregate.ActualArrivalUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ContractorVisitor].[usp_ContractorVisitorAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ContractorVisitor {ContractorVisitorId}", aggregate.ContractorVisitorId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ContractorVisitorId", aggregate.ContractorVisitorId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("FullName", aggregate.FullName, 200)
            .AddString("CompanyName", aggregate.CompanyName, 200)
            .AddString("VisitorType", aggregate.VisitorType, 50)
            .AddString("PurposeOfVisit", aggregate.PurposeOfVisit, 2000)
            .AddGuid("HostUserId", aggregate.HostUserId.Value)
            .AddDateTime("ScheduledArrivalUtc", aggregate.ScheduledArrivalUtc?.Value)
            .AddDateTime("ActualArrivalUtc", aggregate.ActualArrivalUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_ContractorVisitorAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[ContractorVisitor].[usp_ContractorVisitorAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ContractorVisitor {aggregate.ContractorVisitorId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor Hydrate(ContractorVisitorRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.ContractorVisitorId), ContractorVisitorId.FromGuid(row.ContractorVisitorId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.Status), Enum.Parse<ContractorVisitorStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.FullName), row.FullName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.CompanyName), row.CompanyName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.VisitorType), row.VisitorType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.PurposeOfVisit), row.PurposeOfVisit);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.HostUserId), new UserId(row.HostUserId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.ScheduledArrivalUtc), row.ScheduledArrivalUtc.HasValue ? UtcTimestamp.From(row.ScheduledArrivalUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.ActualArrivalUtc), row.ActualArrivalUtc.HasValue ? UtcTimestamp.From(row.ActualArrivalUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IContractorVisitorDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ContractorVisitorAggregate_GetById</c>.</summary>
    private sealed record ContractorVisitorRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_ContractorVisitorAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ContractorVisitorId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string FullName { get; init; }
        public required string CompanyName { get; init; }
        public required string VisitorType { get; init; }
        public required string PurposeOfVisit { get; init; }
        public required Guid HostUserId { get; init; }
        public DateTime? ScheduledArrivalUtc { get; init; }
        public DateTime? ActualArrivalUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
