// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Infrastructure
//  File:           IncidentTrackingPerformanceEvaluationDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the IncidentTrackingPerformanceEvaluation aggregate. Implements both the
//                  Domain-layer write repository (IIncidentTrackingPerformanceEvaluationRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetIncidentTrackingPerformanceEvaluationItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  IncidentTrackingPerformanceEvaluation exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Specifications;
using PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.ValueObjects.Ids;
using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation;
using IWriteRepo = PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Interfaces.IIncidentTrackingPerformanceEvaluationRepository;
using IReadRepo = PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Repositories.IIncidentTrackingPerformanceEvaluationRepository;

namespace PlantSafety.Infrastructure.ContractorSafety.IncidentTrackingPerformanceEvaluation.Persistence;

/// <summary>Flat Dapper row for [ContractorSafety].[IncidentTrackingPerformanceEvaluation], column-for-column with the IncidentTrackingPerformanceEvaluation aggregate.</summary>
internal sealed class IncidentTrackingPerformanceEvaluationRow
{
    public Guid IncidentTrackingPerformanceEvaluationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? EvaluationTitle { get; init; }
    public string? IncidentCategory { get; init; }
    public string? ContractorName { get; init; }
    public Guid EvaluatedByUserId { get; init; }
    public DateTime? InvestigatedAtUtc { get; init; }
    public string? RootCauseSummary { get; init; }
    public int CapaCount { get; init; }
    public decimal? PerformanceScore { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the IncidentTrackingPerformanceEvaluation aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class IncidentTrackingPerformanceEvaluationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<IncidentTrackingPerformanceEvaluationDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "ContractorSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(IncidentTrackingPerformanceEvaluationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for IncidentTrackingPerformanceEvaluation {IncidentTrackingPerformanceEvaluationId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("IncidentTrackingPerformanceEvaluationId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<IncidentTrackingPerformanceEvaluationRow>(
            $"[{Schema}].[usp_IncidentTrackingPerformanceEvaluation_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, IncidentTrackingPerformanceEvaluationRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(IncidentTrackingPerformanceEvaluationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for IncidentTrackingPerformanceEvaluation");

        var rows = await queryExecutor.QueryAsync<IncidentTrackingPerformanceEvaluationRow>(
            $"[{Schema}].[usp_IncidentTrackingPerformanceEvaluation_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, IncidentTrackingPerformanceEvaluationRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for IncidentTrackingPerformanceEvaluation {IncidentTrackingPerformanceEvaluationId}", aggregate.IncidentTrackingPerformanceEvaluationId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("IncidentTrackingPerformanceEvaluationId", aggregate, "IncidentTrackingPerformanceEvaluationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("EvaluationTitle", aggregate, "EvaluationTitle")
            .AddAggregateProperty("IncidentCategory", aggregate, "IncidentCategory")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("EvaluatedByUserId", aggregate, "EvaluatedByUserId")
            .AddAggregateProperty("InvestigatedAtUtc", aggregate, "InvestigatedAtUtc")
            .AddAggregateProperty("RootCauseSummary", aggregate, "RootCauseSummary")
            .AddAggregateProperty("CapaCount", aggregate, "CapaCount")
            .AddAggregateProperty("PerformanceScore", aggregate, "PerformanceScore")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_IncidentTrackingPerformanceEvaluation_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for IncidentTrackingPerformanceEvaluation {IncidentTrackingPerformanceEvaluationId}", aggregate.IncidentTrackingPerformanceEvaluationId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("IncidentTrackingPerformanceEvaluationId", aggregate, "IncidentTrackingPerformanceEvaluationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("EvaluationTitle", aggregate, "EvaluationTitle")
            .AddAggregateProperty("IncidentCategory", aggregate, "IncidentCategory")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("EvaluatedByUserId", aggregate, "EvaluatedByUserId")
            .AddAggregateProperty("InvestigatedAtUtc", aggregate, "InvestigatedAtUtc")
            .AddAggregateProperty("RootCauseSummary", aggregate, "RootCauseSummary")
            .AddAggregateProperty("CapaCount", aggregate, "CapaCount")
            .AddAggregateProperty("PerformanceScore", aggregate, "PerformanceScore")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_IncidentTrackingPerformanceEvaluation_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetIncidentTrackingPerformanceEvaluationItemsAsync"/>
    public async Task<IReadOnlyList<IncidentTrackingPerformanceEvaluationItemDto>> GetIncidentTrackingPerformanceEvaluationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentTrackingPerformanceEvaluationItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IncidentTrackingPerformanceEvaluationItemDto>(
            $"[{Schema}].[usp_IncidentTrackingPerformanceEvaluation_GetItems]", parameters, cancellationToken);
    }
}
