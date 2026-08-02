// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Infrastructure
//  File:           PMSchedulingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPMSchedulingRepository (Domain repository
//                  interface for the PMScheduling aggregate root). All DB access via stored
//                  procedures in the [Scheduling] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PMScheduling exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Scheduling.PMScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PMScheduling.Enums;
using PlantSafety.Domain.Scheduling.PMScheduling.Events;
using PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PMScheduling.Specifications;
using PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Scheduling.PMScheduling.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PMScheduling"/> aggregate root.</summary>
public sealed class PMSchedulingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PMSchedulingDapperRepository> logger)
    : IPMSchedulingRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling?> GetByIdAsync(PMSchedulingId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PMScheduling {PMSchedulingId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("PMSchedulingId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PMSchedulingRow>(
            "[Scheduling].[usp_PMScheduling_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling>> ListAsync(PMSchedulingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PMScheduling via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PMSchedulingRow>(
            "[Scheduling].[usp_PMScheduling_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PMScheduling {PMSchedulingId}", aggregate.PMSchedulingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PMSchedulingId", aggregate.PMSchedulingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("EquipmentDescription", aggregate.EquipmentDescription, 400)
            .AddString("PmTaskType", aggregate.PmTaskType, 400)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc?.Value)
            .AddBool("IsOverdue", aggregate.IsOverdue)
            .AddString("EscalationPriorityCode", aggregate.EscalationPriorityCode, 400)
            .AddString("CompletionNotes", aggregate.CompletionNotes, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Scheduling].[usp_PMScheduling_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PMScheduling {PMSchedulingId}", aggregate.PMSchedulingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PMSchedulingId", aggregate.PMSchedulingId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("EquipmentId", aggregate.EquipmentId)
            .AddString("EquipmentDescription", aggregate.EquipmentDescription, 400)
            .AddString("PmTaskType", aggregate.PmTaskType, 400)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc?.Value)
            .AddBool("IsOverdue", aggregate.IsOverdue)
            .AddString("EscalationPriorityCode", aggregate.EscalationPriorityCode, 400)
            .AddString("CompletionNotes", aggregate.CompletionNotes, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_PMScheduling_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Scheduling].[usp_PMScheduling_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PMScheduling {aggregate.PMSchedulingId.Value} was modified by another process or the ETag is stale.", Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PMScheduling"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling Hydrate(PMSchedulingRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling>();

        AggregateReflectionHydrator.SetProperty(agg, "PMSchedulingId", PMSchedulingId.FromGuid(row.PMSchedulingId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PMSchedulingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "EquipmentId", row.EquipmentId);
        AggregateReflectionHydrator.SetProperty(agg, "EquipmentDescription", row.EquipmentDescription);
        AggregateReflectionHydrator.SetProperty(agg, "PmTaskType", row.PmTaskType);
        AggregateReflectionHydrator.SetProperty(agg, "DueAtUtc", row.DueAtUtc.HasValue ? UtcTimestamp.From(row.DueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "IsOverdue", row.IsOverdue);
        AggregateReflectionHydrator.SetProperty(agg, "EscalationPriorityCode", row.EscalationPriorityCode);
        AggregateReflectionHydrator.SetProperty(agg, "CompletionNotes", row.CompletionNotes);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPMSchedulingDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PMScheduling_GetById</c>/<c>usp_PMScheduling_GetAllActive</c>.</summary>
    private sealed record PMSchedulingRow
    {
        public required Guid PMSchedulingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid EquipmentId { get; init; }
        public required string EquipmentDescription { get; init; }
        public required string PmTaskType { get; init; }
        public DateTime? DueAtUtc { get; init; }
        public required bool IsOverdue { get; init; }
        public string? EscalationPriorityCode { get; init; }
        public string? CompletionNotes { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }

        /// <summary>
        /// Optimistic-concurrency token, projected from the row's native RowVersion column. Not
        /// consumed by aggregate hydration (the PMScheduling aggregate has no ETag concept) --
        /// present so Dapper's column-mapping of usp_PMScheduling_GetById's ETag column does not
        /// fail; mirrors RiskDapperRepository.RiskRow.ETag's identical precedent.
        /// </summary>
        public string? ETag { get; init; }
    }
}
