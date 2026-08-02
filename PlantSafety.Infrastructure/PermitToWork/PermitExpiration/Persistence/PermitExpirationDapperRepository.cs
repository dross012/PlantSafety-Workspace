// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Infrastructure
//  File:           PermitExpirationDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPermitExpirationRepository (Domain repository
//                  interface for the PermitExpiration aggregate root). All DB access via stored
//                  procedures in the [PermitToWork] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PermitExpiration exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an @ETag optimistic-concurrency token
//                  (Gap 2 remediation) and invokes usp_PermitExpiration_Update via
//                  ExecuteScalarAsync (not ExecuteAsync) so the proc's rows-affected SELECT can
//                  be read back; zero rows affected throws ConcurrencyConflictException. Mirrors
//                  IncidentDapperRepository.UpdateAsync. PermitExpirationRow gains an ETag field,
//                  populated by usp_PermitExpiration_GetById's RowVersion projection.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates;
using PlantSafety.Domain.PermitToWork.PermitExpiration.Enums;
using PlantSafety.Domain.PermitToWork.PermitExpiration.Events;
using PlantSafety.Domain.PermitToWork.PermitExpiration.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitExpiration.Specifications;
using PlantSafety.Domain.PermitToWork.PermitExpiration.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PermitToWork.PermitExpiration.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PermitExpiration"/> aggregate root.</summary>
public sealed class PermitExpirationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitExpirationDapperRepository> logger)
    : IPermitExpirationRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration?> GetByIdAsync(PermitExpirationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PermitExpiration {PermitExpirationId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PermitExpirationId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitExpirationRow>(
            "[PermitToWork].[usp_PermitExpiration_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration>> ListAsync(PermitExpirationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PermitExpiration via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PermitExpirationRow>(
            "[PermitToWork].[usp_PermitExpiration_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PermitExpiration {PermitExpirationId}", aggregate.PermitExpirationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitExpirationId", aggregate.PermitExpirationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddBool("ExtensionRequested", aggregate.ExtensionRequested)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitExpiration_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PermitExpiration {PermitExpirationId}", aggregate.PermitExpirationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitExpirationId", aggregate.PermitExpirationId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddBool("ExtensionRequested", aggregate.ExtensionRequested)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PermitExpiration_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PermitToWork].[usp_PermitExpiration_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PermitExpiration {aggregate.PermitExpirationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. Previously only AddAsync did this, so every
        // post-creation transition silently dropped its domain event. See AddAsync remarks
        // for the outbox flush consistency gap.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PermitExpiration"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration Hydrate(PermitExpirationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration>();

        AggregateReflectionHydrator.SetProperty(agg, "PermitExpirationId", PermitExpirationId.FromGuid(row.PermitExpirationId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PermitExpirationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PermitNumber", row.PermitNumber);
        AggregateReflectionHydrator.SetProperty(agg, "IssuedAtUtc", row.IssuedAtUtc.HasValue ? UtcTimestamp.From(row.IssuedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ExpiresAtUtc", row.ExpiresAtUtc.HasValue ? UtcTimestamp.From(row.ExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ExtensionRequested", row.ExtensionRequested);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPermitExpirationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PermitExpiration_GetById</c>/<c>usp_PermitExpiration_GetAllActive</c>.</summary>
    private sealed record PermitExpirationRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PermitExpiration_GetById</c>. Not populated by <c>usp_PermitExpiration_GetAllActive</c>
        /// (list projection is unchanged); mutation callers obtain the ETag from a prior
        /// single-record read or from the PermitExpiration read-model (<c>PermitExpirationItemDto.ETag</c>).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PermitExpirationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PermitNumber { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public required bool ExtensionRequested { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
