// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Governance
//  File:           PredictiveGovernanceEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The fixed set of EventId literals
//                  Predictive Safety's telemetry and governance layers
//                  write, mirroring SchedulingGovernanceEvents (Phase 14)
//                  exactly.
// ============================================================================
namespace Shared.PredictiveSafety.Infrastructure.Governance;

/// <summary>The fixed set of EventId literals Predictive Safety's telemetry and governance layers write.</summary>
public static class PredictiveGovernanceEvents
{
    /// <summary>Emitted by PredictiveTelemetry after a domain evaluation completes.</summary>
    public const string EvaluationComputed = "PREDICTIVE_EVALUATION_COMPUTED";

    /// <summary>Emitted by PredictiveSafetyEventEmitter after a predictive result event is published.</summary>
    public const string EvaluationEventPublished = "PREDICTIVE_EVALUATION_EVENT_PUBLISHED";

    /// <summary>Emitted by PredictiveGovernanceEvaluator when a governance rule violation is found.</summary>
    public const string GovernanceViolation = "PREDICTIVE_GOVERNANCE_VIOLATION";

    /// <summary>Emitted by PredictiveSafetyOrchestrator once a full evaluation completes.</summary>
    public const string OrchestrationCompleted = "PREDICTIVE_ORCHESTRATION_COMPLETED";

    /// <summary>Emitted by PredictiveTracing for each recorded orchestration span.</summary>
    public const string TraceSpan = "PREDICTIVE_TRACE_SPAN";
}
