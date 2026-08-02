// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProceduresRepository (Domain repository
//                  interface for the Procedures aggregate root). All DB access via stored
//                  procedures in the [Compliance] schema, against the
//                  [Compliance].[ProceduresAggregate] table (distinct from the pre-existing
//                  [Compliance].[Procedures*] category tables). Aggregate reconstruction uses
//                  AggregateReflectionHydrator because Procedures exposes only a private "for
//                  rehydration only" constructor and private-setter properties. This file
//                  fully-qualifies every reference to the Procedures aggregate type because the
//                  namespace segment "Procedures" would otherwise be ambiguous with the type of the
//                  same name, and the class is named "ProceduresAggregateDapperRepository" (not
//                  "ProceduresDapperRepository") to avoid colliding with the pre-existing,
//                  unregistered ProceduresRepository orphan class in this same namespace. Mirrors
//                  PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Procedures.Enums;
using PlantSafety.Domain.Compliance.Procedures.Events;
using PlantSafety.Domain.Compliance.Procedures.Interfaces;
using PlantSafety.Domain.Compliance.Procedures.Specifications;
using PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.Procedures.Persistence;

/// <summary>Dapper-backed repository for the Procedures aggregate root.</summary>
public sealed class ProceduresAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ProceduresAggregateDapperRepository> logger)
    : IProceduresRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures?> GetByIdAsync(ProceduresId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Procedures {ProceduresId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProceduresId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ProceduresRow>(
            "[Compliance].[usp_Procedures_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures>> ListAsync(ProceduresSpecification specification, CancellationToken cancellationToken = default)
    {
        // No bulk "get all active" proc exists yet for this aggregate -- ListAsync is not
        // exercised by any Application-layer command/query in this pass (only GetByIdAsync is).
        logger.LogDebug("List Procedures via specification {Specification}", specification.GetType().Name);
        return Task.FromResult<IReadOnlyList<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures>>(Array.Empty<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures>());
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Procedures {ProceduresId}", aggregate.ProceduresId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProceduresId", aggregate.ProceduresId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ProcedureTitle", aggregate.ProcedureTitle, 400)
            .AddString("ProcedureNumber", aggregate.ProcedureNumber, 400)
            .AddString("Category", aggregate.Category, 100)
            .AddInt("RevisionNumber", aggregate.RevisionNumber)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_Procedures_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Procedures {ProceduresId}", aggregate.ProceduresId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProceduresId", aggregate.ProceduresId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ProcedureTitle", aggregate.ProcedureTitle, 400)
            .AddString("ProcedureNumber", aggregate.ProcedureNumber, 400)
            .AddString("Category", aggregate.Category, 100)
            .AddInt("RevisionNumber", aggregate.RevisionNumber)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddDateTime("ReviewDueAtUtc", aggregate.ReviewDueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_Procedures_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_Procedures_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Procedures {aggregate.ProceduresId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a Procedures aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures Hydrate(ProceduresRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.ProceduresId), ProceduresId.FromGuid(row.ProceduresId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.Status), Enum.Parse<ProceduresStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.ProcedureTitle), row.ProcedureTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.ProcedureNumber), row.ProcedureNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.RevisionNumber), row.RevisionNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.EffectiveAtUtc), row.EffectiveAtUtc.HasValue ? UtcTimestamp.From(row.EffectiveAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.ReviewDueAtUtc), row.ReviewDueAtUtc.HasValue ? UtcTimestamp.From(row.ReviewDueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IProceduresDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Procedures_GetById</c>.</summary>
    private sealed record ProceduresRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated by
        /// <c>usp_Procedures_GetById</c>. Not consumed by aggregate hydration -- callers obtain the
        /// ETag for a subsequent write from a prior single-record read (see the Application-layer
        /// ProceduresItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ProceduresId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ProcedureTitle { get; init; }
        public required string ProcedureNumber { get; init; }
        public required string Category { get; init; }
        public required int RevisionNumber { get; init; }
        public required Guid OwnerId { get; init; }
        public DateTime? EffectiveAtUtc { get; init; }
        public DateTime? ReviewDueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
