// ============================================================================
// File: AssignTrainingResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned after successfully assigning
//              a training course to a worker.
// ============================================================================

namespace PlantSafety.Api.Features.Operational.Training.Contracts.Responses;

/// <summary>Response payload returned after assigning a training course.</summary>
/// <param name="TrainingRecordId">Identifier of the newly created training assignment record.</param>
public sealed record AssignTrainingResponse(Guid TrainingRecordId);
