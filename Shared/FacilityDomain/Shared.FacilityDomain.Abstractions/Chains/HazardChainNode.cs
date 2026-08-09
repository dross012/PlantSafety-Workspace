// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Chains
//  File:           HazardChainNode.cs
//  Created:        2026-08-08
//  Description:    Phase C.9 (Path C: Operational Modernization): one node
//                  in a multi-hazard event chain. Uses PropagationStage
//                  (Phase C.6), not HazardType, as the node vocabulary --
//                  Path C's example chains (Fire -> Smoke -> Evacuation,
//                  MachineFailure -> Injury -> Shutdown) mix originating
//                  hazards with downstream consequences that HazardType
//                  alone cannot express, and PropagationStage already
//                  covers both.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

namespace Shared.FacilityDomain.Abstractions.Chains;

/// <summary>One node in a multi-hazard event chain.</summary>
public sealed record HazardChainNode(
    PropagationStage Stage,
    SeverityLevel Severity,
    DateTime OccurredAtUtc,
    string? Description = null);
