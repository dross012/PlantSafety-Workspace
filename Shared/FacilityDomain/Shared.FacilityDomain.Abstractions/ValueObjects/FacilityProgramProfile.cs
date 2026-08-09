// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  File:           FacilityProgramProfile.cs
//  Created:        2026-08-08
//  Description:    A facility's required safety-program codes. Kept as
//                  plain codes for the same independence reason documented
//                  in FacilityIndustry.cs.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>A facility's required safety-program codes.</summary>
public sealed record FacilityProgramProfile(IReadOnlyList<string> RequiredProgramCodes, DateTime LastUpdatedUtc);
