// ============================================================================
// File: CreateCrisisEventResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> after a
//              Crisis Management event is declared.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.CrisisManagement.Contracts.Responses;

/// <summary>Response payload for a newly declared Crisis event.</summary>
/// <param name="CrisisEventId">Identifier of the newly declared crisis event.</param>
public sealed record CreateCrisisEventResponse(Guid CrisisEventId);
