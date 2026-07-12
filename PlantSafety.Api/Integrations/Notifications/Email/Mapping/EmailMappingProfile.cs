// ============================================================================
// File: EmailMappingProfile.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Mapping configuration between Email Application-layer result types and Presentation-layer response DTOs.
// ============================================================================
//
// Onboarding Notes:
// - Manual mapping is used in the V1 controller for simple projections. Register AutoMapper/Mapster rules here for complex or reused projections.
//
// Rationale:
// - Centralises Email DTO projection rules. Prevents controller actions from containing complex mapping logic.
//
// Future Extension Seams:
// - Expand this profile when EmailResponse requires flattening nested Application-layer value objects.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Email.Mapping;

/// <summary>
/// Mapping profile for projecting Email Application results to Presentation DTOs.
/// Manual mapping in the V1 controller is preferred for simple projections.
/// </summary>
public sealed class EmailMappingProfile
{
    // Register AutoMapper or Mapster rules here when projections become complex or reused.
}