// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Infrastructure
//  File:           AutonomousVehicleIncidentDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AutonomousVehicleIncident aggregate. Implements both
//                  the Domain-layer write repository (IAutonomousVehicleIncidentRepository,
//                  GetByIdAsync/ListAsync/AddAsync/UpdateAsync) and the Application-layer
//                  read-model repository of the same simple name
//                  (GetIncidentHistoryByAssetIdAsync). All DB access via stored procedures; no
//                  dynamic SQL. Aggregate rehydration uses AggregateHydrator (reflection,
//                  confined to Infrastructure). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Specifications;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.ValueObjects.Ids;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates.AutonomousVehicleIncident;
using IWriteRepo = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces.IAutonomousVehicleIncidentRepository;
using IReadRepo = PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Repositories.IAutonomousVehicleIncidentRepository;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.AutonomousVehicleIncident.Persistence;

/// <summary>Flat Dapper row for [AutonomousVehicleSafety].[AutonomousVehicleIncident], column-for-column with the AutonomousVehicleIncident aggregate.</summary>
internal sealed class AutonomousVehicleIncidentRow
{
    public Guid AutonomousVehicleIncidentId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid AutonomousVehicleAssetId { get; init; }
    public string? IncidentType { get; init; }
    public string? SeverityLevel { get; init; }
    public DateTime? OccurredAtUtc { get; init; }
    public string? DescriptionSummary { get; init; }
    public Guid? InvestigationLeadUserId { get; init; }
    public bool SuspensionTriggered { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the AutonomousVehicleIncident aggregate. Implements the Domain
/// write repository and the Application read-model repository against the same table.
/// </summary>
public sealed class AutonomousVehicleIncidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AutonomousVehicleIncidentDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "AutonomousVehicleSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AutonomousVehicleIncidentId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AutonomousVehicleIncident {AutonomousVehicleIncidentId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AutonomousVehicleIncidentId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AutonomousVehicleIncidentRow>(
            $"[{Schema}].[usp_AutonomousVehicleIncident_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AutonomousVehicleIncidentRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AutonomousVehicleIncidentSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for AutonomousVehicleIncident");

        var rows = await queryExecutor.QueryAsync<AutonomousVehicleIncidentRow>(
            $"[{Schema}].[usp_AutonomousVehicleIncident_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, AutonomousVehicleIncidentRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for AutonomousVehicleIncident {AutonomousVehicleIncidentId}", aggregate.AutonomousVehicleIncidentId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AutonomousVehicleIncidentId", aggregate, "AutonomousVehicleIncidentId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("IncidentType", aggregate, "IncidentType")
            .AddAggregateProperty("SeverityLevel", aggregate, "SeverityLevel")
            .AddAggregateProperty("OccurredAtUtc", aggregate, "OccurredAtUtc")
            .AddAggregateProperty("DescriptionSummary", aggregate, "DescriptionSummary")
            .AddAggregateProperty("InvestigationLeadUserId", aggregate, "InvestigationLeadUserId")
            .AddAggregateProperty("SuspensionTriggered", aggregate, "SuspensionTriggered")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AutonomousVehicleIncident_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for AutonomousVehicleIncident {AutonomousVehicleIncidentId}", aggregate.AutonomousVehicleIncidentId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AutonomousVehicleIncidentId", aggregate, "AutonomousVehicleIncidentId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("IncidentType", aggregate, "IncidentType")
            .AddAggregateProperty("SeverityLevel", aggregate, "SeverityLevel")
            .AddAggregateProperty("OccurredAtUtc", aggregate, "OccurredAtUtc")
            .AddAggregateProperty("DescriptionSummary", aggregate, "DescriptionSummary")
            .AddAggregateProperty("InvestigationLeadUserId", aggregate, "InvestigationLeadUserId")
            .AddAggregateProperty("SuspensionTriggered", aggregate, "SuspensionTriggered")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AutonomousVehicleIncident_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetIncidentHistoryByAssetIdAsync"/>
    public async Task<IReadOnlyList<AutonomousVehicleIncidentItemDto>> GetIncidentHistoryByAssetIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentHistoryByAssetIdAsync for Asset {AutonomousVehicleAssetId}, tenant {TenantId}", autonomousVehicleAssetId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AutonomousVehicleAssetId", autonomousVehicleAssetId)
            .Build();

        return await queryExecutor.QueryAsync<AutonomousVehicleIncidentItemDto>(
            $"[{Schema}].[usp_AutonomousVehicleIncident_GetHistoryByAssetId]", parameters, cancellationToken);
    }
}
