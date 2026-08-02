// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Infrastructure
//  File:           RegulatoryCalendarAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRegulatoryCalendarRepository (Domain
//                  repository interface for the RegulatoryCalendar aggregate root). All DB access
//                  via stored procedures in the [Compliance] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because RegulatoryCalendar exposes only a private
//                  "for rehydration only" constructor and private-setter properties. ListAsync
//                  evaluates the supplied RegulatoryCalendarSpecification predicate in-memory over
//                  the full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL (mirrors PermitsDapperRepository). Both
//                  AddAsync AND UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag from day one (this aggregate had no prior write path to retrofit).
//                  Named distinctly from the aggregate/namespace-segment ("RegulatoryCalendar")
//                  to avoid a same-simple-name compile collision (class RegulatoryCalendar sits in
//                  namespace ...Compliance.RegulatoryCalendar.Persistence), mirroring the
//                  ScoreCards/Engagement aggregate build-outs' identical precedent. Mirrors
//                  PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Enums;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Events;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Specifications;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Persistence;

/// <summary>Dapper-backed repository for the <see cref="RegulatoryCalendar"/> aggregate root.</summary>
public sealed class RegulatoryCalendarAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RegulatoryCalendarAggregateDapperRepository> logger)
    : IRegulatoryCalendarRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar?> GetByIdAsync(RegulatoryCalendarId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RegulatoryCalendar {RegulatoryCalendarId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RegulatoryCalendarId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<RegulatoryCalendarRow>(
            "[Compliance].[usp_RegulatoryCalendar_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar>> ListAsync(RegulatoryCalendarSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List RegulatoryCalendar via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<RegulatoryCalendarRow>(
            "[Compliance].[usp_RegulatoryCalendar_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add RegulatoryCalendar {RegulatoryCalendarId}", aggregate.RegulatoryCalendarId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RegulatoryCalendarId", aggregate.RegulatoryCalendarId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("EventTitle", aggregate.EventTitle, 400)
            .AddString("RegulatoryBody", aggregate.RegulatoryBody, 400)
            .AddString("JurisdictionCode", aggregate.JurisdictionCode, 50)
            .AddString("ComplianceCategory", aggregate.ComplianceCategory, 200)
            .AddGuid("ResponsiblePartyId", aggregate.ResponsiblePartyId.Value)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc?.Value)
            .AddInt("ReminderLeadDays", aggregate.ReminderLeadDays)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_RegulatoryCalendar_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages].
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update RegulatoryCalendar {RegulatoryCalendarId}", aggregate.RegulatoryCalendarId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RegulatoryCalendarId", aggregate.RegulatoryCalendarId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("EventTitle", aggregate.EventTitle, 400)
            .AddString("RegulatoryBody", aggregate.RegulatoryBody, 400)
            .AddString("JurisdictionCode", aggregate.JurisdictionCode, 50)
            .AddString("ComplianceCategory", aggregate.ComplianceCategory, 200)
            .AddGuid("ResponsiblePartyId", aggregate.ResponsiblePartyId.Value)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc?.Value)
            .AddInt("ReminderLeadDays", aggregate.ReminderLeadDays)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_RegulatoryCalendar_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_RegulatoryCalendar_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"RegulatoryCalendar {aggregate.RegulatoryCalendarId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. UpdateAsync must do this too, not just AddAsync --
        // otherwise every post-creation transition (Activate, RecordCompliance, Close, Archive)
        // would silently drop its domain event.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="RegulatoryCalendar"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar Hydrate(RegulatoryCalendarRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.RegulatoryCalendarId), RegulatoryCalendarId.FromGuid(row.RegulatoryCalendarId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.Status), Enum.Parse<RegulatoryCalendarStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.EventTitle), row.EventTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.RegulatoryBody), row.RegulatoryBody);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.JurisdictionCode), row.JurisdictionCode);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.ComplianceCategory), row.ComplianceCategory);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.ResponsiblePartyId), new UserId(row.ResponsiblePartyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.DueAtUtc), row.DueAtUtc.HasValue ? UtcTimestamp.From(row.DueAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.ReminderLeadDays), row.ReminderLeadDays);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IRegulatoryCalendarDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_RegulatoryCalendar_GetById</c>/<c>usp_RegulatoryCalendar_GetAllActive</c>.</summary>
    private sealed record RegulatoryCalendarRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_RegulatoryCalendar_GetById</c>/<c>usp_RegulatoryCalendar_GetAllActive</c>.
        /// Not consumed by aggregate hydration -- callers obtain the ETag for a subsequent write
        /// from a prior single-record read (see the Application-layer RegulatoryCalendarItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid RegulatoryCalendarId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string EventTitle { get; init; }
        public required string RegulatoryBody { get; init; }
        public required string JurisdictionCode { get; init; }
        public required string ComplianceCategory { get; init; }
        public required Guid ResponsiblePartyId { get; init; }
        public DateTime? DueAtUtc { get; init; }
        public int ReminderLeadDays { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
