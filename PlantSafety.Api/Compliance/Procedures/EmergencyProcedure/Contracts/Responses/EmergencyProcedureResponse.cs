// ============================================================================
// File: EmergencyProcedureResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing response DTO returned inside ApiResponse<EmergencyProcedureResponse> for EmergencyProcedure queries.
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

using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Models;

namespace PlantSafety.Api.Compliance.Procedures.EmergencyProcedure.Contracts.Responses;

/// <summary>Response payload for EmergencyProcedure queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the EmergencyProcedure read-model.</param>
public sealed record EmergencyProcedureResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<EmergencyProcedureItemDto> Items);
