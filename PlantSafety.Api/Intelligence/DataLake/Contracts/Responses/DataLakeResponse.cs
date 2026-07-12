// ============================================================================
// File: DataLakeResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<DataLakeResponse> for DataLake queries.
// ============================================================================
//
// Onboarding Notes:
// - Serialised by the controller and wrapped in ApiResponse<T> before being sent to callers.
//
// Rationale:
// - Keeps the external contract stable even when Application-layer result shapes change.
//
// Future Extension Seams:
// - Add hypermedia links or pagination metadata here when HAL/HATEOAS support is added.
//
// ============================================================================
using PlantSafety.Application.Intelligence.DataLake.Models;

namespace PlantSafety.Api.Intelligence.DataLake.Contracts.Responses;

/// <summary>Response payload for DataLake queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the DataLake read-model.</param>
public sealed record DataLakeResponse(
    Guid                       TenantId,
    Guid?                      SiteId,
    DateTime                   GeneratedAt,
    IReadOnlyList<DataLakeItemDto>    Items);