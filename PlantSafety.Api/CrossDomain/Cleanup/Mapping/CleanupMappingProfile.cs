// ============================================================================
// File: CleanupMappingProfile.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Mapping configuration between Cleanup Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises Cleanup DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when CleanupResponse requires flattening nested Application-layer value objects.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Cleanup.Mapping;

/// <summary>
/// Mapping profile for projecting Cleanup Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class CleanupMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}