// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RouteVersionSnapshotReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RouteVersionSnapshot Application-layer read-model.
//                  Backs GetRouteVersionHistoryQuery and GetRouteDiffQuery. Genuinely new -- no
//                  analog exists for ProcessDefinition. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

/// <summary>Dapper-backed repository for RouteVersionSnapshot read-model queries.</summary>
public sealed class RouteVersionSnapshotReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RouteVersionSnapshotReadDapperRepository> logger)
    : IRouteVersionSnapshotReadRepository
{
    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RouteVersionSummaryDto> Items, int TotalCount)> GetPagedForRouteAsync(
        Guid tenantId, Guid routeDefinitionId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPagedForRoute RouteVersionSnapshot for RouteDefinition {RouteDefinitionId}", routeDefinitionId);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", routeDefinitionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var pageParameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", routeDefinitionId)
            .AddGuid("TenantId", tenantId)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int>(
            "[ProcessManagement].[usp_RouteVersionSnapshot_GetPagedForRouteCount]", countParameters, cancellationToken);

        var rows = await queryExecutor.QueryAsync<RouteVersionRow>(
            "[ProcessManagement].[usp_RouteVersionSnapshot_GetPagedForRouteList]", pageParameters, cancellationToken);

        var items = rows.Select(r => new RouteVersionSummaryDto
        {
            VersionId = r.RouteVersionId,
            VersionLabel = $"v{r.VersionNumber}",
            EffectiveDateUtc = DateTime.SpecifyKind(r.EffectiveDateUtc, DateTimeKind.Utc),
            Status = r.Status,
        }).ToList();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RouteStepSnapshot>?> GetStepSnapshotsAsync(
        Guid tenantId, Guid routeDefinitionId, Guid versionId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetStepSnapshots RouteVersionSnapshot {RouteVersionId}", versionId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteVersionId", versionId)
            .AddGuid("RouteDefinitionId", routeDefinitionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var stepsJson = await queryExecutor.QuerySingleOrDefaultAsync<string>(
            "[ProcessManagement].[usp_RouteVersionSnapshot_GetStepsById]", parameters, cancellationToken);

        return stepsJson is null ? null : RouteStepJsonSerializer.Deserialize(stepsJson);
    }

    private sealed record RouteVersionRow
    {
        public required Guid RouteVersionId { get; init; }
        public required int VersionNumber { get; init; }
        public required DateTime EffectiveDateUtc { get; init; }
        public required string Status { get; init; }
    }
}
