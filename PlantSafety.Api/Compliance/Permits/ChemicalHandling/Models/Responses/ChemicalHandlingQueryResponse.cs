// ============================================================================
// File: ChemicalHandlingQueryResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Internal Presentation-layer response model for ChemicalHandling, before projection to the external Contracts/Responses DTO.
// ============================================================================
//
// Onboarding Notes:
// - Populated from the Application-layer result and then projected to ChemicalHandlingResponse in the controller or MappingProfile.
//
// Rationale:
// - Provides a stable internal model that absorbs Application-layer shape changes without immediately breaking the external contract.
//
// Future Extension Seams:
// - Add computed Presentation-layer fields here that enrich the Application result before serialisation.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.ChemicalHandling.Models.Responses;

/// <summary>Internal Presentation-layer response model for ChemicalHandling queries.</summary>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Scoped site, or null for tenant-wide data.</param>
/// <param name="GeneratedAt">Snapshot generation timestamp (UTC).</param>
/// <param name="Items">Result items from the Application layer.</param>
public sealed record ChemicalHandlingQueryResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<object> Items);