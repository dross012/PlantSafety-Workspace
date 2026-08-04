// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Infrastructure
//  File:           EvacuationRouteDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EvacuationRoute aggregate. Implements both the
//                  Domain-layer write repository (IEvacuationRouteRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name. All DB access via stored procedures; no
//                  dynamic SQL. Aggregate rehydration uses AggregateHydrator because
//                  EvacuationRoute exposes only a private parameterless constructor and
//                  private-set properties. Obstruction history is persisted to the child
//                  RouteObstructions table via an idempotent MERGE upsert per entry.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute;
using IWriteRepo = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces.IEvacuationRouteRepository;
using IReadRepo = PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories.IEvacuationRouteRepository;

namespace PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationRoutes.Persistence;

/// <summary>Flat Dapper row for [DigitalTwin].[EvacuationRoutes], column-for-column with the EvacuationRoute aggregate.</summary>
internal sealed class EvacuationRouteRow
{
    public Guid EvacuationRouteId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? RouteName { get; init; }
    public Guid ZoneId { get; init; }
    public Guid AssemblyPointId { get; init; }
    public string? PathGeometryRef { get; init; }
    public string? TwinModelVersion { get; init; }
    public int EstimatedEgressTimeSeconds { get; init; }
    public int ObstructionCount { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Flat Dapper row for [DigitalTwin].[RouteObstructions].</summary>
internal sealed class RouteObstructionRow
{
    public Guid RouteObstructionId { get; init; }
    public Guid EvacuationRouteId { get; init; }
    public Guid TenantId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime RecordedAtUtc { get; init; }
    public DateTime? ClearedAtUtc { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EvacuationRoute aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same tables.
/// </summary>
public sealed class EvacuationRouteDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EvacuationRouteDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "DigitalTwin";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EvacuationRouteId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EvacuationRoute {EvacuationRouteId}", id);

        var parameters = new StoredProcedureBuilder().AddGuid("EvacuationRouteId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationRouteRow>(
            $"[{Schema}].[usp_EvacuationRoutes_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EvacuationRouteRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EvacuationRouteSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for EvacuationRoute");

        var rows = await queryExecutor.QueryAsync<EvacuationRouteRow>(
            $"[{Schema}].[usp_EvacuationRoutes_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, EvacuationRouteRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for EvacuationRoute {EvacuationRouteId}", aggregate.EvacuationRouteId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EvacuationRouteId", aggregate, "EvacuationRouteId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("RouteName", aggregate, "RouteName")
            .AddAggregateProperty("ZoneId", aggregate, "ZoneId")
            .AddAggregateProperty("AssemblyPointId", aggregate, "AssemblyPointId")
            .AddAggregateProperty("PathGeometryRef", aggregate, "PathGeometryRef")
            .AddAggregateProperty("TwinModelVersion", aggregate, "TwinModelVersion")
            .AddAggregateProperty("EstimatedEgressTimeSeconds", aggregate, "EstimatedEgressTimeSeconds")
            .AddAggregateProperty("ObstructionCount", aggregate, "ObstructionCount")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationRoutes_Create]", parameters, cancellationToken: cancellationToken);

        await PersistObstructionHistoryAsync(aggregate, connection, cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for EvacuationRoute {EvacuationRouteId}", aggregate.EvacuationRouteId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EvacuationRouteId", aggregate, "EvacuationRouteId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("RouteName", aggregate, "RouteName")
            .AddAggregateProperty("ZoneId", aggregate, "ZoneId")
            .AddAggregateProperty("AssemblyPointId", aggregate, "AssemblyPointId")
            .AddAggregateProperty("PathGeometryRef", aggregate, "PathGeometryRef")
            .AddAggregateProperty("TwinModelVersion", aggregate, "TwinModelVersion")
            .AddAggregateProperty("EstimatedEgressTimeSeconds", aggregate, "EstimatedEgressTimeSeconds")
            .AddAggregateProperty("ObstructionCount", aggregate, "ObstructionCount")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationRoutes_Update]", parameters, cancellationToken: cancellationToken);

        await PersistObstructionHistoryAsync(aggregate, connection, cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    private async Task PersistObstructionHistoryAsync(Agg aggregate, IDbConnection connection, CancellationToken cancellationToken)
    {
        foreach (var obstruction in aggregate.ObstructionHistory)
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("RouteObstructionId", obstruction.RouteObstructionId.Value)
                .AddGuid("EvacuationRouteId", aggregate.EvacuationRouteId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("Description", obstruction.Description, 1000)
                .AddString("Severity", obstruction.Severity.ToString(), 50)
                .AddDateTime("RecordedAtUtc", obstruction.RecordedAtUtc.Value)
                .AddDateTime("ClearedAtUtc", obstruction.ClearedAtUtc?.Value)
                .Build();

            await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_RouteObstructions_Upsert]", parameters, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc cref="IReadRepo.GetByIdAsync"/>
    async Task<EvacuationRouteItemDto?> IReadRepo.GetByIdAsync(Guid tenantId, Guid evacuationRouteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EvacuationRouteId", evacuationRouteId).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationRouteRow>(
            $"[{Schema}].[usp_EvacuationRoutes_GetById]", parameters, cancellationToken);

        return row is null || row.TenantId != tenantId ? null : ToDto(row);
    }

    /// <inheritdoc cref="IReadRepo.ListByZoneAsync"/>
    public async Task<IReadOnlyList<EvacuationRouteItemDto>> ListByZoneAsync(Guid tenantId, Guid zoneId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("ZoneId", zoneId).Build();

        var rows = await queryExecutor.QueryAsync<EvacuationRouteRow>(
            $"[{Schema}].[usp_EvacuationRoutes_GetByZone]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc cref="IReadRepo.ListActiveObstructionsAsync"/>
    public async Task<IReadOnlyList<RouteObstructionItemDto>> ListActiveObstructionsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("SiteId", siteId).Build();

        var rows = await queryExecutor.QueryAsync<RouteObstructionRow>(
            $"[{Schema}].[usp_RouteObstructions_GetActive]", parameters, cancellationToken);

        return rows.Select(r => new RouteObstructionItemDto
        {
            RouteObstructionId = r.RouteObstructionId,
            EvacuationRouteId = r.EvacuationRouteId,
            TenantId = r.TenantId,
            Description = r.Description,
            Severity = r.Severity,
            RecordedAtUtc = r.RecordedAtUtc,
            ClearedAtUtc = r.ClearedAtUtc,
        }).ToList();
    }

    private static EvacuationRouteItemDto ToDto(EvacuationRouteRow row) => new()
    {
        EvacuationRouteId = row.EvacuationRouteId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        RouteName = row.RouteName ?? string.Empty,
        ZoneId = row.ZoneId,
        AssemblyPointId = row.AssemblyPointId,
        PathGeometryRef = row.PathGeometryRef ?? string.Empty,
        TwinModelVersion = row.TwinModelVersion ?? string.Empty,
        EstimatedEgressTimeSeconds = row.EstimatedEgressTimeSeconds,
        ObstructionCount = row.ObstructionCount,
        RouteStatus = row.Status ?? string.Empty,
        CreatedAtUtc = row.CreatedAtUtc ?? default,
        CreatedBy = row.CreatedBy,
    };
}
