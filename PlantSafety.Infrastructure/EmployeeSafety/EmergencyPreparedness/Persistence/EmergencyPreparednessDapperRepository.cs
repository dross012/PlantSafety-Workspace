// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Infrastructure
//  File:           EmergencyPreparednessDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyPreparedness aggregate. Implements both the
//                  Domain-layer write repository (IEmergencyPreparednessRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetEmergencyPreparednessItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  EmergencyPreparedness exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Specifications;
using PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Interfaces.IEmergencyPreparednessRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Repositories.IEmergencyPreparednessRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.EmergencyPreparedness.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[EmergencyPreparedness], column-for-column with the EmergencyPreparedness aggregate.</summary>
internal sealed class EmergencyPreparednessRow
{
    public Guid EmergencyPreparednessId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? PlanTitle { get; init; }
    public string? EmergencyType { get; init; }
    public string? EvacuationRoute { get; init; }
    public Guid CoordinatedByUserId { get; init; }
    public DateTime? DrillConductedAtUtc { get; init; }
    public DateTime? NextDrillDueAtUtc { get; init; }
    public int ParticipantCount { get; init; }
    public decimal? ReadinessScore { get; init; }
    public decimal ReadinessThreshold { get; init; }
    public string? ReadinessGapNotes { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EmergencyPreparedness aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EmergencyPreparednessDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EmergencyPreparednessDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EmergencyPreparednessId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyPreparedness {EmergencyPreparednessId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EmergencyPreparednessId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EmergencyPreparednessRow>(
            $"[{Schema}].[usp_EmergencyPreparedness_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EmergencyPreparednessRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EmergencyPreparednessSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for EmergencyPreparedness");

        var rows = await queryExecutor.QueryAsync<EmergencyPreparednessRow>(
            $"[{Schema}].[usp_EmergencyPreparedness_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, EmergencyPreparednessRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for EmergencyPreparedness {EmergencyPreparednessId}", aggregate.EmergencyPreparednessId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EmergencyPreparednessId", aggregate, "EmergencyPreparednessId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PlanTitle", aggregate, "PlanTitle")
            .AddAggregateProperty("EmergencyType", aggregate, "EmergencyType")
            .AddAggregateProperty("EvacuationRoute", aggregate, "EvacuationRoute")
            .AddAggregateProperty("CoordinatedByUserId", aggregate, "CoordinatedByUserId")
            .AddAggregateProperty("DrillConductedAtUtc", aggregate, "DrillConductedAtUtc")
            .AddAggregateProperty("NextDrillDueAtUtc", aggregate, "NextDrillDueAtUtc")
            .AddAggregateProperty("ParticipantCount", aggregate, "ParticipantCount")
            .AddAggregateProperty("ReadinessScore", aggregate, "ReadinessScore")
            .AddAggregateProperty("ReadinessThreshold", aggregate, "ReadinessThreshold")
            .AddAggregateProperty("ReadinessGapNotes", aggregate, "ReadinessGapNotes")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyPreparedness_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for EmergencyPreparedness {EmergencyPreparednessId}", aggregate.EmergencyPreparednessId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EmergencyPreparednessId", aggregate, "EmergencyPreparednessId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PlanTitle", aggregate, "PlanTitle")
            .AddAggregateProperty("EmergencyType", aggregate, "EmergencyType")
            .AddAggregateProperty("EvacuationRoute", aggregate, "EvacuationRoute")
            .AddAggregateProperty("CoordinatedByUserId", aggregate, "CoordinatedByUserId")
            .AddAggregateProperty("DrillConductedAtUtc", aggregate, "DrillConductedAtUtc")
            .AddAggregateProperty("NextDrillDueAtUtc", aggregate, "NextDrillDueAtUtc")
            .AddAggregateProperty("ParticipantCount", aggregate, "ParticipantCount")
            .AddAggregateProperty("ReadinessScore", aggregate, "ReadinessScore")
            .AddAggregateProperty("ReadinessThreshold", aggregate, "ReadinessThreshold")
            .AddAggregateProperty("ReadinessGapNotes", aggregate, "ReadinessGapNotes")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EmergencyPreparedness_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetEmergencyPreparednessItemsAsync"/>
    public async Task<IReadOnlyList<EmergencyPreparednessItemDto>> GetEmergencyPreparednessItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyPreparednessItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyPreparednessItemDto>(
            $"[{Schema}].[usp_EmergencyPreparedness_GetItems]", parameters, cancellationToken);
    }
}
