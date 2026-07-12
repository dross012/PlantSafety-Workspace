// ============================================================================
// File: AnalyticsMappingProfile.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Mapping configuration between Integrations.Analytics Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises Integrations.Analytics DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when AnalyticsResponse requires flattening nested Application-layer value objects.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analytics.Mapping;

/// <summary>
/// Mapping profile for projecting Integrations.Analytics Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class AnalyticsMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}
