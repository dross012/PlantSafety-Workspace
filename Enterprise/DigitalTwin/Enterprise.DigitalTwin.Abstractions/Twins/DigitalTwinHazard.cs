// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinHazard.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: a read-only virtual snapshot of a hazard
//                  occurrence, for simulation purposes only. Reuses the
//                  real HazardType (Phase B.2) and SeverityLevel (Phase
//                  C.3) vocabularies rather than inventing parallel ones.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a hazard occurrence.</summary>
public sealed record DigitalTwinHazard(
    HazardType HazardType,
    SeverityLevel SeverityLevel);
