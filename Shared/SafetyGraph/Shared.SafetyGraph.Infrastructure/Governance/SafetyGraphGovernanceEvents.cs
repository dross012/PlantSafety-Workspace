// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Governance
//  File:           SafetyGraphGovernanceEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The fixed set of EventId literals
//                  Safety Graph's telemetry and governance layers write,
//                  mirroring SchedulingGovernanceEvents/PredictiveGovernanceEvents
//                  (Phases 14/15) exactly.
// ============================================================================
namespace Shared.SafetyGraph.Infrastructure.Governance;

/// <summary>The fixed set of EventId literals Safety Graph's telemetry and governance layers write.</summary>
public static class SafetyGraphGovernanceEvents
{
    /// <summary>Emitted by SafetyGraphTelemetry after a graph is constructed.</summary>
    public const string GraphConstructed = "SAFETY_GRAPH_CONSTRUCTED";

    /// <summary>Emitted by SafetyGraphTelemetry after a traversal operation completes.</summary>
    public const string TraversalPerformed = "SAFETY_GRAPH_TRAVERSAL_PERFORMED";

    /// <summary>Emitted by SafetyGraphTelemetry after an inference evaluation completes.</summary>
    public const string InferenceComputed = "SAFETY_GRAPH_INFERENCE_COMPUTED";

    /// <summary>Emitted by SafetyGraphEventEmitter after a graph evaluation result event is published.</summary>
    public const string EvaluationEventPublished = "SAFETY_GRAPH_EVALUATION_EVENT_PUBLISHED";

    /// <summary>Emitted by SafetyGraphGovernanceEvaluator when a governance rule violation is found.</summary>
    public const string GovernanceViolation = "SAFETY_GRAPH_GOVERNANCE_VIOLATION";

    /// <summary>Emitted by SafetyGraphOrchestrator once a full evaluation completes.</summary>
    public const string OrchestrationCompleted = "SAFETY_GRAPH_ORCHESTRATION_COMPLETED";

    /// <summary>Emitted by SafetyGraphTracing for each recorded orchestration span.</summary>
    public const string TraceSpan = "SAFETY_GRAPH_TRACE_SPAN";
}
