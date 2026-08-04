// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Infrastructure
//  File:           CommunityNotificationDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunityNotification aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model repository
//                  (period-scoped notification history). All DB access via stored procedures; no
//                  dynamic SQL. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.ValueObjects.Ids;
using PlantSafety.Application.CommunitySafety.CommunityNotification.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates.CommunityNotification;
using IWriteRepo = PlantSafety.Domain.CommunitySafety.CommunityNotification.Interfaces.ICommunityNotificationRepository;
using IReadRepo = PlantSafety.Application.CommunitySafety.CommunityNotification.Repositories.ICommunityNotificationRepository;

namespace PlantSafety.Infrastructure.CommunitySafety.CommunityNotification.Persistence;

/// <summary>Flat Dapper row for [CommunitySafety].[CommunityNotification], column-for-column with the aggregate.</summary>
internal sealed class CommunityNotificationRow
{
    public Guid CommunityNotificationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? Channel { get; init; }
    public Guid RecipientContactId { get; init; }
    public string? Subject { get; init; }
    public string? MessageBody { get; init; }
    public Guid? RelatedConcernId { get; init; }
    public Guid? RelatedIncidentId { get; init; }
    public DateTime? SentAtUtc { get; init; }
    public DateTime? AcknowledgedAtUtc { get; init; }
    public string? FailureReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CommunityNotification aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CommunityNotificationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunityNotificationDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "CommunitySafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CommunityNotificationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunityNotification {CommunityNotificationId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunityNotificationId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunityNotificationRow>(
            $"[{Schema}].[usp_CommunityNotification_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CommunityNotificationRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CommunityNotification {CommunityNotificationId}", aggregate.CommunityNotificationId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CommunityNotificationId", aggregate, "CommunityNotificationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("Channel", aggregate, "Channel")
            .AddAggregateProperty("RecipientContactId", aggregate, "RecipientContactId")
            .AddAggregateProperty("Subject", aggregate, "Subject")
            .AddAggregateProperty("MessageBody", aggregate, "MessageBody")
            .AddAggregateProperty("RelatedConcernId", aggregate, "RelatedConcernId")
            .AddAggregateProperty("RelatedIncidentId", aggregate, "RelatedIncidentId")
            .AddAggregateProperty("SentAtUtc", aggregate, "SentAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityNotification_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CommunityNotification {CommunityNotificationId}", aggregate.CommunityNotificationId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CommunityNotificationId", aggregate, "CommunityNotificationId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AcknowledgedAtUtc", aggregate, "AcknowledgedAtUtc")
            .AddAggregateProperty("FailureReason", aggregate, "FailureReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityNotification_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetCommunityNotificationItemsAsync"/>
    public async Task<IReadOnlyList<CommunityNotificationItemDto>> GetCommunityNotificationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCommunityNotificationItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CommunityNotificationItemDto>(
            $"[{Schema}].[usp_CommunityNotification_GetItems]", parameters, cancellationToken);
    }
}
