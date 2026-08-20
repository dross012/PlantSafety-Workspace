// ============================================================================
// File: EnvironmentalMonitoringResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: External-facing response DTO returned inside ApiResponse<EnvironmentalMonitoringResponse> for EnvironmentalMonitoring queries.
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

using PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;

namespace PlantSafety.Api.Operational.EnvironmentalMonitoring.Contracts.Responses;

/// <summary>Response payload for EnvironmentalMonitoring queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the EnvironmentalMonitoring read-model.</param>
public sealed record EnvironmentalMonitoringResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<EnvironmentalMonitoringItemDto> Items);