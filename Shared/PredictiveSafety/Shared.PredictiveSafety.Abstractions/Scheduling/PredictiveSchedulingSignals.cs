// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Scheduling
//  File:           PredictiveSchedulingSignals.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic cross-module
//                  inputs PredictiveSchedulingInferenceEngine reacts to.
//                  TotalRequiredActionCount/WorstCaseDueWithinHours are drawn
//                  from the real Shared.Scheduling.Abstractions.Rules.
//                  SchedulingRuleDecision (Phase 14). HazardPredictionRequiresEscalation/
//                  MaintenancePredictionRequiresImmediate/RoutingPredictionRequiresPriority
//                  are the upstream predictions, threaded forward by
//                  PredictiveSafetyEvaluationPipeline's fixed evaluation
//                  order (Hazard -> Maintenance -> Routing -> Scheduling).
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Scheduling;

/// <summary>The deterministic cross-module inputs a scheduling prediction reacts to.</summary>
public sealed record PredictiveSchedulingSignals(
    int TotalRequiredActionCount,
    int WorstCaseDueWithinHours,
    bool HazardPredictionRequiresEscalation,
    bool MaintenancePredictionRequiresImmediate,
    bool RoutingPredictionRequiresPriority);
