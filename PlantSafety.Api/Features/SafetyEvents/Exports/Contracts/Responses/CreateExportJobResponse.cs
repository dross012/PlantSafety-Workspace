// ============================================================================
// File: CreateExportJobResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> after an
//              export job is queued.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.Exports.Contracts.Responses;

/// <summary>Response payload for a newly queued export job.</summary>
/// <param name="JobId">Identifier of the newly queued export job.</param>
public sealed record CreateExportJobResponse(Guid JobId);
