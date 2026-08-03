// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           ConcurrencyConflictDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IConcurrencyConflictRepository. All DB access
//                  via stored procedures in the [Auditing] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ConcurrencyConflictRecord"/> aggregate root.</summary>
public sealed class ConcurrencyConflictDapperRepository(
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    ISqlConnectionFactory connectionFactory,
    IOutboxWriter outboxWriter,
    ILogger<ConcurrencyConflictDapperRepository> logger)
    : IConcurrencyConflictRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(ConcurrencyConflictRecord record, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Recording concurrency conflict {ConcurrencyConflictRecordId} for {EntityType} {EntityId}",
            record.ConcurrencyConflictRecordId.Value, record.EntityType, record.EntityId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ConcurrencyConflictRecordId", record.ConcurrencyConflictRecordId.Value)
            .AddGuid("TenantId", record.TenantId)
            .AddString("EntityType", record.EntityType, 200)
            .AddGuid("EntityId", record.EntityId)
            .AddString("ExpectedETag", record.ExpectedETag, 64)
            .AddString("ActualETag", record.ActualETag, 64)
            .AddString("AttemptedOperation", record.AttemptedOperation, 100)
            .AddGuid("ActorId", record.ActorId)
            .AddDateTime("OccurredAtUtc", record.OccurredAtUtc.Value)
            .AddGuid("CorrelationId", record.CorrelationId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_ConcurrencyConflictRecord_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in record.DomainEvents)
            outboxWriter.Enqueue(domainEvent, record.TenantId.ToString(), "system", record.CorrelationId.ToString());
        record.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task<ConcurrencyConflictRecord?> GetByIdAsync(ConcurrencyConflictRecordId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ConcurrencyConflictRecordId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ConflictRow>(
            "[Auditing].[usp_ConcurrencyConflictRecord_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConcurrencyConflictRecord>> GetForEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("EntityType", entityType, 200)
            .AddGuid("EntityId", entityId)
            .Build();

        var rows = await queryExecutor.QueryAsync<ConflictRow>(
            "[Auditing].[usp_ConcurrencyConflictRecord_GetForEntity]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    private static ConcurrencyConflictRecord Hydrate(ConflictRow row)
    {
        var record = AggregateReflectionHydrator.CreateUninitialized<ConcurrencyConflictRecord>();

        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.ConcurrencyConflictRecordId), ConcurrencyConflictRecordId.FromGuid(row.ConcurrencyConflictRecordId));
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.EntityType), row.EntityType);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.EntityId), row.EntityId);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.ExpectedETag), row.ExpectedETag);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.ActualETag), row.ActualETag);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.AttemptedOperation), row.AttemptedOperation);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.ActorId), row.ActorId);
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.OccurredAtUtc), UtcTimestamp.From(row.OccurredAtUtc));
        AggregateReflectionHydrator.SetProperty(record, nameof(ConcurrencyConflictRecord.CorrelationId), row.CorrelationId);

        return record;
    }

    private sealed record ConflictRow
    {
        public required Guid ConcurrencyConflictRecordId { get; init; }
        public required Guid TenantId { get; init; }
        public required string EntityType { get; init; }
        public required Guid EntityId { get; init; }
        public required string ExpectedETag { get; init; }
        public string? ActualETag { get; init; }
        public required string AttemptedOperation { get; init; }
        public Guid? ActorId { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
        public required Guid CorrelationId { get; init; }
    }
}
