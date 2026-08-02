// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Infrastructure
//  File:           ObservationWriteDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Domain-layer IObservationRepository
//                  (write-side aggregate repository). Named *WriteDapperRepository (not
//                  ObservationItemsDapperRepository, which already backs the read-only
//                  Application-layer IObservationRepository against
//                  usp_ObservationItems_GetObservations, nor ObservationDapperRepository, the
//                  unrelated Features-tree read model) to avoid an unqualified-name collision
//                  between the multiple distinct IObservationRepository interfaces in this
//                  codebase. All DB access via stored procedures against
//                  [SafetyEvents].[ObservationItems] (the same table backing the read-only list
//                  query). Aggregate reconstruction uses AggregateReflectionHydrator because
//                  Observation exposes only a private "for rehydration only" constructor and
//                  private-setter properties. ListAsync evaluates the supplied
//                  ObservationSpecification in-memory over the full active candidate set
//                  (usp_ObservationItems_GetAllActive), since the interface carries no
//                  tenant/site scoping parameter to push down to SQL. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag from day one. Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.SafetyEvents.Observation.Enums;
using PlantSafety.Domain.SafetyEvents.Observation.Events;
using PlantSafety.Domain.SafetyEvents.Observation.Interfaces;
using PlantSafety.Domain.SafetyEvents.Observation.Specifications;
using PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation"/>
/// aggregate root (Domain write-side repository interface).
/// </summary>
public sealed class ObservationWriteDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ObservationWriteDapperRepository> logger)
    : IObservationRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation?> GetByIdAsync(ObservationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Observation {ObservationId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ObservationId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ObservationRow>(
            "[SafetyEvents].[usp_ObservationItems_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation>> ListAsync(ObservationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Observation via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<ObservationRow>(
            "[SafetyEvents].[usp_ObservationItems_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Observation {ObservationId}", aggregate.ObservationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ObservationId", aggregate.ObservationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ObservationType", aggregate.ObservationType, 50)
            .AddString("Category", aggregate.HazardCategory, 50)
            .AddString("Location", aggregate.Location, 400)
            .AddString("Description", aggregate.Description, 2000)
            .AddBool("CorrectiveActionRequired", aggregate.CorrectiveActionRequired)
            .AddDateTime("FollowUpDueAtUtc", aggregate.FollowUpDueAtUtc?.Value)
            .AddDateTime("ObservedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("ObservedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_ObservationItems_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring: enqueue any domain events raised by the aggregate during this write so
        // TransactionalUnitOfWork.CommitAsync (invoked by TransactionBehavior after the handler
        // returns) can flush them to [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Observation {ObservationId}", aggregate.ObservationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ObservationId", aggregate.ObservationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("ObservationType", aggregate.ObservationType, 50)
            .AddString("Category", aggregate.HazardCategory, 50)
            .AddString("Location", aggregate.Location, 400)
            .AddString("Description", aggregate.Description, 2000)
            .AddBool("CorrectiveActionRequired", aggregate.CorrectiveActionRequired)
            .AddDateTime("FollowUpDueAtUtc", aggregate.FollowUpDueAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_ObservationItems_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[SafetyEvents].[usp_ObservationItems_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Observation {aggregate.ObservationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring: UpdateAsync must enqueue domain events too, not just AddAsync --
        // otherwise every post-creation transition (Activate, Complete, Close, Archive) would
        // silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation Hydrate(ObservationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.ObservationId), ObservationId.FromGuid(row.ObservationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.Status), Enum.Parse<ObservationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.ObserverId), new UserId(row.ObservedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.ObservationType), row.ObservationType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.HazardCategory), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.CorrectiveActionRequired), row.CorrectiveActionRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.FollowUpDueAtUtc), row.FollowUpDueAtUtc.HasValue ? UtcTimestamp.From(row.FollowUpDueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.CreatedAtUtc), UtcTimestamp.From(row.ObservedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation.CreatedBy), new UserId(row.ObservedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IObservationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ObservationItems_GetById</c>/<c>usp_ObservationItems_GetAllActive</c>.</summary>
    private sealed record ObservationRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_ObservationItems_GetById</c>. Not consumed by aggregate hydration -- callers
        /// obtain the ETag for a subsequent write from a prior single-record read.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ObservationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string ObservationType { get; init; }
        public required string Category { get; init; }
        public string? Location { get; init; }
        public string? Description { get; init; }
        public bool CorrectiveActionRequired { get; init; }
        public DateTime? FollowUpDueAtUtc { get; init; }
        public required DateTime ObservedAtUtc { get; init; }
        public required Guid ObservedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
