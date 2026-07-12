// ============================================================================
// File: RegulatoryRequirementsResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing response DTO returned inside ApiResponse<RegulatoryRequirementsResponse> for RegulatoryRequirements queries.
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

using PlantSafety.Application.Compliance.RegulatoryRequirements.Models;

namespace PlantSafety.Api.Compliance.Compliance.RegulatoryRequirements.Contracts.Responses;

/// <summary>Response payload for RegulatoryRequirements queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the RegulatoryRequirements read-model.</param>
public sealed record RegulatoryRequirementsResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<RegulatoryRequirementsItemDto> Items);