// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceModelExplanationQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query for the Model Explanation panel, backed by the real
//                  FailureLikelihoodPredictor's public blend weights and rationale output.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Queries;

/// <summary>Retrieves the model-explanation (feature importance + version) data for a prediction.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictionId">The underlying PredictiveMaintenanceAlert identifier.</param>
public sealed record GetPredictiveMaintenanceModelExplanationQuery(Guid TenantId, Guid PredictionId) : IRequest<Result<PredictiveMaintenanceModelExplanationDto>>;
