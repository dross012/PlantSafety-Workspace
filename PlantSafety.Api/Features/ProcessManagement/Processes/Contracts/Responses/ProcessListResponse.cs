// ============================================================================
// File: ProcessListResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing paginated response DTO for Process Definition listings.
// ============================================================================

namespace PlantSafety.Api.Features.ProcessManagement.Processes.Contracts.Responses;

/// <summary>Paginated response payload for Process Definition listings.</summary>
public sealed record ProcessListResponse(
    IReadOnlyList<ProcessResponse> Items,
    int                            TotalCount,
    int                            PageNumber,
    int                            PageSize,
    int                            TotalPages,
    bool                           HasNextPage,
    bool                           HasPreviousPage);
