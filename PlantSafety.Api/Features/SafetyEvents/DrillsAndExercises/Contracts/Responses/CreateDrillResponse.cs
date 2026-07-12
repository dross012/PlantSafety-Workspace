// ============================================================================
// File: CreateDrillResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> after a
//              Safety Drill or Exercise is scheduled.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Responses;

/// <summary>Response payload for a newly scheduled Drill.</summary>
/// <param name="DrillId">Identifier of the newly scheduled drill.</param>
public sealed record CreateDrillResponse(Guid DrillId);
