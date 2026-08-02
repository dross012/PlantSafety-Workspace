// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           GetDrillsAndExercisesByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single DrillsAndExercises read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;

/// <summary>Retrieves a single DrillsAndExercises read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DrillsAndExercisesId">Identity of the DrillsAndExercises record to retrieve.</param>
public sealed record GetDrillsAndExercisesByIdQuery(
    Guid TenantId,
    Guid DrillsAndExercisesId) : IRequest<Result<DrillsAndExercisesDetailDto>>;
