// ============================================================================
// File: WorkerWellBeingListResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing paginated response DTO for Worker WellBeing listings.
// ============================================================================

namespace PlantSafety.Api.Features.WorkerWellBeing.Records.Contracts.Responses;

/// <summary>Paginated response payload for Worker WellBeing listings.</summary>
public sealed record WorkerWellBeingListResponse(
    IReadOnlyList<WorkerWellBeingResponse> Items,
    int                                    TotalCount,
    int                                    PageNumber,
    int                                    PageSize,
    int                                    TotalPages,
    bool                                   HasNextPage,
    bool                                   HasPreviousPage);
