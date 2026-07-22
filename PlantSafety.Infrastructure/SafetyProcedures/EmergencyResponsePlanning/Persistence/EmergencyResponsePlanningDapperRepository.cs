// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Infrastructure
//  File:           EmergencyResponsePlanningDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyResponsePlanning aggregate. Implements both the
//                  Domain-layer write repository (IEmergencyResponsePlanningRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetEmergencyResponsePlanningItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  EmergencyResponsePlanning exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Specifications;
using PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Interfaces.IEmergencyResponsePlanningRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Repositories.IEmergencyResponsePlanningRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.EmergencyResponsePlanning.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[EmergencyResponsePlanning], column-for-column with the EmergencyResponsePlanning aggregate.</summary>
internal sealed class EmergencyResponsePlanningRow
{
    public Guid EmergencyResponsePlanningId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? PlanTitle { get; init; }
    public string? EmergencyType { get; init; }
    public string? EvacuationRoute { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? DrillConductedAtUtc { get; init; }
    public DateTime? NextDrillDueAtUtc { get; init; }
    public int ParticipantCount { get; init; }
    public decimal? ReadinessScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EmergencyResponsePlanning aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EmergencyResponsePlanningDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EmergencyResponsePlanningDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EmergencyResponsePlanningId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyResponsePlanning {EmergencyResponsePlanningId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyResponsePlanningId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EmergencyResponsePlanningRow>(
            $"[{Schema}].[usp_EmergencyResponsePlanning_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EmergencyResponsePlanningRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EmergencyResponsePlanningSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for EmergencyResponsePlanning");

        var rows = await queryExecutor.QueryAsync<EmergencyResponsePlanningRow>(
            $"[{Schema}].[usp_EmergencyResponsePlanning_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, EmergencyResponsePlanningRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for EmergencyResponsePlanning {EmergencyResponsePlanningId}", aggregate.EmergencyResponsePlanningId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EmergencyResponsePlanningId", aggregate, "EmergencyResponsePlanningId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PlanTitle", aggregate, "PlanTitle")
            .AddAggregateProperty("EmergencyType", aggregate, "EmergencyType")
            .AddAggregateProperty("EvacuationRoute", aggregate, "EvacuationRoute")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("DrillConductedAtUtc", aggregate, "DrillConductedAtUtc")
            .AddAggregateProperty("NextDrillDueAtUtc", aggregate, "NextDrillDueAtUtc")
            .AddAggregateProperty("ParticipantCount", aggregate, "ParticipantCount")
            .AddAggregateProperty("ReadinessScore", aggregate, "ReadinessScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyResponsePlanning_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for EmergencyResponsePlanning {EmergencyResponsePlanningId}", aggregate.EmergencyResponsePlanningId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EmergencyResponsePlanningId", aggregate, "EmergencyResponsePlanningId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PlanTitle", aggregate, "PlanTitle")
            .AddAggregateProperty("EmergencyType", aggregate, "EmergencyType")
            .AddAggregateProperty("EvacuationRoute", aggregate, "EvacuationRoute")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("DrillConductedAtUtc", aggregate, "DrillConductedAtUtc")
            .AddAggregateProperty("NextDrillDueAtUtc", aggregate, "NextDrillDueAtUtc")
            .AddAggregateProperty("ParticipantCount", aggregate, "ParticipantCount")
            .AddAggregateProperty("ReadinessScore", aggregate, "ReadinessScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyResponsePlanning_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetEmergencyResponsePlanningItemsAsync"/>
    public async Task<IReadOnlyList<EmergencyResponsePlanningItemDto>> GetEmergencyResponsePlanningItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyResponsePlanningItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyResponsePlanningItemDto>(
            $"[{Schema}].[usp_EmergencyResponsePlanning_GetItems]", parameters, cancellationToken);
    }
}
