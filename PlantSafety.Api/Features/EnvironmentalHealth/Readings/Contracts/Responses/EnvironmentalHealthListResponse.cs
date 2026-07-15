// ============================================================================
// File: EnvironmentalHealthListResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing paginated response DTO for Environmental Health listings.
// ============================================================================

namespace PlantSafety.Api.Features.EnvironmentalHealth.Readings.Contracts.Responses;

/// <summary>Paginated response payload for Environmental Health reading listings.</summary>
public sealed record EnvironmentalHealthListResponse(
    IReadOnlyList<EnvironmentalHealthResponse> Items,
    int                                        TotalCount,
    int                                        PageNumber,
    int                                        PageSize,
    int                                        TotalPages,
    bool                                       HasNextPage,
    bool                                       HasPreviousPage);
