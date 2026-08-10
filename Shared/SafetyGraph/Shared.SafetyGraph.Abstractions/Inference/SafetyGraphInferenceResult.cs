// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Inference
//  File:           SafetyGraphInferenceResult.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The deterministic output of a full
//                  graph inference pass. OverallPriority reuses Shared.
//                  PredictiveSafety.Abstractions.Common.PredictivePriority
//                  (Phase 15) rather than inventing a second priority
//                  vocabulary.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.SafetyGraph.Abstractions.Inference;

/// <summary>The deterministic output of a full Safety Graph inference pass.</summary>
public sealed record SafetyGraphInferenceResult(
    bool RequiresGraphWideEscalation,
    PredictivePriority OverallPriority,
    IReadOnlyList<string> AppliedRuleCodes,
    IReadOnlyList<string> HazardPropagationNodeIds,
    IReadOnlyList<string> MaintenancePropagationNodeIds,
    IReadOnlyList<string> WorkflowPropagationNodeIds);
