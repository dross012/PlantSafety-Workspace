// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ListHazmatContainersQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve HazmatContainer read-model data for a tenant/site, optionally
//                  filtered by MaterialId. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Queries;

/// <summary>Retrieves HazmatContainer data for the given tenant, optionally scoped to a site and/or material.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="MaterialId">Optional material filter.</param>
public sealed record ListHazmatContainersQuery(
    Guid TenantId,
    Guid? SiteId,
    Guid? MaterialId) : IRequest<Result<ListHazmatContainersResult>>;

/// <summary>Result of <see cref="ListHazmatContainersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazmatContainer items.</param>
public sealed record ListHazmatContainersResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<Models.HazmatContainerDto> Items);
