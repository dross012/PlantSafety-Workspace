// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Infrastructure
//  File:           CommunityConcernDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunityConcern aggregate (primary aggregate for
//                  the CommunitySafety module). Implements both the Domain-layer write repository
//                  (ICommunityConcernRepository, GetByIdAsync/ListAsync/AddAsync/UpdateAsync) and
//                  the Application-layer read-model repository of the same simple name
//                  (open-concerns list, get-by-id). All DB access via stored procedures; no
//                  dynamic SQL. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Specifications;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;
using IWriteRepo = PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces.ICommunityConcernRepository;
using IReadRepo = PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories.ICommunityConcernRepository;

namespace PlantSafety.Infrastructure.CommunitySafety.CommunityConcern.Persistence;

/// <summary>Flat Dapper row for [CommunitySafety].[CommunityConcern], column-for-column with the CommunityConcern aggregate.</summary>
internal sealed class CommunityConcernRow
{
    public Guid CommunityConcernId { get; init; }
    public Guid TenantId { get; init; }
    public Guid SiteId { get; init; }
    public string? Status { get; init; }
    public string? Category { get; init; }
    public string? Severity { get; init; }
    public string? FacilityArea { get; init; }
    public string? Description { get; init; }
    public Guid? ReportedByContactId { get; init; }
    public Guid? RelatedIncidentId { get; init; }
    public DateTime? LoggedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
    public int EscalationCount { get; init; }
    public string? EscalationReason { get; init; }
    public DateTime? EscalatedAtUtc { get; init; }
    public string? ResolutionNotes { get; init; }
    public DateTime? ResolvedAtUtc { get; init; }
}

/// <summary>
/// Dapper-backed repository for the CommunityConcern aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class CommunityConcernDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CommunityConcernDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "CommunitySafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CommunityConcernId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for CommunityConcern {CommunityConcernId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CommunityConcernId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CommunityConcernRow>(
            $"[{Schema}].[usp_CommunityConcern_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CommunityConcernRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(CommunityConcernSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for CommunityConcern");

        var rows = await queryExecutor.QueryAsync<CommunityConcernRow>(
            $"[{Schema}].[usp_CommunityConcern_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, CommunityConcernRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for CommunityConcern {CommunityConcernId}", aggregate.CommunityConcernId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CommunityConcernId", aggregate, "CommunityConcernId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("Severity", aggregate, "Severity")
            .AddAggregateProperty("FacilityArea", aggregate, "FacilityArea")
            .AddAggregateProperty("Description", aggregate, "Description")
            .AddAggregateProperty("ReportedByContactId", aggregate, "ReportedByContactId")
            .AddAggregateProperty("RelatedIncidentId", aggregate, "RelatedIncidentId")
            .AddAggregateProperty("LoggedAtUtc", aggregate, "LoggedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityConcern_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for CommunityConcern {CommunityConcernId}", aggregate.CommunityConcernId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CommunityConcernId", aggregate, "CommunityConcernId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("Severity", aggregate, "Severity")
            .AddAggregateProperty("EscalationCount", aggregate, "EscalationCount")
            .AddAggregateProperty("EscalationReason", aggregate, "EscalationReason")
            .AddAggregateProperty("EscalatedAtUtc", aggregate, "EscalatedAtUtc")
            .AddAggregateProperty("ResolutionNotes", aggregate, "ResolutionNotes")
            .AddAggregateProperty("ResolvedAtUtc", aggregate, "ResolvedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_CommunityConcern_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetOpenCommunityConcernItemsAsync"/>
    public async Task<IReadOnlyList<CommunityConcernItemDto>> GetOpenCommunityConcernItemsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetOpenCommunityConcernItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<CommunityConcernItemDto>(
            $"[{Schema}].[usp_CommunityConcern_GetOpen]", parameters, cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetCommunityConcernItemByIdAsync"/>
    public async Task<CommunityConcernItemDto?> GetCommunityConcernItemByIdAsync(
        Guid tenantId,
        Guid communityConcernId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCommunityConcernItemByIdAsync for {CommunityConcernId}", communityConcernId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CommunityConcernId", communityConcernId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CommunityConcernItemDto>(
            $"[{Schema}].[usp_CommunityConcern_GetById]", parameters, cancellationToken);
    }
}
