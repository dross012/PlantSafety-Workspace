// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RouteDefinitionReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RouteDefinition Application-layer read-model. Backs
//                  GetRouteDefinitionsQuery, GetRouteDefinitionByIdQuery, and GetRouteStepsQuery.
//                  GetStepsAsync rehydrates domain RouteStep entities from the persisted StepsJson
//                  column so RouteStep.ComputeStatus() remains the single source of truth for
//                  step readiness classification (no duplicated logic at the read edge). Mirrors
//                  PlantSafety.Infrastructure.ProcessManagement.Persistence.ProcessDefinitionReadDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Entities;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

/// <summary>Dapper-backed repository for RouteDefinition read-model queries.</summary>
public sealed class RouteDefinitionReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RouteDefinitionReadDapperRepository> logger)
    : IRouteDefinitionReadRepository
{
    /// <inheritdoc/>
    public async Task<(IReadOnlyList<RouteDefinitionSummaryDto> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPagedAsync RouteDefinitions for Tenant {TenantId}", tenantId);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Search", search, 200)
            .Build();

        var pageParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Search", search, 200)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int>(
            "[ProcessManagement].[usp_RouteDefinitionAggregate_GetPagedCount]", countParameters, cancellationToken);

        var rows = await queryExecutor.QueryAsync<RouteSummaryRow>(
            "[ProcessManagement].[usp_RouteDefinitionAggregate_GetPagedList]", pageParameters, cancellationToken);

        var items = rows.Select(r => new RouteDefinitionSummaryDto
        {
            RoutingId = r.RouteDefinitionId,
            Name = r.Name,
            Plant = r.PlantName,
            Status = r.Status,
            Version = $"v{r.Version}",
            LastUpdatedUtc = DateTime.SpecifyKind(r.LastAuditedAtUtc, DateTimeKind.Utc),
            Owner = r.OwnerName,
        }).ToList();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<RouteDefinitionDetailDto?> GetByIdAsync(Guid tenantId, Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById RouteDefinition {RouteDefinitionId} Tenant {TenantId}", routeDefinitionId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", routeDefinitionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RouteDetailRow>(
            "[ProcessManagement].[usp_RouteDefinitionAggregate_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return new RouteDefinitionDetailDto
        {
            RoutingId = row.RouteDefinitionId,
            TenantId = row.TenantId,
            Name = row.Name,
            Plant = row.PlantName,
            Status = row.Status,
            Version = $"v{row.Version}",
            VersionId = null,
            EffectiveFromUtc = DateTime.SpecifyKind(row.EffectiveFromUtc, DateTimeKind.Utc),
            EffectiveToUtc = row.EffectiveToUtc.HasValue ? DateTime.SpecifyKind(row.EffectiveToUtc.Value, DateTimeKind.Utc) : null,
            Owner = row.OwnerName,
            LastAuditedUtc = DateTime.SpecifyKind(row.LastAuditedAtUtc, DateTimeKind.Utc),
            ETag = row.ETag,
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RouteStepItemDto>?> GetStepsAsync(Guid tenantId, Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSteps RouteDefinition {RouteDefinitionId} Tenant {TenantId}", routeDefinitionId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RouteDefinitionId", routeDefinitionId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RouteDetailRow>(
            "[ProcessManagement].[usp_RouteDefinitionAggregate_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        return RouteStepJsonSerializer.Deserialize(row.StepsJson)
            .Select(s => RouteStep.Rehydrate(s.StepId, s.StepNumber, s.Name, s.Instructions, s.Resources, s.EstimatedDurationMinutes))
            .OrderBy(s => s.StepNumber)
            .Select(s => new RouteStepItemDto
            {
                StepNumber = s.StepNumber,
                Name = s.Name,
                Status = s.ComputeStatus().ToString(),
                Instructions = s.Instructions,
                Resources = s.Resources,
            })
            .ToList();
    }

    private sealed record RouteSummaryRow
    {
        public required Guid RouteDefinitionId { get; init; }
        public required string Name { get; init; }
        public required string PlantName { get; init; }
        public required string Status { get; init; }
        public required int Version { get; init; }
        public required DateTime LastAuditedAtUtc { get; init; }
        public required string OwnerName { get; init; }
    }

    private sealed record RouteDetailRow
    {
        public string? ETag { get; init; }
        public required Guid RouteDefinitionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string PlantName { get; init; }
        public required string Name { get; init; }
        public required string Status { get; init; }
        public required int Version { get; init; }
        public required string OwnerName { get; init; }
        public required DateTime EffectiveFromUtc { get; init; }
        public DateTime? EffectiveToUtc { get; init; }
        public string? StepsJson { get; init; }
        public required DateTime LastAuditedAtUtc { get; init; }
    }
}
