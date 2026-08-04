// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Infrastructure
//  File:           IncidentNotificationDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IIncidentNotificationRepository (Domain
//                  repository interface for the IncidentNotification aggregate root). All DB
//                  access via stored procedures in the [PublicSafetyCommunication] schema.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Events;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Specifications;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.IncidentNotificationDispatch.Persistence;

/// <summary>Dapper-backed repository for the <see cref="IncidentNotification"/> aggregate root.</summary>
public sealed class IncidentNotificationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<IncidentNotificationDapperRepository> logger)
    : IIncidentNotificationRepository
{
    /// <inheritdoc/>
    public async Task<IncidentNotification?> GetByIdAsync(IncidentNotificationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for IncidentNotification {IncidentNotificationId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("IncidentNotificationId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<IncidentNotificationRow>(
            "[PublicSafetyCommunication].[usp_IncidentNotification_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentNotification>> ListAsync(IncidentNotificationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List IncidentNotification via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<IncidentNotificationRow>(
            "[PublicSafetyCommunication].[usp_IncidentNotification_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<IncidentNotification>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(IncidentNotification aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add IncidentNotification {IncidentNotificationId}", aggregate.IncidentNotificationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentNotificationId", aggregate.IncidentNotificationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("IncidentId", aggregate.IncidentId)
            .AddGuid("PublicSafetyAgencyId", aggregate.PublicSafetyAgencyId.Value)
            .AddString("ChannelCode", aggregate.Channel.Code, 64)
            .AddString("Message", aggregate.Message, size: -1)
            .AddDateTime("DispatchedAtUtc", aggregate.DispatchedAtUtc?.Value)
            .AddGuid("DispatchedBy", aggregate.DispatchedBy?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PublicSafetyCommunication].[usp_IncidentNotification_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(IncidentNotification aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update IncidentNotification {IncidentNotificationId}", aggregate.IncidentNotificationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentNotificationId", aggregate.IncidentNotificationId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("AcknowledgedAtUtc", aggregate.AcknowledgedAtUtc?.Value)
            .AddString("AcknowledgedByContactName", aggregate.AcknowledgedByContactName, 400)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddString("FailureReason", aggregate.FailureReason, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PublicSafetyCommunication].[usp_IncidentNotification_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"IncidentNotification {aggregate.IncidentNotificationId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs an <see cref="IncidentNotification"/> aggregate from its flat read-model row.</summary>
    private static IncidentNotification Hydrate(IncidentNotificationRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<IncidentNotification>();

        AggregateReflectionHydrator.SetProperty(agg, "IncidentNotificationId", IncidentNotificationId.FromGuid(row.IncidentNotificationId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<IncidentNotificationStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "IncidentId", row.IncidentId);
        AggregateReflectionHydrator.SetProperty(agg, "PublicSafetyAgencyId", PublicSafetyAgencyId.FromGuid(row.PublicSafetyAgencyId));
        AggregateReflectionHydrator.SetProperty(agg, "Channel", PublicSafetyDeliveryChannel.From(row.ChannelCode));
        AggregateReflectionHydrator.SetProperty(agg, "Message", row.Message);
        AggregateReflectionHydrator.SetProperty(agg, "DispatchedAtUtc", row.DispatchedAtUtc.HasValue ? UtcTimestamp.From(row.DispatchedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "DispatchedBy", row.DispatchedBy.HasValue ? new UserId(row.DispatchedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "AcknowledgedAtUtc", row.AcknowledgedAtUtc.HasValue ? UtcTimestamp.From(row.AcknowledgedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "AcknowledgedByContactName", row.AcknowledgedByContactName);
        AggregateReflectionHydrator.SetProperty(agg, "ClosedAtUtc", row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "FailureReason", row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IIncidentNotificationDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_IncidentNotification_GetById</c>/<c>usp_IncidentNotification_GetAllActive</c>.</summary>
    private sealed record IncidentNotificationRow
    {
        /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
        public string? ETag { get; init; }

        public required Guid IncidentNotificationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public Guid? IncidentId { get; init; }
        public required Guid PublicSafetyAgencyId { get; init; }
        public required string ChannelCode { get; init; }
        public required string Message { get; init; }
        public DateTime? DispatchedAtUtc { get; init; }
        public Guid? DispatchedBy { get; init; }
        public DateTime? AcknowledgedAtUtc { get; init; }
        public string? AcknowledgedByContactName { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public string? FailureReason { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
