// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenancePredictionsQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Paginated Predictions List query, backing both the "active predictions" and
//                  "full predictions" UI sub-resources via the ActiveOnly flag. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves a paginated list of PredictiveMaintenanceAlert-backed prediction rows for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
/// <param name="ActiveOnly">When true, restricts results to alerts that have not reached a terminal status.</param>
public sealed record GetPredictiveMaintenancePredictionsQuery(
    Guid TenantId,
    int PageNumber,
    int PageSize,
    bool ActiveOnly) : IRequest<Result<PaginatedResult<PredictiveMaintenancePredictionRowDto>>>;
