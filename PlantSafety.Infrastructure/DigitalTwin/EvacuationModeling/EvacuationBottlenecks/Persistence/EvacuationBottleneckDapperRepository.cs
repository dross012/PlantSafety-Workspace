// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Infrastructure
//  File:           EvacuationBottleneckDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EvacuationBottleneck aggregate. Implements both the
//                  Domain-layer write repository (including GetOpenForRouteAsync, used by
//                  RunEgressSimulationCommandHandler to distinguish a new detection from a
//                  recurrence) and the Application-layer read-model repository of the same simple
//                  name. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck;
using IWriteRepo = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces.IEvacuationBottleneckRepository;
using IReadRepo = PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Repositories.IEvacuationBottleneckRepository;

namespace PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Persistence;

/// <summary>Flat Dapper row for [DigitalTwin].[EvacuationBottlenecks], column-for-column with the EvacuationBottleneck aggregate.</summary>
internal sealed class EvacuationBottleneckRow
{
    public Guid EvacuationBottleneckId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid RouteId { get; init; }
    public Guid SimulationRunId { get; init; }
    public string? BottleneckLocation { get; init; }
    public int DelaySeconds { get; init; }
    public int RecurrenceCount { get; init; }
    public DateTime? DetectedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EvacuationBottleneck aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EvacuationBottleneckDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EvacuationBottleneckDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "DigitalTwin";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EvacuationBottleneckId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EvacuationBottleneckId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationBottleneckRow>(
            $"[{Schema}].[usp_EvacuationBottlenecks_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EvacuationBottleneckRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.GetOpenForRouteAsync"/>
    public async Task<Agg?> GetOpenForRouteAsync(EvacuationRouteId routeId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("RouteId", routeId.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationBottleneckRow>(
            $"[{Schema}].[usp_EvacuationBottlenecks_GetOpenForRoute]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EvacuationBottleneckRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EvacuationBottleneckSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<EvacuationBottleneckRow>(
            $"[{Schema}].[usp_EvacuationBottlenecks_GetHistory]",
            new StoredProcedureBuilder().AddGuid("TenantId", Guid.Empty).AddGuid("SiteId", (Guid?)null).AddGuid("RouteId", (Guid?)null).Build(),
            cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, EvacuationBottleneckRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EvacuationBottleneckId", aggregate, "EvacuationBottleneckId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("RouteId", aggregate, "RouteId")
            .AddAggregateProperty("SimulationRunId", aggregate, "SimulationRunId")
            .AddAggregateProperty("BottleneckLocation", aggregate, "BottleneckLocation")
            .AddAggregateProperty("DelaySeconds", aggregate, "DelaySeconds")
            .AddAggregateProperty("RecurrenceCount", aggregate, "RecurrenceCount")
            .AddAggregateProperty("DetectedAtUtc", aggregate, "DetectedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationBottlenecks_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EvacuationBottleneckId", aggregate, "EvacuationBottleneckId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SimulationRunId", aggregate, "SimulationRunId")
            .AddAggregateProperty("DelaySeconds", aggregate, "DelaySeconds")
            .AddAggregateProperty("RecurrenceCount", aggregate, "RecurrenceCount")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationBottlenecks_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetHistoryAsync"/>
    public async Task<IReadOnlyList<EvacuationBottleneckItemDto>> GetHistoryAsync(Guid tenantId, Guid? siteId, Guid? routeId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddGuid("RouteId", routeId)
            .Build();

        var rows = await queryExecutor.QueryAsync<EvacuationBottleneckRow>(
            $"[{Schema}].[usp_EvacuationBottlenecks_GetHistory]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    private static EvacuationBottleneckItemDto ToDto(EvacuationBottleneckRow row) => new()
    {
        EvacuationBottleneckId = row.EvacuationBottleneckId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        RouteId = row.RouteId,
        SimulationRunId = row.SimulationRunId,
        BottleneckLocation = row.BottleneckLocation ?? string.Empty,
        DelaySeconds = row.DelaySeconds,
        RecurrenceCount = row.RecurrenceCount,
        BottleneckStatus = row.Status ?? string.Empty,
        DetectedAtUtc = row.DetectedAtUtc ?? default,
        CreatedAtUtc = row.CreatedAtUtc ?? default,
        CreatedBy = row.CreatedBy,
    };
}
