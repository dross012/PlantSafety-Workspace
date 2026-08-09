// ============================================================================
//  Project:        AI.HazardUnderstanding
//  Module:         Abstractions.Semantics
//  File:           HazardSemanticModel.cs
//  Created:        2026-08-08
//  Description:    Phase E.2: the full deterministic "understanding" of a
//                  single hazard occurrence -- its category (HazardOntology),
//                  worst-case severity (Shared.FacilityDomain's Phase C.3
//                  SeverityInferenceRules), propagation chain (Phase C.6's
//                  HazardPropagationRules), the richer multi-hazard chain
//                  with per-node severity (Phase C.9's HazardChainBuilder),
//                  and regulatory impact (Phase C.4's RegulatoryInferenceRules).
//                  Built exclusively by HazardUnderstandingEngine.Understand.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Chains;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.HazardUnderstanding.Abstractions.Semantics;

/// <summary>The full deterministic "understanding" of a single hazard occurrence.</summary>
public sealed record HazardSemanticModel(
    HazardType HazardType,
    HazardCategory Category,
    IReadOnlyList<HazardType> RelatedHazards,
    SeverityLevel AssessedSeverity,
    IReadOnlyList<PropagationStage> PropagationChain,
    HazardChain MultiHazardChain,
    IReadOnlyList<RegulatoryJurisdiction> RegulatoryImpact);
