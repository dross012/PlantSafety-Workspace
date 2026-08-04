// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Infrastructure
//  File:           PublicAlertBroadcastDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPublicAlertBroadcastRepository (Domain
//                  repository interface for the PublicAlertBroadcast aggregate root). All DB
//                  access via stored procedures in the [PublicSafetyCommunication] schema.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Events;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PublicAlertBroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.PublicAlertBroadcast.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PublicAlertBroadcastAggregate"/> aggregate root.</summary>
public sealed class PublicAlertBroadcastDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PublicAlertBroadcastDapperRepository> logger)
    : IPublicAlertBroadcastRepository
{
    /// <inheritdoc/>
    public async Task<PublicAlertBroadcastAggregate?> GetByIdAsync(PublicAlertBroadcastId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PublicAlertBroadcast {PublicAlertBroadcastId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PublicAlertBroadcastId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PublicAlertBroadcastRow>(
            "[PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PublicAlertBroadcastAggregate>> ListAsync(PublicAlertBroadcastSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PublicAlertBroadcast via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PublicAlertBroadcastRow>(
            "[PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PublicAlertBroadcastAggregate>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PublicAlertBroadcastAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PublicAlertBroadcast {PublicAlertBroadcastId}", aggregate.PublicAlertBroadcastId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PublicAlertBroadcastId", aggregate.PublicAlertBroadcastId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("IncidentId", aggregate.IncidentId)
            .AddString("SeverityCode", aggregate.Severity.Code, 64)
            .AddString("ChannelCode", aggregate.Channel.Code, 64)
            .AddString("Headline", aggregate.Headline, 200)
            .AddString("Message", aggregate.Message, size: -1)
            .AddString("TargetArea", aggregate.TargetArea, 400)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddGuid("IssuedBy", aggregate.IssuedBy?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PublicSafetyCommunication].[usp_PublicAlertBroadcast_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PublicAlertBroadcastAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PublicAlertBroadcast {PublicAlertBroadcastId}", aggregate.PublicAlertBroadcastId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PublicAlertBroadcastId", aggregate.PublicAlertBroadcastId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("DeliveryConfirmedAtUtc", aggregate.DeliveryConfirmedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, 400)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PublicSafetyCommunication].[usp_PublicAlertBroadcast_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PublicAlertBroadcast {aggregate.PublicAlertBroadcastId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PublicAlertBroadcastAggregate"/> aggregate from its flat read-model row.</summary>
    private static PublicAlertBroadcastAggregate Hydrate(PublicAlertBroadcastRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PublicAlertBroadcastAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, "PublicAlertBroadcastId", PublicAlertBroadcastId.FromGuid(row.PublicAlertBroadcastId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PublicAlertBroadcastStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "IncidentId", row.IncidentId);
        AggregateReflectionHydrator.SetProperty(agg, "Severity", PublicAlertSeverity.From(row.SeverityCode));
        AggregateReflectionHydrator.SetProperty(agg, "Channel", PublicBroadcastChannel.From(row.ChannelCode));
        AggregateReflectionHydrator.SetProperty(agg, "Headline", row.Headline);
        AggregateReflectionHydrator.SetProperty(agg, "Message", row.Message);
        AggregateReflectionHydrator.SetProperty(agg, "TargetArea", row.TargetArea);
        AggregateReflectionHydrator.SetProperty(agg, "IssuedAtUtc", row.IssuedAtUtc.HasValue ? UtcTimestamp.From(row.IssuedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "IssuedBy", row.IssuedBy.HasValue ? new UserId(row.IssuedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "DeliveryConfirmedAtUtc", row.DeliveryConfirmedAtUtc.HasValue ? UtcTimestamp.From(row.DeliveryConfirmedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "FailureReason", row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, "CancelledAtUtc", row.CancelledAtUtc.HasValue ? UtcTimestamp.From(row.CancelledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPublicAlertBroadcastDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PublicAlertBroadcast_GetById</c>/<c>usp_PublicAlertBroadcast_GetAllActive</c>.</summary>
    private sealed record PublicAlertBroadcastRow
    {
        /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
        public string? ETag { get; init; }

        public required Guid PublicAlertBroadcastId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public Guid? IncidentId { get; init; }
        public required string SeverityCode { get; init; }
        public required string ChannelCode { get; init; }
        public required string Headline { get; init; }
        public required string Message { get; init; }
        public required string TargetArea { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public Guid? IssuedBy { get; init; }
        public DateTime? DeliveryConfirmedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
