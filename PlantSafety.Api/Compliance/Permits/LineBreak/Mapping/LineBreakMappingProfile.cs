// ============================================================================
// File: LineBreakMappingProfile.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Mapping configuration between LineBreak Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises LineBreak DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when LineBreakResponse requires flattening nested Application-layer value objects.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.LineBreak.Mapping;

/// <summary>
/// Mapping profile for projecting LineBreak Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class LineBreakMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}