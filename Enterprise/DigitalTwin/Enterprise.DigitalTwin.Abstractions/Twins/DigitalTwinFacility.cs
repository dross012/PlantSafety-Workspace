// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinFacility.cs
//  Created:        2026-08-08
//  Description:    Phase D.7 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): a read-only virtual snapshot of a real
//                  Shared.FacilityDomain facility, for simulation purposes
//                  only. Reuses the real FacilityLifecycleStatus (Phase
//                  C.12) rather than inventing a parallel status
//                  vocabulary. Never mutates the real FacilityAggregate --
//                  DigitalTwinSimulationEngine answers "what would happen
//                  if" questions against this snapshot, it doesn't apply
//                  commands to live domain state.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Lifecycle;

namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a real Shared.FacilityDomain facility.</summary>
public sealed record DigitalTwinFacility(
    Guid FacilityId,
    string Name,
    FacilityLifecycleStatus LifecycleStatus);
