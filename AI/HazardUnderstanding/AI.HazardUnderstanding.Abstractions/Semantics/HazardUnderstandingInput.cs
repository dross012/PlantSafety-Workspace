// ============================================================================
//  Project:        AI.HazardUnderstanding
//  Module:         Abstractions.Semantics
//  File:           HazardUnderstandingInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.2: the raw input HazardUnderstandingEngine.Understand
//                  reasons over. OriginPropagationStage is supplied by the
//                  caller rather than derived automatically, since not
//                  every HazardType has a corresponding PropagationStage
//                  (Smoke/Evacuation/Vapor/Exposure/Shutdown are
//                  consequence-only stages -- see Phase C.6's
//                  PropagationStage for the full rationale).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;

namespace AI.HazardUnderstanding.Abstractions.Semantics;

/// <summary>The raw input a hazard-understanding session reasons over.</summary>
public sealed record HazardUnderstandingInput(
    HazardType HazardType,
    PropagationStage OriginPropagationStage,
    int EmployeeInjuryCount,
    int MachineFailureCount,
    int EventChainLength);
