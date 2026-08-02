// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Infrastructure
//  File:           ProcessDefinitionAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProcessDefinitionRepository (Domain
//                  repository interface for the ProcessDefinition aggregate root). All DB access
//                  via stored procedures in the [ProcessManagement] schema, against
//                  [ProcessManagement].[ProcessDefinitionAggregate] (named to avoid ambiguity
//                  with the pre-existing, unrelated Features-vertical
//                  [ProcessManagement].[Processes] table/IProcessRepository that lives in this
//                  same module folder but is a different, unrelated concept). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because ProcessDefinition
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.Persistence.AuditingAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.ProcessManagement.Aggregates;
using PlantSafety.Domain.ProcessManagement.Enums;
using PlantSafety.Domain.ProcessManagement.Events;
using PlantSafety.Domain.ProcessManagement.Repositories;
using PlantSafety.Domain.ProcessManagement.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.ProcessManagement.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ProcessDefinition"/> aggregate root.</summary>
public sealed class ProcessDefinitionAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ProcessDefinitionAggregateDapperRepository> logger)
    : IProcessDefinitionRepository
{
    /// <inheritdoc/>
    public async Task<ProcessDefinition?> GetByIdAsync(ProcessDefinitionId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ProcessDefinition {ProcessDefinitionId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProcessDefinitionId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ProcessDefinitionRow>(
            "[ProcessManagement].[usp_ProcessDefinitionAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ProcessDefinition aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ProcessDefinition {ProcessDefinitionId}", aggregate.ProcessDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProcessDefinitionId", aggregate.ProcessDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("ProcessName", aggregate.ProcessName, 200)
            .AddString("Category", aggregate.Category, 100)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddInt("Version", aggregate.Version)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ProcessManagement].[usp_ProcessDefinitionAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProcessDefinition aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ProcessDefinition {ProcessDefinitionId}", aggregate.ProcessDefinitionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProcessDefinitionId", aggregate.ProcessDefinitionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("ProcessName", aggregate.ProcessName, 200)
            .AddString("Category", aggregate.Category, 100)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddInt("Version", aggregate.Version)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_ProcessDefinitionAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[ProcessManagement].[usp_ProcessDefinitionAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ProcessDefinition {aggregate.ProcessDefinitionId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ProcessDefinition"/> aggregate from its flat read-model row.</summary>
    private static ProcessDefinition Hydrate(ProcessDefinitionRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ProcessDefinition>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.ProcessDefinitionId), ProcessDefinitionId.FromGuid(row.ProcessDefinitionId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.ProcessName), row.ProcessName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.Status), Enum.Parse<ProcessDefinitionStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.Version), row.Version);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProcessDefinition.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IProcessDefinitionDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ProcessDefinitionAggregate_GetById</c>.</summary>
    private sealed record ProcessDefinitionRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_ProcessDefinitionAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ProcessDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ProcessName { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required int Version { get; init; }
        public required Guid OwnerId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
