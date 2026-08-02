// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Infrastructure
//  File:           LotoProceduresDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LotoProcedures aggregate. Implements both the
//                  Domain-layer write repository (ILotoProceduresRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetLotoProceduresItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  LotoProcedures exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.LotoProcedures.Specifications;
using PlantSafety.Domain.SafetyProcedures.LotoProcedures.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.LotoProcedures.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.LotoProcedures.Interfaces.ILotoProceduresRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.LotoProcedures.Repositories.ILotoProceduresRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.LotoProcedures.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[LotoProcedures], column-for-column with the LotoProcedures aggregate.</summary>
internal sealed class LotoProceduresRow
{
    public Guid LotoProceduresId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProcedureTitle { get; init; }
    public string? EnergySourceTypes { get; init; }
    public string? IsolationPointDescription { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? NextAuditDueAtUtc { get; init; }
    public int AuthorizedPersonnelCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the LotoProcedures aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class LotoProceduresDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<LotoProceduresDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(LotoProceduresId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for LotoProcedures {LotoProceduresId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LotoProceduresId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LotoProceduresRow>(
            $"[{Schema}].[usp_LotoProcedures_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, LotoProceduresRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(LotoProceduresSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for LotoProcedures");

        var rows = await queryExecutor.QueryAsync<LotoProceduresRow>(
            $"[{Schema}].[usp_LotoProcedures_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, LotoProceduresRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for LotoProcedures {LotoProceduresId}", aggregate.LotoProceduresId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("LotoProceduresId", aggregate, "LotoProceduresId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .AddAggregateProperty("AuthorizedPersonnelCount", aggregate, "AuthorizedPersonnelCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoProcedures_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for LotoProcedures {LotoProceduresId}", aggregate.LotoProceduresId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("LotoProceduresId", aggregate, "LotoProceduresId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .AddAggregateProperty("AuthorizedPersonnelCount", aggregate, "AuthorizedPersonnelCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoProcedures_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetLotoProceduresItemsAsync"/>
    public async Task<IReadOnlyList<LotoProceduresItemDto>> GetLotoProceduresItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLotoProceduresItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LotoProceduresItemDto>(
            $"[{Schema}].[usp_LotoProcedures_GetItems]", parameters, cancellationToken);
    }
}
