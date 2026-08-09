// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Aggregates
//  File:           FacilityState.cs
//  Created:        2026-08-08
//  Description:    The immutable data snapshot of a FacilityAggregate at a
//                  point in time. FacilityAggregate is the only thing
//                  allowed to construct/replace a FacilityState -- callers
//                  read it, they don't build it directly.
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Aggregates;

/// <summary>The immutable data snapshot of a <see cref="FacilityAggregate"/>.</summary>
public sealed record FacilityState(
    FacilityId FacilityId,
    string Name,
    FacilityIndustry Industry,
    FacilityLocation Location,
    FacilityHazardProfile HazardProfile,
    FacilityRegulatoryProfile RegulatoryProfile,
    FacilityProgramProfile ProgramProfile,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
