// ============================================================================
// File: EngagementResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: External-facing response DTO returned inside ApiResponse<EngagementResponse> for Engagement analytics queries.
// ============================================================================
//
// Onboarding Notes:
// - Serialised by the controller and wrapped in ApiResponse<T> before being sent to callers. Maps from the Application-layer result DTO.
//
// Rationale:
// - Keeps the external contract stable even when Application-layer result shapes change. Projection is done in the controller or MappingProfile.
//
// Future Extension Seams:
// - Add hypermedia links or pagination metadata here when HAL/HATEOAS support is added to Analytics endpoints.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Contracts.Responses;

/// <summary>Response payload for Engagement analytics queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide aggregates.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Data">Analytics data points for Engagement.</param>
public sealed record EngagementResponse(
    Guid     TenantId,
    Guid?    SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EngagementDataPoint> Data);

/// <summary>A single data point within a Engagement analytics result set.</summary>
/// <param name="Label">Human-readable label (department, date bucket, category, etc.).</param>
/// <param name="Value">Primary numeric metric value.</param>
/// <param name="Trend">Trend direction: Improving, Stable, or Declining.</param>
public sealed record EngagementDataPoint(
    string  Label,
    decimal Value,
    string? Trend);