// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Orchestration
//  File:           SafetyGraphEventEmitter.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Emits a composed
//                  SafetyGraphEvaluationResultContract as a structured
//                  LogEvent -- the unified event schema (Phase 12 precedent),
//                  mirroring SchedulingEventEmitter/PredictiveSafetyEventEmitter
//                  (Phases 14/15) exactly. Pure emission: never recomputes
//                  the graph or its inference result.
// ============================================================================
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Orchestration;

/// <summary>Emits a composed SafetyGraphEvaluationResultContract as a structured LogEvent.</summary>
public static class SafetyGraphEventEmitter
{
    private const string Module = "SafetyGraph";

    /// <summary>Writes one SAFETY_GRAPH_EVALUATION_EVENT_PUBLISHED log event carrying <paramref name="result"/>.</summary>
    public static void Emit(SafetyGraphEvaluationResultContract result, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: SafetyGraphGovernanceEvents.EvaluationEventPublished,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Info,
            Message: $"Published Safety Graph evaluation for tenant \"{result.Metadata.TenantId}\": {result.Graph.Nodes.Count} node(s), {result.Graph.Edges.Count} edge(s), priority {result.Inference.OverallPriority}.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["NodeCount"] = result.Graph.Nodes.Count,
                ["EdgeCount"] = result.Graph.Edges.Count,
                ["RequiresGraphWideEscalation"] = result.Inference.RequiresGraphWideEscalation,
                ["OverallPriority"] = result.Inference.OverallPriority.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: Module,
            OccurredAtUtc: result.Metadata.IssuedAtUtc));
    }
}
