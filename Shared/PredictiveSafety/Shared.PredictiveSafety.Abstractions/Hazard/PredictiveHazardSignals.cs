// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Hazard
//  File:           PredictiveHazardSignals.cs
//  Created:        2026-08-10
//  Description:    Phase 15 (Predictive Safety Engine) deliverable. The
//                  deterministic cross-module inputs PredictiveHazardInferenceEngine
//                  reacts to. RoutingTriggeredAuditCount/RoutingChangedComplianceElementCount
//                  are drawn from the real EventRoutingResult (Shared.SafetyData,
//                  Phase 13). SchedulingRequiresImmediateAction is drawn from
//                  the real SchedulingRuleDecision (Shared.Scheduling, Phase
//                  14). RecentReliabilityFailureCount substitutes for the
//                  "Diagnostics signals" this phase's own prompt asks for --
//                  no standalone Diagnostics domain exists in this program
//                  (confirmed before writing this file, same finding Phase
//                  13's own header comments document); the closest real,
//                  existing signal is Shared.Telemetry's ReliabilityHooks
//                  failure count, which this program's telemetry decorators
//                  already produce for every wrapped operation.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Hazard;

/// <summary>The deterministic cross-module inputs a hazard prediction reacts to.</summary>
public sealed record PredictiveHazardSignals(
    string EventType,
    string? Description,
    int EmployeeExposureCount,
    int MachineFailureCount,
    int EventChainLength,
    int RoutingTriggeredAuditCount,
    int RoutingChangedComplianceElementCount,
    bool SchedulingRequiresImmediateAction,
    int RecentReliabilityFailureCount);
