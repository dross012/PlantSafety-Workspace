// ============================================================================
// File: CreateEmergencyResponseResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> after an
//              Emergency Response plan is activated.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.EmergencyManagement.Contracts.Responses;

/// <summary>Response payload for a newly activated Emergency Response.</summary>
/// <param name="ResponseId">Identifier of the newly activated emergency response.</param>
public sealed record CreateEmergencyResponseResponse(Guid ResponseId);
