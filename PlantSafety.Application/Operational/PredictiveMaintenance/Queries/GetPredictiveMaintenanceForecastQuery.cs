// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceForecastQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the risk forecast curve, each point independently computed by the
//                  real FailureLikelihoodPredictor. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the risk forecast curve for a prediction.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictionId">The underlying PredictiveMaintenanceAlert identifier.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetPredictiveMaintenanceForecastQuery(
    Guid TenantId,
    Guid PredictionId,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>>;
