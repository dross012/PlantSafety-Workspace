// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Infrastructure
//  File:           PpePoliciesDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PpePolicies aggregate. Implements both the
//                  Domain-layer write repository (IPpePoliciesRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetPpePoliciesItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  PpePolicies exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.PpePolicies.Specifications;
using PlantSafety.Domain.SafetyProcedures.PpePolicies.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.PpePolicies.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.PpePolicies.Interfaces.IPpePoliciesRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.PpePolicies.Repositories.IPpePoliciesRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.PpePolicies.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[PpePolicies], column-for-column with the PpePolicies aggregate.</summary>
internal sealed class PpePoliciesRow
{
    public Guid PpePoliciesId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? PolicyTitle { get; init; }
    public string? PpeCategory { get; init; }
    public string? AssignedZone { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? NextReviewDueAtUtc { get; init; }
    public int ReplacementScheduleCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the PpePolicies aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class PpePoliciesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PpePoliciesDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(PpePoliciesId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PpePolicies {PpePoliciesId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PpePoliciesId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PpePoliciesRow>(
            $"[{Schema}].[usp_PpePolicies_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, PpePoliciesRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(PpePoliciesSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for PpePolicies");

        var rows = await queryExecutor.QueryAsync<PpePoliciesRow>(
            $"[{Schema}].[usp_PpePolicies_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, PpePoliciesRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for PpePolicies {PpePoliciesId}", aggregate.PpePoliciesId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("PpePoliciesId", aggregate, "PpePoliciesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PolicyTitle", aggregate, "PolicyTitle")
            .AddAggregateProperty("PpeCategory", aggregate, "PpeCategory")
            .AddAggregateProperty("AssignedZone", aggregate, "AssignedZone")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("ReplacementScheduleCount", aggregate, "ReplacementScheduleCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PpePolicies_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for PpePolicies {PpePoliciesId}", aggregate.PpePoliciesId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("PpePoliciesId", aggregate, "PpePoliciesId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PolicyTitle", aggregate, "PolicyTitle")
            .AddAggregateProperty("PpeCategory", aggregate, "PpeCategory")
            .AddAggregateProperty("AssignedZone", aggregate, "AssignedZone")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("ReplacementScheduleCount", aggregate, "ReplacementScheduleCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PpePolicies_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetPpePoliciesItemsAsync"/>
    public async Task<IReadOnlyList<PpePoliciesItemDto>> GetPpePoliciesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPpePoliciesItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PpePoliciesItemDto>(
            $"[{Schema}].[usp_PpePolicies_GetItems]", parameters, cancellationToken);
    }
}
