// ============================================================================
// File: KPISnapshotsResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing response DTO returned inside ApiResponse<KPISnapshotsResponse> for KPISnapshots queries.
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

using PlantSafety.Application.Analytics.Dashboard.KPISnapshots.Models;

namespace PlantSafety.Api.Analytics.Dashboard.KPISnapshots.Contracts.Responses;

/// <summary>Response payload for KPISnapshots queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the KPISnapshots read-model.</param>
public sealed record KPISnapshotsResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<KPISnapshotsItemDto> Items);