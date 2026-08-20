// ============================================================================
// File: WorkforceSafetyMappingProfile.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Mapping configuration between WorkforceSafety Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises WorkforceSafety DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when WorkforceSafetyResponse requires flattening nested Application-layer value objects.
//
// ============================================================================

namespace PlantSafety.Api.Operational.WorkforceSafety.Mapping;

/// <summary>
/// Mapping profile for projecting WorkforceSafety Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class WorkforceSafetyMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}