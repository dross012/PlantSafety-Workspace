// ============================================================================
// File: AnnualAuditResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing response DTO returned inside ApiResponse<AnnualAuditResponse> for AnnualAudit queries.
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

using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Models;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Contracts.Responses;

/// <summary>Response payload for AnnualAudit queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the AnnualAudit read-model.</param>
public sealed record AnnualAuditResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<AnnualAuditItemDto> Items);
