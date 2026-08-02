// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           GetAIInvestigationByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single AIInvestigation read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries
//                  .GetRootCauseAnalysisByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AIInvestigations.Models;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Queries;

/// <summary>Retrieves a single AIInvestigation read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AIInvestigationId">Identity of the AIInvestigation record to retrieve.</param>
public sealed record GetAIInvestigationByIdQuery(
    Guid TenantId,
    Guid AIInvestigationId) : IRequest<Result<AIInvestigationDetailDto>>;
