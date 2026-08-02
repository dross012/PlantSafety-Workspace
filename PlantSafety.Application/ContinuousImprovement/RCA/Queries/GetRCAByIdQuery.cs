// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           GetRCAByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single RCA read-model record by identity, including its
//                  current ETag, so a caller can read-before-write into one of the lifecycle
//                  commands. Distinct from the pre-existing, untouched date-range GetRCAQuery.
//                  Mirrors PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries
//                  .GetRootCauseAnalysisByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContinuousImprovement.RCA.Models;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Queries;

/// <summary>Retrieves a single RCA read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="RcaId">Identity of the RCA record to retrieve.</param>
public sealed record GetRCAByIdQuery(
    Guid TenantId,
    Guid RcaId) : IRequest<Result<RCADetailDto>>;
