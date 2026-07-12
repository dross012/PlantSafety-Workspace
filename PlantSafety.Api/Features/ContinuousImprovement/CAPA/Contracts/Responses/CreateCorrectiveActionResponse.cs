// ============================================================================
// File: CreateCorrectiveActionResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned after successfully creating
//              a Corrective or Preventive Action.
// ============================================================================

namespace PlantSafety.Api.Features.ContinuousImprovement.CAPA.Contracts.Responses;

/// <summary>Response payload returned after creating a Corrective/Preventive Action.</summary>
/// <param name="CorrectiveActionId">Identifier of the newly created action.</param>
public sealed record CreateCorrectiveActionResponse(Guid CorrectiveActionId);
