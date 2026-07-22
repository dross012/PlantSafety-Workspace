// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Infrastructure
//  File:           ActiveMonitoringAuditingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ActiveMonitoringAuditing aggregate. Implements both the
//                  Domain-layer write repository (IActiveMonitoringAuditingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetActiveMonitoringAuditingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  ActiveMonitoringAuditing exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Specifications;
using PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.ValueObjects.Ids;
using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing;
using IWriteRepo = PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Interfaces.IActiveMonitoringAuditingRepository;
using IReadRepo = PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Repositories.IActiveMonitoringAuditingRepository;

namespace PlantSafety.Infrastructure.ContractorSafety.ActiveMonitoringAuditing.Persistence;

/// <summary>Flat Dapper row for [ContractorSafety].[ActiveMonitoringAuditing], column-for-column with the ActiveMonitoringAuditing aggregate.</summary>
internal sealed class ActiveMonitoringAuditingRow
{
    public Guid ActiveMonitoringAuditingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? MonitoringTitle { get; init; }
    public string? Category { get; init; }
    public string? WorkLocation { get; init; }
    public Guid MonitoredByUserId { get; init; }
    public DateTime? ObservedAtUtc { get; init; }
    public int UnsafeConditionCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public string? FindingNotes { get; init; }
    public DateTime? FollowUpAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the ActiveMonitoringAuditing aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class ActiveMonitoringAuditingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ActiveMonitoringAuditingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "ContractorSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(ActiveMonitoringAuditingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ActiveMonitoringAuditing {ActiveMonitoringAuditingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ActiveMonitoringAuditingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ActiveMonitoringAuditingRow>(
            $"[{Schema}].[usp_ActiveMonitoringAuditing_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, ActiveMonitoringAuditingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(ActiveMonitoringAuditingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for ActiveMonitoringAuditing");

        var rows = await queryExecutor.QueryAsync<ActiveMonitoringAuditingRow>(
            $"[{Schema}].[usp_ActiveMonitoringAuditing_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, ActiveMonitoringAuditingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for ActiveMonitoringAuditing {ActiveMonitoringAuditingId}", aggregate.ActiveMonitoringAuditingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("ActiveMonitoringAuditingId", aggregate, "ActiveMonitoringAuditingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("MonitoringTitle", aggregate, "MonitoringTitle")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("WorkLocation", aggregate, "WorkLocation")
            .AddAggregateProperty("MonitoredByUserId", aggregate, "MonitoredByUserId")
            .AddAggregateProperty("ObservedAtUtc", aggregate, "ObservedAtUtc")
            .AddAggregateProperty("UnsafeConditionCount", aggregate, "UnsafeConditionCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("FindingNotes", aggregate, "FindingNotes")
            .AddAggregateProperty("FollowUpAtUtc", aggregate, "FollowUpAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ActiveMonitoringAuditing_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for ActiveMonitoringAuditing {ActiveMonitoringAuditingId}", aggregate.ActiveMonitoringAuditingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("ActiveMonitoringAuditingId", aggregate, "ActiveMonitoringAuditingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("MonitoringTitle", aggregate, "MonitoringTitle")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("WorkLocation", aggregate, "WorkLocation")
            .AddAggregateProperty("MonitoredByUserId", aggregate, "MonitoredByUserId")
            .AddAggregateProperty("ObservedAtUtc", aggregate, "ObservedAtUtc")
            .AddAggregateProperty("UnsafeConditionCount", aggregate, "UnsafeConditionCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("FindingNotes", aggregate, "FindingNotes")
            .AddAggregateProperty("FollowUpAtUtc", aggregate, "FollowUpAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ActiveMonitoringAuditing_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetActiveMonitoringAuditingItemsAsync"/>
    public async Task<IReadOnlyList<ActiveMonitoringAuditingItemDto>> GetActiveMonitoringAuditingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetActiveMonitoringAuditingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ActiveMonitoringAuditingItemDto>(
            $"[{Schema}].[usp_ActiveMonitoringAuditing_GetItems]", parameters, cancellationToken);
    }
}
