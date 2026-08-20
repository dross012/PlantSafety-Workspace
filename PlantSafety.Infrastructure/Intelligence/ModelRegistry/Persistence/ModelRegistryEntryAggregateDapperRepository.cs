// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Infrastructure
//  File:           ModelRegistryEntryAggregateDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  IModelRegistryEntryRepository for the ModelRegistryEntry aggregate root. All DB
//                  access via stored procedures in the [Intelligence] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because ModelRegistryEntry
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag. Both AddAsync AND UpdateAsync enqueue domain events to
//                  the outbox after their write. Mirrors
//                  PlantSafety.Infrastructure.Intelligence.DataLake.Persistence
//                  .DataLakeActivityAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.ModelRegistry.Enums;
using PlantSafety.Domain.Intelligence.ModelRegistry.Events;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using ModelRegistryEntryAggregate = PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry;

namespace PlantSafety.Infrastructure.Intelligence.ModelRegistry.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ModelRegistryEntryAggregate"/> aggregate root.</summary>
public sealed class ModelRegistryEntryAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ModelRegistryEntryAggregateDapperRepository> logger)
    : IModelRegistryEntryRepository
{
    /// <inheritdoc/>
    public async Task<ModelRegistryEntryAggregate?> GetByIdAsync(ModelRegistryEntryId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ModelRegistryEntry {ModelRegistryEntryId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ModelRegistryEntryId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ModelRegistryEntryRow>(
            "[Intelligence].[usp_ModelRegistryEntry_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<ModelRegistryEntryAggregate?> GetByModelNameAndVersionAsync(string modelName, ModelVersion version, Guid tenantId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(version);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("ModelName", modelName, 200)
            .AddString("ModelVersion", version.Value, 32)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ModelRegistryEntryRow>(
            "[Intelligence].[usp_ModelRegistryEntry_GetByModelNameAndVersion]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModelRegistryEntryAggregate>> GetApprovedByModelNameAsync(string modelName, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("ModelName", modelName, 200)
            .Build();
        var rows = await queryExecutor.QueryAsync<ModelRegistryEntryRow>(
            "[Intelligence].[usp_ModelRegistryEntry_GetApprovedByModelName]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ModelRegistryEntryAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ModelRegistryEntry {ModelRegistryEntryId}", aggregate.ModelRegistryEntryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ModelRegistryEntryId", aggregate.ModelRegistryEntryId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("ModelName", aggregate.ModelName, 200)
            .AddString("ModelVersion", aggregate.Version.Value, 32)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DatasetHash", aggregate.DatasetHash, 64)
            .AddDouble("TrainingAccuracyScore", aggregate.TrainingAccuracyScore)
            .AddGuid("RegisteredBy", aggregate.RegisteredBy.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddString("DeprecationReason", aggregate.DeprecationReason, size: 1000)
            .AddDateTime("DeprecatedAtUtc", aggregate.DeprecatedAtUtc?.Value)
            .AddDateTime("RetiredAtUtc", aggregate.RetiredAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_ModelRegistryEntry_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ModelRegistryEntryAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update ModelRegistryEntry {ModelRegistryEntryId}", aggregate.ModelRegistryEntryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ModelRegistryEntryId", aggregate.ModelRegistryEntryId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddString("DeprecationReason", aggregate.DeprecationReason, size: 1000)
            .AddDateTime("DeprecatedAtUtc", aggregate.DeprecatedAtUtc?.Value)
            .AddDateTime("RetiredAtUtc", aggregate.RetiredAtUtc?.Value)
            .AddGuid("ModifiedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_ModelRegistryEntry_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_ModelRegistryEntry_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"ModelRegistryEntry {aggregate.ModelRegistryEntryId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ModelRegistryEntryAggregate"/> aggregate from its flat read-model row.</summary>
    private static ModelRegistryEntryAggregate Hydrate(ModelRegistryEntryRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ModelRegistryEntryAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.ModelRegistryEntryId), ModelRegistryEntryId.FromGuid(row.ModelRegistryEntryId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.Status), Enum.Parse<ModelRegistryEntryStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.ModelName), row.ModelName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.Version), ModelVersion.From(row.ModelVersion));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.DatasetHash), row.DatasetHash);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.TrainingAccuracyScore), row.TrainingAccuracyScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.RegisteredBy), new UserId(row.RegisteredBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.ApprovedBy), row.ApprovedBy.HasValue ? new UserId(row.ApprovedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.ApprovedAtUtc), row.ApprovedAtUtc.HasValue ? UtcTimestamp.From(row.ApprovedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.DeprecationReason), row.DeprecationReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.DeprecatedAtUtc), row.DeprecatedAtUtc.HasValue ? UtcTimestamp.From(row.DeprecatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.RetiredAtUtc), row.RetiredAtUtc.HasValue ? UtcTimestamp.From(row.RetiredAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ModelRegistryEntryAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IModelRegistryEntryDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by the ModelRegistryEntry read stored procedures.</summary>
    private sealed record ModelRegistryEntryRow
    {
        public string? ETag { get; init; }

        public required Guid ModelRegistryEntryId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
        public required string Status { get; init; }
        public required string DatasetHash { get; init; }
        public required double TrainingAccuracyScore { get; init; }
        public required Guid RegisteredBy { get; init; }
        public Guid? ApprovedBy { get; init; }
        public DateTime? ApprovedAtUtc { get; init; }
        public string? DeprecationReason { get; init; }
        public DateTime? DeprecatedAtUtc { get; init; }
        public DateTime? RetiredAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
