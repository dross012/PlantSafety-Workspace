// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           ListHazmatShipmentsQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve HazmatShipment read-model data for a tenant/site, optionally
//                  filtered by Status and/or Mode. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Queries;

/// <summary>Retrieves HazmatShipment data for the given tenant, optionally scoped to a site and/or filtered by status/mode.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="Status">Optional lifecycle status filter.</param>
/// <param name="Mode">Optional transport mode filter.</param>
public sealed record ListHazmatShipmentsQuery(
    Guid TenantId,
    Guid? SiteId,
    string? Status,
    string? Mode) : IRequest<Result<ListHazmatShipmentsResult>>;

/// <summary>Result of <see cref="ListHazmatShipmentsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazmatShipment items.</param>
public sealed record ListHazmatShipmentsResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<Models.HazmatShipmentDto> Items);
