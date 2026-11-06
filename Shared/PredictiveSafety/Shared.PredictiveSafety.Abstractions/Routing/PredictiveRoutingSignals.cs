// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Routing
//  File:           PredictiveRoutingSignals.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic cross-module
//                  inputs PredictiveRoutingInferenceEngine reacts to.
//                  TriggeredAuditCount/ChangedComplianceElementCount are
//                  drawn from the real EventRoutingResult (Shared.SafetyData,
//                  Phase 13). HazardPredictionRequiresEscalation/
//                  MaintenancePredictionRequiresImmediate are the upstream
//                  PredictiveHazardDecision/PredictiveMaintenanceDecision
//                  flags, threaded forward by PredictiveSafetyEvaluationPipeline's
//                  fixed evaluation order (Hazard -> Maintenance -> Routing),
//                  satisfying this phase's "consume hazard and maintenance
//                  predictions" requirement.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Routing;

/// <summary>The deterministic cross-module inputs a routing prediction reacts to.</summary>
public sealed record PredictiveRoutingSignals(
    string EventCode,
    Guid FacilityId,
    int TriggeredAuditCount,
    int ChangedComplianceElementCount,
    bool HazardPredictionRequiresEscalation,
    bool MaintenancePredictionRequiresImmediate);
