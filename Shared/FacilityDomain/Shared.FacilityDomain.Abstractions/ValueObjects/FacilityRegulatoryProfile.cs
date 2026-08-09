// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  File:           FacilityRegulatoryProfile.cs
//  Created:        2026-08-08
//  Description:    A facility's bound regulator codes. Kept as plain codes
//                  (not a reference to Shared.SafetyData's RegulatorCategory)
//                  for the same independence reason documented in
//                  FacilityIndustry.cs.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>A facility's bound regulator codes (e.g. "OSHA", "EPA").</summary>
public sealed record FacilityRegulatoryProfile(IReadOnlyList<string> RegulatorCodes, DateTime LastUpdatedUtc);
