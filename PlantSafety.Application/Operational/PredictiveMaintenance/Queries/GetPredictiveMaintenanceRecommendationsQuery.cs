// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceRecommendationsQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for a prediction's recommended-actions list. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the recommended corrective actions for a prediction.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictionId">The underlying PredictiveMaintenanceAlert identifier.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetPredictiveMaintenanceRecommendationsQuery(
    Guid TenantId,
    Guid PredictionId,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<PredictiveMaintenanceRecommendationDto>>>;
