// ============================================================================
// File: IncidentManagementMappingProfile.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Mapping configuration between IncidentManagement Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises IncidentManagement DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when IncidentManagementResponse requires flattening nested Application-layer value objects.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.IncidentManagement.Mapping;

/// <summary>
/// Mapping profile for projecting IncidentManagement Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class IncidentManagementMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}