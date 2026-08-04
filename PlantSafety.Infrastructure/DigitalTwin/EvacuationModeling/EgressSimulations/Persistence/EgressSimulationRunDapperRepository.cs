// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Infrastructure
//  File:           EgressSimulationRunDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EgressSimulationRun aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model repository
//                  of the same simple name. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun;
using IWriteRepo = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces.IEgressSimulationRunRepository;
using IReadRepo = PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Repositories.IEgressSimulationRepository;

namespace PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EgressSimulations.Persistence;

/// <summary>Flat Dapper row for [DigitalTwin].[EgressSimulationRuns], column-for-column with the EgressSimulationRun aggregate.</summary>
internal sealed class EgressSimulationRunRow
{
    public Guid EgressSimulationRunId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid ZoneId { get; init; }
    public string? TwinModelVersion { get; init; }
    public string? TriggerReason { get; init; }
    public int? SimulatedEvacuationTimeSeconds { get; init; }
    public int? PredictedPeakOccupancy { get; init; }
    public Guid? LinkedDrillId { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EgressSimulationRun aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EgressSimulationRunDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EgressSimulationRunDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "DigitalTwin";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EgressSimulationRunId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EgressSimulationRunId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EgressSimulationRunRow>(
            $"[{Schema}].[usp_EgressSimulationRuns_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EgressSimulationRunRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EgressSimulationRunSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<EgressSimulationRunRow>(
            $"[{Schema}].[usp_EgressSimulationRuns_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, EgressSimulationRunRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EgressSimulationRunId", aggregate, "EgressSimulationRunId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ZoneId", aggregate, "ZoneId")
            .AddAggregateProperty("TwinModelVersion", aggregate, "TwinModelVersion")
            .AddAggregateProperty("TriggerReason", aggregate, "TriggerReason")
            .AddAggregateProperty("SimulatedEvacuationTimeSeconds", aggregate, "SimulatedEvacuationTimeSeconds")
            .AddAggregateProperty("PredictedPeakOccupancy", aggregate, "PredictedPeakOccupancy")
            .AddAggregateProperty("LinkedDrillId", aggregate, "LinkedDrillId")
            .AddAggregateProperty("StartedAtUtc", aggregate, "StartedAtUtc")
            .AddAggregateProperty("CompletedAtUtc", aggregate, "CompletedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EgressSimulationRuns_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EgressSimulationRunId", aggregate, "EgressSimulationRunId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SimulatedEvacuationTimeSeconds", aggregate, "SimulatedEvacuationTimeSeconds")
            .AddAggregateProperty("PredictedPeakOccupancy", aggregate, "PredictedPeakOccupancy")
            .AddAggregateProperty("LinkedDrillId", aggregate, "LinkedDrillId")
            .AddAggregateProperty("CompletedAtUtc", aggregate, "CompletedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EgressSimulationRuns_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetLatestByZoneAsync"/>
    public async Task<EgressSimulationItemDto?> GetLatestByZoneAsync(Guid tenantId, Guid? siteId, Guid zoneId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddGuid("ZoneId", zoneId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EgressSimulationRunRow>(
            $"[{Schema}].[usp_EgressSimulationRuns_GetLatestByZone]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    private static EgressSimulationItemDto ToDto(EgressSimulationRunRow row) => new()
    {
        EgressSimulationRunId = row.EgressSimulationRunId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        ZoneId = row.ZoneId,
        TwinModelVersion = row.TwinModelVersion ?? string.Empty,
        TriggerReason = row.TriggerReason ?? string.Empty,
        SimulationStatus = row.Status ?? string.Empty,
        SimulatedEvacuationTimeSeconds = row.SimulatedEvacuationTimeSeconds,
        PredictedPeakOccupancy = row.PredictedPeakOccupancy,
        LinkedDrillId = row.LinkedDrillId,
        StartedAtUtc = row.StartedAtUtc ?? default,
        CompletedAtUtc = row.CompletedAtUtc,
        CreatedAtUtc = row.CreatedAtUtc ?? default,
        CreatedBy = row.CreatedBy,
    };
}
