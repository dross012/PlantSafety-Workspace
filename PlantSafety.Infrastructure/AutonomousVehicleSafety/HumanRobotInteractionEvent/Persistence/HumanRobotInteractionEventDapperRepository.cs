// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Infrastructure
//  File:           HumanRobotInteractionEventDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HumanRobotInteractionEvent aggregate. Implements both
//                  the Domain-layer write repository (IHumanRobotInteractionEventRepository,
//                  GetByIdAsync/ListAsync/AddAsync/UpdateAsync) and the Application-layer
//                  read-model repository of the same simple name
//                  (GetHumanRobotInteractionEventItemsAsync). All DB access via stored
//                  procedures; no dynamic SQL. Aggregate rehydration uses AggregateHydrator
//                  (reflection, confined to Infrastructure). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Specifications;
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.ValueObjects.Ids;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates.HumanRobotInteractionEvent;
using IWriteRepo = PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces.IHumanRobotInteractionEventRepository;
using IReadRepo = PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Repositories.IHumanRobotInteractionEventRepository;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.HumanRobotInteractionEvent.Persistence;

/// <summary>Flat Dapper row for [AutonomousVehicleSafety].[HumanRobotInteractionEvent], column-for-column with the HumanRobotInteractionEvent aggregate.</summary>
internal sealed class HumanRobotInteractionEventRow
{
    public Guid HumanRobotInteractionEventId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid AutonomousVehicleAssetId { get; init; }
    public string? InteractionType { get; init; }
    public string? SeverityLevel { get; init; }
    public decimal? DistanceMeters { get; init; }
    public string? LocationDescription { get; init; }
    public DateTime? DetectedAtUtc { get; init; }
    public Guid? ReviewedByUserId { get; init; }
    public DateTime? ReviewedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the HumanRobotInteractionEvent aggregate. Implements the Domain
/// write repository and the Application read-model repository against the same table.
/// </summary>
public sealed class HumanRobotInteractionEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HumanRobotInteractionEventDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "AutonomousVehicleSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(HumanRobotInteractionEventId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HumanRobotInteractionEvent {HumanRobotInteractionEventId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HumanRobotInteractionEventId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HumanRobotInteractionEventRow>(
            $"[{Schema}].[usp_HumanRobotInteractionEvent_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, HumanRobotInteractionEventRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(HumanRobotInteractionEventSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for HumanRobotInteractionEvent");

        var rows = await queryExecutor.QueryAsync<HumanRobotInteractionEventRow>(
            $"[{Schema}].[usp_HumanRobotInteractionEvent_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, HumanRobotInteractionEventRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for HumanRobotInteractionEvent {HumanRobotInteractionEventId}", aggregate.HumanRobotInteractionEventId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("HumanRobotInteractionEventId", aggregate, "HumanRobotInteractionEventId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("InteractionType", aggregate, "InteractionType")
            .AddAggregateProperty("SeverityLevel", aggregate, "SeverityLevel")
            .AddAggregateProperty("DistanceMeters", aggregate, "DistanceMeters")
            .AddAggregateProperty("LocationDescription", aggregate, "LocationDescription")
            .AddAggregateProperty("DetectedAtUtc", aggregate, "DetectedAtUtc")
            .AddAggregateProperty("ReviewedByUserId", aggregate, "ReviewedByUserId")
            .AddAggregateProperty("ReviewedAtUtc", aggregate, "ReviewedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HumanRobotInteractionEvent_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for HumanRobotInteractionEvent {HumanRobotInteractionEventId}", aggregate.HumanRobotInteractionEventId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("HumanRobotInteractionEventId", aggregate, "HumanRobotInteractionEventId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("InteractionType", aggregate, "InteractionType")
            .AddAggregateProperty("SeverityLevel", aggregate, "SeverityLevel")
            .AddAggregateProperty("DistanceMeters", aggregate, "DistanceMeters")
            .AddAggregateProperty("LocationDescription", aggregate, "LocationDescription")
            .AddAggregateProperty("DetectedAtUtc", aggregate, "DetectedAtUtc")
            .AddAggregateProperty("ReviewedByUserId", aggregate, "ReviewedByUserId")
            .AddAggregateProperty("ReviewedAtUtc", aggregate, "ReviewedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HumanRobotInteractionEvent_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetHumanRobotInteractionEventItemsAsync"/>
    public async Task<IReadOnlyList<HumanRobotInteractionEventItemDto>> GetHumanRobotInteractionEventItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHumanRobotInteractionEventItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HumanRobotInteractionEventItemDto>(
            $"[{Schema}].[usp_HumanRobotInteractionEvent_GetItems]", parameters, cancellationToken);
    }
}
