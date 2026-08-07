// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ListClassifiedMaterialsQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve HazmatMaterial read-model data for a tenant/site, optionally
//                  filtered by UN number. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Queries;

/// <summary>Retrieves HazmatMaterial records for the given tenant, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="UNNumber">Optional DOT/UN number filter.</param>
public sealed record ListClassifiedMaterialsQuery(
    Guid TenantId,
    Guid? SiteId,
    string? UNNumber) : IRequest<Result<ListClassifiedMaterialsResult>>;
