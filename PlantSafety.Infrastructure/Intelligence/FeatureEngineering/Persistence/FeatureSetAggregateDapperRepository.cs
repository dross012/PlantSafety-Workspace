// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Infrastructure
//  File:           FeatureSetAggregateDapperRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer write-side
//                  IFeatureSetRepository for the FeatureSet entity. All DB access via stored
//                  procedures in the [Intelligence] schema. Entity reconstruction uses
//                  AggregateReflectionHydrator because FeatureSet exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Feature names are
//                  persisted as a comma-delimited FeatureNamesCsv column (see FeatureSet.sql
//                  remarks) and split/joined here at the persistence boundary -- the Domain layer
//                  never sees the CSV representation. GetByIdAsync is tenant-scoped and
//                  UpdateAsync enforces optimistic concurrency via @ETag. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write. Mirrors
//                  PlantSafety.Infrastructure.Intelligence.DataLake.Persistence
//                  .DataLakeActivityAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Events;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using FeatureSetEntity = PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet;

namespace PlantSafety.Infrastructure.Intelligence.FeatureEngineering.Persistence;

/// <summary>Dapper-backed repository for the <see cref="FeatureSetEntity"/> entity.</summary>
public sealed class FeatureSetAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<FeatureSetAggregateDapperRepository> logger)
    : IFeatureSetRepository
{
    private const char FeatureNameDelimiter = ',';

    /// <inheritdoc/>
    public async Task<FeatureSetEntity?> GetByIdAsync(FeatureSetId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for FeatureSet {FeatureSetId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FeatureSetId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<FeatureSetRow>(
            "[Intelligence].[usp_FeatureSet_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<FeatureSetEntity?> GetLatestPublishedAsync(string name, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Name", name, 200)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<FeatureSetRow>(
            "[Intelligence].[usp_FeatureSet_GetLatestPublishedByName]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureSetEntity>> GetPublishedByNameAsync(string name, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Name", name, 200)
            .Build();
        var rows = await queryExecutor.QueryAsync<FeatureSetRow>(
            "[Intelligence].[usp_FeatureSet_GetPublishedByName]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(FeatureSetEntity featureSet, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add FeatureSet {FeatureSetId}", featureSet.FeatureSetId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FeatureSetId", featureSet.FeatureSetId.Value)
            .AddGuid("TenantId", featureSet.TenantId)
            .AddGuid("SiteId", featureSet.SiteId)
            .AddString("Name", featureSet.Name, 200)
            .AddString("Version", featureSet.Version.Value, 16)
            .AddString("DataSourcePath", featureSet.DataSourcePath, 500)
            .AddBool("IsPublished", featureSet.IsPublished)
            .AddString("FeatureNamesCsv", ToCsv(featureSet), size: -1)
            .AddDateTime("ComputedAtUtc", featureSet.ComputedAtUtc.Value)
            .AddDateTime("PublishedAtUtc", featureSet.PublishedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", featureSet.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", featureSet.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Intelligence].[usp_FeatureSet_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in featureSet.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, featureSet.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        featureSet.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(FeatureSetEntity featureSet, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update FeatureSet {FeatureSetId}", featureSet.FeatureSetId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("FeatureSetId", featureSet.FeatureSetId.Value)
            .AddGuid("TenantId", featureSet.TenantId)
            .AddBool("IsPublished", featureSet.IsPublished)
            .AddString("FeatureNamesCsv", ToCsv(featureSet), size: -1)
            .AddDateTime("PublishedAtUtc", featureSet.PublishedAtUtc?.Value)
            .AddGuid("ModifiedBy", featureSet.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_FeatureSet_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Intelligence].[usp_FeatureSet_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"FeatureSet {featureSet.FeatureSetId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in featureSet.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, featureSet.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        featureSet.ClearDomainEvents();
    }

    private static string ToCsv(FeatureSetEntity featureSet) =>
        string.Join(FeatureNameDelimiter, featureSet.Features.Select(f => f.Value));

    /// <summary>Reconstructs a <see cref="FeatureSetEntity"/> from its flat read-model row.</summary>
    private static FeatureSetEntity Hydrate(FeatureSetRow row)
    {
        var entity = AggregateReflectionHydrator.CreateUninitialized<FeatureSetEntity>();

        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.FeatureSetId), FeatureSetId.FromGuid(row.FeatureSetId));
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.Version), FeatureVersion.From(row.Version));
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.DataSourcePath), row.DataSourcePath);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.IsPublished), row.IsPublished);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.ComputedAtUtc), UtcTimestamp.From(row.ComputedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(entity, nameof(FeatureSetEntity.CreatedBy), new UserId(row.CreatedBy));

        var features = (row.FeatureNamesCsv ?? string.Empty)
            .Split(FeatureNameDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(FeatureName.From)
            .ToList();
        AggregateReflectionHydrator.SetField(entity, "_features", features);
        AggregateReflectionHydrator.SetField(entity, "_domainEvents", new List<IFeatureEngineeringDomainEvent>());

        return entity;
    }

    /// <summary>Flat read-model row shape returned by the FeatureSet read stored procedures.</summary>
    private sealed record FeatureSetRow
    {
        public string? ETag { get; init; }

        public required Guid FeatureSetId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Name { get; init; }
        public required string Version { get; init; }
        public required string DataSourcePath { get; init; }
        public required bool IsPublished { get; init; }
        public string? FeatureNamesCsv { get; init; }
        public required DateTime ComputedAtUtc { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? ModifiedAtUtc { get; init; }
        public Guid? ModifiedBy { get; init; }
    }
}
