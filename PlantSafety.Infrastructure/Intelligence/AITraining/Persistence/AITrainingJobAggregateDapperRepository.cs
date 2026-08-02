// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Infrastructure
//  File:           AITrainingJobAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  PlantSafety.Domain.Intelligence.AITraining.Interfaces.IAITrainingJobRepository
//                  for the AITrainingJob aggregate root. All DB access via stored procedures in
//                  the [Intelligence] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because AITrainingJob exposes only a private "for
//                  rehydration only" constructor and private-setter properties. GetByIdAsync is
//                  tenant-scoped and UpdateAsync enforces optimistic concurrency via @ETag from day
//                  one. Both AddAsync AND UpdateAsync enqueue domain events to the outbox after
//                  their write. Named "AITrainingJobAggregateDapperRepository" (rather than
//                  "AITrainingDapperRepository") to avoid colliding with the pre-existing read-list
//                  AITrainingDapperRepository already declared in the sibling Persistence
//                  namespace (backs the unrelated, untouched date-range
//                  GetAITrainingQuery/usp_AITraining_GetTrainingJobs read path). Mirrors
//                  PlantSafety.Infrastructure.ContinuousImprovement.RCA.Persistence
//                  .RCAAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.AITraining.Enums;
using PlantSafety.Domain.Intelligence.AITraining.Events;
using PlantSafety.Domain.Intelligence.AITraining.Interfaces;
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using AITrainingJobAggregate = PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob;

namespace PlantSafety.Infrastructure.Intelligence.AITraining.Persistence;

/// <summary>Dapper-backed repository for the <see cref="AITrainingJobAggregate"/> aggregate root.</summary>
public sealed class AITrainingJobAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AITrainingJobAggregateDapperRepository> logger)
    : IAITrainingJobRepository
{
    /// <inheritdoc/>
    public async Task<AITrainingJobAggregate?> GetByIdAsync(AITrainingJobId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AITrainingJob {AITrainingJobId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AITrainingJobId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AITrainingJobRow>(
            "[Intelligence].[usp_AITrainingJob_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(AITrainingJobAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AITrainingJob {AITrainingJobId}", aggregate.AITrainingJobId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AITrainingJobId", aggregate.AITrainingJobId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("ModelName", aggregate.ModelName, 200)
            .AddString("ModelVersion", aggregate.ModelVersion, 50)
            .AddString("DatasetReference", aggregate.DatasetReference, 400)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc.Value)
            .AddGuid("InitiatedBy", aggregate.InitiatedBy.Value)
            .AddGuid("RunStartedBy", aggregate.RunStartedBy?.Value)
            .AddDateTime("RunStartedAtUtc", aggregate.RunStartedAtUtc?.Value)
            .AddString("ModelArtifactReference", aggregate.ModelArtifactReference, 400)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, size: -1)
            .AddDateTime("FailedAtUtc", aggregate.FailedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_AITrainingJob_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AITrainingJobAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AITrainingJob {AITrainingJobId}", aggregate.AITrainingJobId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AITrainingJobId", aggregate.AITrainingJobId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("ModelName", aggregate.ModelName, 200)
            .AddString("ModelVersion", aggregate.ModelVersion, 50)
            .AddString("DatasetReference", aggregate.DatasetReference, 400)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("RunStartedBy", aggregate.RunStartedBy?.Value)
            .AddDateTime("RunStartedAtUtc", aggregate.RunStartedAtUtc?.Value)
            .AddString("ModelArtifactReference", aggregate.ModelArtifactReference, 400)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, size: -1)
            .AddDateTime("FailedAtUtc", aggregate.FailedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("ModifiedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_AITrainingJob_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_AITrainingJob_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"AITrainingJob {aggregate.AITrainingJobId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="AITrainingJobAggregate"/> aggregate from its flat read-model row.</summary>
    private static AITrainingJobAggregate Hydrate(AITrainingJobRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<AITrainingJobAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.AITrainingJobId), AITrainingJobId.FromGuid(row.AITrainingJobId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.Status), Enum.Parse<AITrainingJobStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.ModelName), row.ModelName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.ModelVersion), row.ModelVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.DatasetReference), row.DatasetReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.StartedAtUtc), UtcTimestamp.From(row.StartedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.InitiatedBy), new UserId(row.InitiatedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.RunStartedBy), row.RunStartedBy.HasValue ? new UserId(row.RunStartedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.RunStartedAtUtc), row.RunStartedAtUtc.HasValue ? UtcTimestamp.From(row.RunStartedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.ModelArtifactReference), row.ModelArtifactReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.FailureReason), row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.FailedAtUtc), row.FailedAtUtc.HasValue ? UtcTimestamp.From(row.FailedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AITrainingJobAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAITrainingJobDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AITrainingJob_GetById</c>.</summary>
    private sealed record AITrainingJobRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_AITrainingJob_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read (see the
        /// Application-layer AITrainingJobDetailDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid AITrainingJobId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
        public string? DatasetReference { get; init; }
        public required string Status { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public required Guid InitiatedBy { get; init; }
        public Guid? RunStartedBy { get; init; }
        public DateTime? RunStartedAtUtc { get; init; }
        public string? ModelArtifactReference { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? FailedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
