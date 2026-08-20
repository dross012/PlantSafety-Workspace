// ============================================================================
// File: WellnessQueryResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Internal Presentation-layer response model for Wellness, before projection to the external Contracts/Responses DTO.
// ============================================================================
//
// Onboarding Notes:
// - Populated from the Application-layer result and then projected to WellnessResponse in the controller or MappingProfile.
//
// Rationale:
// - Provides a stable internal model that absorbs Application-layer shape changes without immediately breaking the external contract.
//
// Future Extension Seams:
// - Add computed Presentation-layer fields here that enrich the Application result before serialisation.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Wellness.Models.Responses;

/// <summary>Internal Presentation-layer response model for Wellness queries.</summary>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Scoped site, or null for tenant-wide data.</param>
/// <param name="GeneratedAt">Snapshot generation timestamp (UTC).</param>
/// <param name="Items">Result items from the Application layer.</param>
public sealed record WellnessQueryResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<object> Items);