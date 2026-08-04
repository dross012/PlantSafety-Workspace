// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Infrastructure
//  File:           CommunityContactDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunityContact aggregate. Implements both the
//                  Domain-layer write repository (ICommunityContactRepository, GetByIdAsync/
//                  AddAsync/UpdateAsync) and the Application-layer read-model repository of the
//                  same simple name (GetCommunityContactItemsAsync). All DB access via stored
//                  procedures; no dynamic SQL. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.ValueObjects.Ids;
using PlantSafety.Application.CommunitySafety.CommunityContact.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;
using IWriteRepo = PlantSafety.Domain.CommunitySafety.CommunityContact.Interfaces.ICommunityContactRepository;
using IReadRepo = PlantSafety.Application.CommunitySafety.CommunityContact.Repositories.ICommunityContactRepository;

namespace PlantSafety.Infrastructure.CommunitySafety.CommunityContact.Persistence;

/// <summary>Flat Dapper row for [CommunitySafety].[CommunityContact], column-for-column with the CommunityContact aggregate.</summary>
internal sealed class CommunityContactRow
{
    public Guid CommunityContactId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? DisplayName { get; init; }
    public string? ContactType { get; init; }
    public string? Organization { get; init; }
    public string? CommunityArea { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public bool OptedInForNotifications { get; init; }
    public DateTime? ArchivedAtUtc { get; init; }
    public string? ArchivalReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CommunityContact aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CommunityContactDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunityContactDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "CommunitySafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CommunityContactId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunityContact {CommunityContactId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunityContactId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunityContactRow>(
            $"[{Schema}].[usp_CommunityContact_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CommunityContactRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CommunityContact {CommunityContactId}", aggregate.CommunityContactId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CommunityContactId", aggregate, "CommunityContactId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DisplayName", aggregate, "DisplayName")
            .AddAggregateProperty("ContactType", aggregate, "ContactType")
            .AddAggregateProperty("Organization", aggregate, "Organization")
            .AddAggregateProperty("CommunityArea", aggregate, "CommunityArea")
            .AddAggregateProperty("Email", aggregate, "Email")
            .AddAggregateProperty("Phone", aggregate, "Phone")
            .AddAggregateProperty("OptedInForNotifications", aggregate, "OptedInForNotifications")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityContact_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CommunityContact {CommunityContactId}", aggregate.CommunityContactId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CommunityContactId", aggregate, "CommunityContactId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("OptedInForNotifications", aggregate, "OptedInForNotifications")
            .AddAggregateProperty("ArchivedAtUtc", aggregate, "ArchivedAtUtc")
            .AddAggregateProperty("ArchivalReason", aggregate, "ArchivalReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityContact_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetCommunityContactItemsAsync"/>
    public async Task<IReadOnlyList<CommunityContactItemDto>> GetCommunityContactItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCommunityContactItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CommunityContactItemDto>(
            $"[{Schema}].[usp_CommunityContact_GetItems]", parameters, cancellationToken);
    }
}
