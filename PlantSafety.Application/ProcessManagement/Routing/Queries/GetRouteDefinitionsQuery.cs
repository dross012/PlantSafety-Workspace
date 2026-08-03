// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDefinitionsQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a deterministically ordered, paginated, optionally
//                  search-filtered page of route definitions for a tenant. Backs the
//                  PlantSafety.UI Routing module's Dashboard list page. Mirrors
//                  PlantSafety.Application.Analytics.Reporting.Queries.GetReportsPagedQuery's
//                  shape. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Retrieves a paginated, optionally search-filtered page of route definitions for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="Search">Optional free-text search term matched against Name/PlantName. Null returns all.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size, capped by <see cref="Common.Constants.PaginationConstants"/>.</param>
public sealed record GetRouteDefinitionsQuery(
    Guid TenantId,
    string? Search,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<RouteDefinitionSummaryDto>>>;
