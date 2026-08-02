// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           GetPredictiveSafetyAlertByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single PredictiveSafetyAlert read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Distinct from the pre-existing, untouched date-range
//                  GetPredictiveSafetyQuery. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Queries.GetRCAByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.PredictiveSafety.Models;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Queries;

/// <summary>Retrieves a single PredictiveSafetyAlert read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictiveSafetyAlertId">Identity of the predictive safety alert record to retrieve.</param>
public sealed record GetPredictiveSafetyAlertByIdQuery(
    Guid TenantId,
    Guid PredictiveSafetyAlertId) : IRequest<Result<PredictiveSafetyAlertDetailDto>>;
