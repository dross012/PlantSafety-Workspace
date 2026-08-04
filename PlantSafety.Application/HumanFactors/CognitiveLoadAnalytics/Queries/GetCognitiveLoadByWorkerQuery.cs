// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetCognitiveLoadByWorkerQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving a single worker's cognitive-load rollup for a period. Data
//                  governance: authorization must restrict this query to the worker themselves,
//                  their direct supervisor, or an authorized HumanFactors analyst role.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;

/// <summary>Retrieves a single worker's cognitive-load rollup for a period.</summary>
public sealed record GetCognitiveLoadByWorkerQuery(
    Guid TenantId,
    Guid WorkerUserId,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc) : IRequest<Result<CognitiveLoadByWorkerDto>>;
