// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           GetAITrainingJobByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single AITrainingJob read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Distinct from the pre-existing, untouched date-range
//                  GetAITrainingQuery. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Queries.GetRCAByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AITraining.Models;

namespace PlantSafety.Application.Intelligence.AITraining.Queries;

/// <summary>Retrieves a single AITrainingJob read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="AITrainingJobId">Identity of the training job record to retrieve.</param>
public sealed record GetAITrainingJobByIdQuery(
    Guid TenantId,
    Guid AITrainingJobId) : IRequest<Result<AITrainingJobDetailDto>>;
