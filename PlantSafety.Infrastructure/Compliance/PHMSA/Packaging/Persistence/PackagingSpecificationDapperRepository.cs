// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Infrastructure
//  File:           PackagingSpecificationDapperRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPackagingSpecificationRepository (Domain
//                  repository interface for the PackagingSpecification aggregate root). All DB
//                  access via stored procedures in the [Compliance] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator because PackagingSpecification
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. Both AddAsync AND UpdateAsync enqueue domain events to the outbox
//                  after their write -- a platform-wide bug fixed elsewhere in this program was
//                  exactly UpdateAsync forgetting to do this, so it is deliberately present here
//                  from day one. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag from day one. PerformanceTests is list-valued
//                  on the aggregate and is persisted as a JSON array string (see
//                  PHMSA_PackagingSpecification.sql). Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Events;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.PHMSA.Packaging.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PackagingSpecification"/> aggregate root.</summary>
public sealed class PackagingSpecificationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PackagingSpecificationDapperRepository> logger)
    : IPackagingSpecificationRepository
{
    /// <inheritdoc/>
    public async Task<PackagingSpecification?> GetByIdAsync(PackagingSpecificationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PackagingSpecification {PackagingSpecificationId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PackagingSpecificationId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PackagingSpecificationRow>(
            "[Compliance].[usp_PHMSAPackagingSpecification_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PackagingSpecification aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PackagingSpecification {PackagingSpecificationId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PackagingSpecificationId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("DOTSpec", aggregate.DOTSpec.Value, 20)
            .AddString("PerformanceTests", JsonSerializer.Serialize(aggregate.PerformanceTests), size: -1)
            .AddDecimal("MaxCapacity", aggregate.MaxCapacity)
            .AddString("CapacityUnit", aggregate.CapacityUnit, 10)
            .AddString("ClosureInstructions", aggregate.ClosureInstructions, size: -1)
            .AddString("ReuseRestrictions", aggregate.ReuseRestrictions, size: -1)
            .AddGuid("AssociatedMaterialId", aggregate.AssociatedMaterialId?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_PHMSAPackagingSpecification_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised by
        // the aggregate during this write so TransactionalUnitOfWork.CommitAsync (invoked by
        // TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PackagingSpecification aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PackagingSpecification {PackagingSpecificationId}", aggregate.Id.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PackagingSpecificationId", aggregate.Id.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("DOTSpec", aggregate.DOTSpec.Value, 20)
            .AddString("PerformanceTests", JsonSerializer.Serialize(aggregate.PerformanceTests), size: -1)
            .AddDecimal("MaxCapacity", aggregate.MaxCapacity)
            .AddString("CapacityUnit", aggregate.CapacityUnit, 10)
            .AddString("ClosureInstructions", aggregate.ClosureInstructions, size: -1)
            .AddString("ReuseRestrictions", aggregate.ReuseRestrictions, size: -1)
            .AddGuid("AssociatedMaterialId", aggregate.AssociatedMaterialId?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PHMSAPackagingSpecification_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_PHMSAPackagingSpecification_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PackagingSpecification {aggregate.Id.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Supersede, Retire) would silently drop its
        // domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PackagingSpecification"/> aggregate from its flat read-model row.</summary>
    private static PackagingSpecification Hydrate(PackagingSpecificationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PackagingSpecification>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.Id), PackagingSpecificationId.FromGuid(row.PackagingSpecificationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.Status), Enum.Parse<PackagingSpecificationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.DOTSpec), new DOTSpec(row.DOTSpec));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.PerformanceTests), (IReadOnlyList<string>)(JsonSerializer.Deserialize<List<string>>(row.PerformanceTests) ?? []));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.MaxCapacity), row.MaxCapacity);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.CapacityUnit), row.CapacityUnit);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.ClosureInstructions), row.ClosureInstructions);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.ReuseRestrictions), row.ReuseRestrictions);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.AssociatedMaterialId), row.AssociatedMaterialId.HasValue ? HazmatMaterialId.FromGuid(row.AssociatedMaterialId.Value) : (HazmatMaterialId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PackagingSpecification.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPackagingSpecificationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by the PackagingSpecification stored procedures.</summary>
    private sealed record PackagingSpecificationRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string). Not
        /// consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer
        /// PackagingSpecificationDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PackagingSpecificationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DOTSpec { get; init; }
        public required string PerformanceTests { get; init; }
        public required decimal MaxCapacity { get; init; }
        public required string CapacityUnit { get; init; }
        public required string ClosureInstructions { get; init; }
        public string? ReuseRestrictions { get; init; }
        public Guid? AssociatedMaterialId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
