// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           GetRootCauseAnalysisByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single RootCauseAnalysis read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Queries
//                  .GetPredictiveMaintenanceAlertByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;

/// <summary>Retrieves a single RootCauseAnalysis read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="RootCauseAnalysisId">Identity of the RootCauseAnalysis record to retrieve.</param>
public sealed record GetRootCauseAnalysisByIdQuery(
    Guid TenantId,
    Guid RootCauseAnalysisId) : IRequest<Result<RootCauseAnalysisDetailDto>>;
