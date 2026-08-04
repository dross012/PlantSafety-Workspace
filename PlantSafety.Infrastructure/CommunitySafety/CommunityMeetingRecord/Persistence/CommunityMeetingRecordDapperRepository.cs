// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Infrastructure
//  File:           CommunityMeetingRecordDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunityMeetingRecord aggregate. Implements both
//                  the Domain-layer write repository and the Application-layer read-model
//                  repository (period-scoped meeting history). All DB access via stored
//                  procedures; no dynamic SQL. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates.CommunityMeetingRecord;
using IWriteRepo = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Interfaces.ICommunityMeetingRecordRepository;
using IReadRepo = PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Repositories.ICommunityMeetingRecordRepository;

namespace PlantSafety.Infrastructure.CommunitySafety.CommunityMeetingRecord.Persistence;

/// <summary>Flat Dapper row for [CommunitySafety].[CommunityMeetingRecord], column-for-column with the aggregate.</summary>
internal sealed class CommunityMeetingRecordRow
{
    public Guid CommunityMeetingRecordId { get; init; }
    public Guid TenantId { get; init; }
    public Guid SiteId { get; init; }
    public string? Status { get; init; }
    public string? MeetingTitle { get; init; }
    public DateTime? MeetingDateUtc { get; init; }
    public string? Location { get; init; }
    public int AttendeeCount { get; init; }
    public string? TopicsSummary { get; init; }
    public string? ActionItems { get; init; }
    public Guid RecordedByUserId { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CommunityMeetingRecord aggregate. Implements the Domain
/// write repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CommunityMeetingRecordDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunityMeetingRecordDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "CommunitySafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CommunityMeetingRecordId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunityMeetingRecord {CommunityMeetingRecordId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunityMeetingRecordId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunityMeetingRecordRow>(
            $"[{Schema}].[usp_CommunityMeetingRecord_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CommunityMeetingRecordRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CommunityMeetingRecord {CommunityMeetingRecordId}", aggregate.CommunityMeetingRecordId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CommunityMeetingRecordId", aggregate, "CommunityMeetingRecordId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("MeetingTitle", aggregate, "MeetingTitle")
            .AddAggregateProperty("MeetingDateUtc", aggregate, "MeetingDateUtc")
            .AddAggregateProperty("Location", aggregate, "Location")
            .AddAggregateProperty("AttendeeCount", aggregate, "AttendeeCount")
            .AddAggregateProperty("TopicsSummary", aggregate, "TopicsSummary")
            .AddAggregateProperty("ActionItems", aggregate, "ActionItems")
            .AddAggregateProperty("RecordedByUserId", aggregate, "RecordedByUserId")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityMeetingRecord_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CommunityMeetingRecord {CommunityMeetingRecordId}", aggregate.CommunityMeetingRecordId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CommunityMeetingRecordId", aggregate, "CommunityMeetingRecordId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityMeetingRecord_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetCommunityMeetingRecordItemsAsync"/>
    public async Task<IReadOnlyList<CommunityMeetingRecordItemDto>> GetCommunityMeetingRecordItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCommunityMeetingRecordItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CommunityMeetingRecordItemDto>(
            $"[{Schema}].[usp_CommunityMeetingRecord_GetItems]", parameters, cancellationToken);
    }
}
