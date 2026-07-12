// ============================================================================
// File: InternalResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<InternalResponse> for Internal queries.
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
using PlantSafety.Application.Integrations.Notifications.Internal.Models;

namespace PlantSafety.Api.Integrations.Notifications.Internal.Contracts.Responses;

/// <summary>Response payload for Internal queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the Internal read-model.</param>
public sealed record InternalResponse(
    Guid                       TenantId,
    Guid?                      SiteId,
    DateTime                   GeneratedAt,
    IReadOnlyList<InternalItemDto>    Items);