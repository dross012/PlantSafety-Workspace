// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ListPackagingSpecsQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve PackagingSpecification read-model data for a tenant/site,
//                  optionally filtered by DOT spec code. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;

/// <summary>Retrieves PackagingSpecification records for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="DOTSpec">Optional DOT packaging specification code filter.</param>
public sealed record ListPackagingSpecsQuery(
    Guid TenantId,
    Guid? SiteId,
    string? DOTSpec) : IRequest<Result<ListPackagingSpecsResult>>;
