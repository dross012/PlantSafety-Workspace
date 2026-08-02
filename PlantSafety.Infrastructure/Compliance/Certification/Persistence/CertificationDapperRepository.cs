// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Infrastructure
//  File:           CertificationDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ICertificationRepository (Domain repository
//                  interface for the Certification aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema, against
//                  [Compliance].[CertificationAggregate] (named to avoid ambiguity with the
//                  pre-existing read-only category slices -- Contractor/DOT/EPA/IndustrySpecific/
//                  ISO14001/ISO45001/OSHACertifications/Training -- and the pre-existing dead
//                  CertificationRepository/CertificationReadModel scaffold that lives in this same
//                  namespace but is a different, unrelated class). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Certification exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Certification.Aggregates;
using PlantSafety.Domain.Compliance.Certification.Enums;
using PlantSafety.Domain.Compliance.Certification.Events;
using PlantSafety.Domain.Compliance.Certification.Interfaces;
using PlantSafety.Domain.Compliance.Certification.Specifications;
using PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.Certification.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Certification"/> aggregate root.</summary>
public sealed class CertificationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CertificationDapperRepository> logger)
    : ICertificationRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification?> GetByIdAsync(CertificationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Certification {CertificationId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CertificationId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<CertificationRow>(
            "[Compliance].[usp_CertificationAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification>> ListAsync(CertificationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Certification via specification {Specification}", specification.GetType().Name);

        // No bulk read stored procedure exists for this aggregate yet (only single-record
        // GetById per this build-out's reduced scope); ListAsync returns an empty result until a
        // bulk-read proc is introduced.
        await Task.CompletedTask;
        return Array.Empty<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification>();
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Certification {CertificationId}", aggregate.CertificationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CertificationId", aggregate.CertificationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("HolderWorkerId", aggregate.HolderWorkerId.Value)
            .AddString("CertificationName", aggregate.CertificationName, 400)
            .AddString("CertificationBody", aggregate.CertificationBody, 400)
            .AddString("CertificationNumber", aggregate.CertificationNumber, 200)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddBool("RenewalRequired", aggregate.RenewalRequired)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_CertificationAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Certification {CertificationId}", aggregate.CertificationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CertificationId", aggregate.CertificationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("HolderWorkerId", aggregate.HolderWorkerId.Value)
            .AddString("CertificationName", aggregate.CertificationName, 400)
            .AddString("CertificationBody", aggregate.CertificationBody, 400)
            .AddString("CertificationNumber", aggregate.CertificationNumber, 200)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddBool("RenewalRequired", aggregate.RenewalRequired)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_CertificationAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_CertificationAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Certification {aggregate.CertificationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="Certification"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.Certification.Aggregates.Certification Hydrate(CertificationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CertificationId), CertificationId.FromGuid(row.CertificationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.Status), Enum.Parse<CertificationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.HolderWorkerId), new UserId(row.HolderWorkerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CertificationName), row.CertificationName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CertificationBody), row.CertificationBody);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CertificationNumber), row.CertificationNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.IssuedAtUtc), UtcTimestamp.From(row.IssuedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.ExpiresAtUtc), row.ExpiresAtUtc.HasValue ? UtcTimestamp.From(row.ExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.RenewalRequired), row.RenewalRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICertificationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_CertificationAggregate_GetById</c>.</summary>
    private sealed record CertificationRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_CertificationAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid CertificationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid HolderWorkerId { get; init; }
        public required string CertificationName { get; init; }
        public required string CertificationBody { get; init; }
        public required string CertificationNumber { get; init; }
        public required DateTime IssuedAtUtc { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public bool RenewalRequired { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
