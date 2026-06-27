// ============================================================================
// File: EngagementMappingProfile.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Mapping configuration between Engagement Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises Engagement DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when EngagementResponse requires flattening nested Application-layer value objects such as DateRange or KpiThreshold.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Mapping;

/// <summary>
/// Mapping profile for projecting Engagement Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class EngagementMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}