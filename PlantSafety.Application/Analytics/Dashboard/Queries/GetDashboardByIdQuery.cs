// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           GetDashboardByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single Dashboard read-model record by identity, including
//                  its current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.Queries;

/// <summary>Retrieves a single Dashboard read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DashboardId">Identity of the Dashboard record to retrieve.</param>
public sealed record GetDashboardByIdQuery(
    Guid TenantId,
    Guid DashboardId) : IRequest<Result<DashboardItemDto>>;
