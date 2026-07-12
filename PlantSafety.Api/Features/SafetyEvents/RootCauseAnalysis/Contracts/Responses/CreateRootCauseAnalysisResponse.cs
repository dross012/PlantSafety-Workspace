// ============================================================================
// File: CreateRootCauseAnalysisResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> after a
//              Root Cause Analysis is created.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts.Responses;

/// <summary>Response payload for a newly created Root Cause Analysis.</summary>
/// <param name="RcaId">Identifier of the newly created RCA.</param>
public sealed record CreateRootCauseAnalysisResponse(Guid RcaId);
