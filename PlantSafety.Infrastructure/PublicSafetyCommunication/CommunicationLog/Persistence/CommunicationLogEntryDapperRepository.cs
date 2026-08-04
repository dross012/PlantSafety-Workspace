// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Infrastructure
//  File:           CommunicationLogEntryDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed, append-only implementation of ICommunicationLogEntryRepository
//                  (Domain repository interface for the CommunicationLogEntry aggregate root).
//                  Exposes no update method, consistent with the aggregate's
//                  immutability-once-recorded data-governance policy. All DB access via stored
//                  procedures in the [PublicSafetyCommunication] schema.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Events;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.CommunicationLog.Persistence;

/// <summary>Dapper-backed, append-only repository for the <see cref="CommunicationLogEntry"/> aggregate root.</summary>
public sealed class CommunicationLogEntryDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunicationLogEntryDapperRepository> logger)
    : ICommunicationLogEntryRepository
{
    /// <inheritdoc/>
    public async Task<CommunicationLogEntry?> GetByIdAsync(CommunicationLogEntryId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunicationLogEntry {CommunicationLogEntryId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("CommunicationLogEntryId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunicationLogEntryRow>(
            "[PublicSafetyCommunication].[usp_CommunicationLogEntry_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CommunicationLogEntry>> ListAsync(CommunicationLogEntrySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List CommunicationLogEntry via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<CommunicationLogEntryRow>(
            "[PublicSafetyCommunication].[usp_CommunicationLogEntry_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<CommunicationLogEntry>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(CommunicationLogEntry aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add CommunicationLogEntry {CommunicationLogEntryId}", aggregate.CommunicationLogEntryId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunicationLogEntryId", aggregate.CommunicationLogEntryId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("IncidentId", aggregate.IncidentId)
            .AddGuid("RelatedIncidentNotificationId", aggregate.RelatedIncidentNotificationId)
            .AddGuid("RelatedPublicAlertBroadcastId", aggregate.RelatedPublicAlertBroadcastId)
            .AddString("Direction", aggregate.Direction.ToString(), 32)
            .AddString("Medium", aggregate.Medium.ToString(), 32)
            .AddString("CounterpartyName", aggregate.CounterpartyName, 400)
            .AddString("Summary", aggregate.Summary, size: -1)
            .AddDateTime("OccurredAtUtc", aggregate.OccurredAtUtc.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PublicSafetyCommunication].[usp_CommunicationLogEntry_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="CommunicationLogEntry"/> aggregate from its flat read-model row.</summary>
    private static CommunicationLogEntry Hydrate(CommunicationLogEntryRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<CommunicationLogEntry>();

        AggregateReflectionHydrator.SetProperty(agg, "CommunicationLogEntryId", CommunicationLogEntryId.FromGuid(row.CommunicationLogEntryId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "IncidentId", row.IncidentId);
        AggregateReflectionHydrator.SetProperty(agg, "RelatedIncidentNotificationId", row.RelatedIncidentNotificationId);
        AggregateReflectionHydrator.SetProperty(agg, "RelatedPublicAlertBroadcastId", row.RelatedPublicAlertBroadcastId);
        AggregateReflectionHydrator.SetProperty(agg, "Direction", Enum.Parse<CommunicationDirection>(row.Direction));
        AggregateReflectionHydrator.SetProperty(agg, "Medium", Enum.Parse<CommunicationMedium>(row.Medium));
        AggregateReflectionHydrator.SetProperty(agg, "CounterpartyName", row.CounterpartyName);
        AggregateReflectionHydrator.SetProperty(agg, "Summary", row.Summary);
        AggregateReflectionHydrator.SetProperty(agg, "OccurredAtUtc", UtcTimestamp.From(row.OccurredAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ICommunicationLogEntryDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_CommunicationLogEntry_GetById</c>/<c>usp_CommunicationLogEntry_GetAllActive</c>.</summary>
    private sealed record CommunicationLogEntryRow
    {
        public required Guid CommunicationLogEntryId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public Guid? IncidentId { get; init; }
        public Guid? RelatedIncidentNotificationId { get; init; }
        public Guid? RelatedPublicAlertBroadcastId { get; init; }
        public required string Direction { get; init; }
        public required string Medium { get; init; }
        public required string CounterpartyName { get; init; }
        public required string Summary { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
