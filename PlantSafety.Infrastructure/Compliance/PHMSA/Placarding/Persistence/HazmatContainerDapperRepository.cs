// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Infrastructure
//  File:           HazmatContainerDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IHazmatContainerRepository (Domain repository
//                  interface for the HazmatContainer aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema (usp_PHMSAHazmatContainer_*). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because HazmatContainer exposes
//                  only a private "for rehydration only" constructor and private-setter
//                  properties. Markings/Labels/Placards are stored as NVARCHAR(MAX) JSON string
//                  arrays and (de)serialized with System.Text.Json. Both AddAsync AND UpdateAsync
//                  enqueue domain events to the outbox after their write (platform-wide fix,
//                  UpdateAsync must not skip this). GetByIdAsync is tenant-scoped and UpdateAsync
//                  enforces optimistic concurrency via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Placarding.Persistence;

/// <summary>Dapper-backed repository for the <see cref="HazmatContainer"/> aggregate root.</summary>
public sealed class HazmatContainerDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazmatContainerDapperRepository> logger)
    : IHazmatContainerRepository
{
    /// <inheritdoc/>
    public async Task<HazmatContainer?> GetByIdAsync(HazmatContainerId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazmatContainer {HazmatContainerId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatContainerId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazmatContainerRow>(
            "[Compliance].[usp_PHMSAHazmatContainer_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(HazmatContainer aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add HazmatContainer {HazmatContainerId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatContainerId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("ContainerType", aggregate.ContainerType.ToString(), 30)
            .AddGuid("MaterialId", aggregate.MaterialId.Value)
            .AddGuid("PackagingSpecId", aggregate.PackagingSpecId.Value)
            .AddString("Markings", JsonSerializer.Serialize(aggregate.Markings), size: -1)
            .AddString("Labels", JsonSerializer.Serialize(aggregate.Labels), size: -1)
            .AddString("Placards", JsonSerializer.Serialize(aggregate.Placards), size: -1)
            .AddBool("OrientationArrowsRequired", aggregate.OrientationArrowsRequired)
            .AddString("CompatibilityGroup", aggregate.CompatibilityGroup, 5)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_PHMSAHazmatContainer_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(HazmatContainer aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update HazmatContainer {HazmatContainerId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazmatContainerId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("ContainerType", aggregate.ContainerType.ToString(), 30)
            .AddGuid("MaterialId", aggregate.MaterialId.Value)
            .AddGuid("PackagingSpecId", aggregate.PackagingSpecId.Value)
            .AddString("Markings", JsonSerializer.Serialize(aggregate.Markings), size: -1)
            .AddString("Labels", JsonSerializer.Serialize(aggregate.Labels), size: -1)
            .AddString("Placards", JsonSerializer.Serialize(aggregate.Placards), size: -1)
            .AddBool("OrientationArrowsRequired", aggregate.OrientationArrowsRequired)
            .AddString("CompatibilityGroup", aggregate.CompatibilityGroup, 5)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_PHMSAHazmatContainer_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_PHMSAHazmatContainer_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"HazmatContainer {aggregate.Id.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="HazmatContainer"/> aggregate from its flat read-model row.</summary>
    private static HazmatContainer Hydrate(HazmatContainerRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<HazmatContainer>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.Id), HazmatContainerId.FromGuid(row.HazmatContainerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.Status), Enum.Parse<HazmatContainerStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.ContainerType), Enum.Parse<ContainerType>(row.ContainerType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.MaterialId), HazmatMaterialId.FromGuid(row.MaterialId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.PackagingSpecId), PackagingSpecificationId.FromGuid(row.PackagingSpecId));
        AggregateReflectionHydrator.SetField(agg, "_markings", JsonSerializer.Deserialize<List<string>>(row.Markings) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_labels", JsonSerializer.Deserialize<List<string>>(row.Labels) ?? []);
        AggregateReflectionHydrator.SetField(agg, "_placards", JsonSerializer.Deserialize<List<string>>(row.Placards) ?? []);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.OrientationArrowsRequired), row.OrientationArrowsRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.CompatibilityGroup), row.CompatibilityGroup);
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(HazmatContainer.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IHazmatContainerDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PHMSAHazmatContainer_GetById</c>/<c>_GetAllActive</c>/<c>_GetByMaterialId</c>.</summary>
    private sealed record HazmatContainerRow
    {
        /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string). Not consumed by aggregate hydration.</summary>
        public string? ETag { get; init; }

        public required Guid HazmatContainerId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ContainerType { get; init; }
        public required Guid MaterialId { get; init; }
        public required Guid PackagingSpecId { get; init; }
        public required string Markings { get; init; }
        public required string Labels { get; init; }
        public required string Placards { get; init; }
        public required bool OrientationArrowsRequired { get; init; }
        public string? CompatibilityGroup { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
