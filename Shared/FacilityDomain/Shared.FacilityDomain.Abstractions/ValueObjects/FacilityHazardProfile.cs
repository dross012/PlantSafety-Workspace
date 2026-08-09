// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  File:           FacilityHazardProfile.cs
//  Created:        2026-08-08
//  Description:    A facility's currently active hazard types (Phase B.2's
//                  unified HazardType enum). Empty until UpdateHazardProfile
//                  is called -- no hazard is assumed present at creation.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;

namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>A facility's currently active hazard types.</summary>
public sealed record FacilityHazardProfile(IReadOnlyList<HazardType> ActiveHazardTypes, DateTime LastUpdatedUtc);
