// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceReportsQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Predictive Reports list -- monthly rollups of real
//                  PredictiveMaintenanceAlert records. No export engine exists yet; Format is
//                  informational only. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the exportable Predictive Reports list (monthly rollups) for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetPredictiveMaintenanceReportsQuery(
    Guid TenantId,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<PredictiveMaintenanceReportDto>>>;
