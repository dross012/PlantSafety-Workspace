// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Infrastructure
//  File:           HazardCommunicationRightToKnowDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardCommunicationRightToKnow aggregate. Implements both the
//                  Domain-layer write repository (IHazardCommunicationRightToKnowRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetHazardCommunicationRightToKnowItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  HazardCommunicationRightToKnow exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Specifications;
using PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Interfaces.IHazardCommunicationRightToKnowRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Repositories.IHazardCommunicationRightToKnowRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.HazardCommunicationRightToKnow.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[HazardCommunicationRightToKnow], column-for-column with the HazardCommunicationRightToKnow aggregate.</summary>
internal sealed class HazardCommunicationRightToKnowRow
{
    public Guid HazardCommunicationRightToKnowId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProcedureTitle { get; init; }
    public string? HazardClass { get; init; }
    public string? SdsReference { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? NextReviewDueAtUtc { get; init; }
    public int LabeledItemCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the HazardCommunicationRightToKnow aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class HazardCommunicationRightToKnowDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazardCommunicationRightToKnowDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(HazardCommunicationRightToKnowId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazardCommunicationRightToKnowId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardCommunicationRightToKnowRow>(
            $"[{Schema}].[usp_HazardCommunicationRightToKnow_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, HazardCommunicationRightToKnowRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(HazardCommunicationRightToKnowSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for HazardCommunicationRightToKnow");

        var rows = await queryExecutor.QueryAsync<HazardCommunicationRightToKnowRow>(
            $"[{Schema}].[usp_HazardCommunicationRightToKnow_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, HazardCommunicationRightToKnowRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}", aggregate.HazardCommunicationRightToKnowId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("HazardCommunicationRightToKnowId", aggregate, "HazardCommunicationRightToKnowId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("SdsReference", aggregate, "SdsReference")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("LabeledItemCount", aggregate, "LabeledItemCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HazardCommunicationRightToKnow_Create]", parameters, cancellationToken: cancellationToken);
    
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

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}", aggregate.HazardCommunicationRightToKnowId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("HazardCommunicationRightToKnowId", aggregate, "HazardCommunicationRightToKnowId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("SdsReference", aggregate, "SdsReference")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("LabeledItemCount", aggregate, "LabeledItemCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HazardCommunicationRightToKnow_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetHazardCommunicationRightToKnowItemsAsync"/>
    public async Task<IReadOnlyList<HazardCommunicationRightToKnowItemDto>> GetHazardCommunicationRightToKnowItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardCommunicationRightToKnowItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardCommunicationRightToKnowItemDto>(
            $"[{Schema}].[usp_HazardCommunicationRightToKnow_GetItems]", parameters, cancellationToken);
    }
}
