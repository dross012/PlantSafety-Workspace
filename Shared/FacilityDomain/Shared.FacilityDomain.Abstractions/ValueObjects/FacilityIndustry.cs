// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.ValueObjects
//  File:           FacilityIndustry.cs
//  Created:        2026-08-08
//  Description:    A facility's industry binding. IndustryName is the same
//                  natural key every other subsystem in this codebase uses
//                  to refer to an industry (see Shared.SafetyData's
//                  IndustryRegistry) -- kept as a plain string here rather
//                  than referencing Shared.SafetyData directly, since
//                  Shared.FacilityDomain is a new, independent bounded
//                  context and must not take a compile-time dependency on
//                  a read-model layer built over the identity it now owns.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.ValueObjects;

/// <summary>A facility's industry binding.</summary>
/// <param name="IndustryName">The industry's canonical display name (matches the natural key used across the rest of PlantSafety).</param>
/// <param name="NaicsCode">The industry's NAICS code, if known.</param>
public sealed record FacilityIndustry(string IndustryName, string? NaicsCode = null);
