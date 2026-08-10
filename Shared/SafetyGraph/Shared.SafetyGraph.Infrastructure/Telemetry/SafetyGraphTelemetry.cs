// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Telemetry
//  File:           SafetyGraphTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Wraps SafetyGraphAssembler.Assemble
//                  and SafetyGraphInferenceEngine.Infer (both unchanged --
//                  still pure functions) with deterministic logging,
//                  metrics, and reliability reporting, via Shared.Telemetry's
//                  existing ReliabilityHooks/ILogEventWriter/PlantSafetyMetrics.
//                  Mirrors SchedulingTelemetry/PredictiveTelemetry (Phases
//                  14/15) exactly.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Construction;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Inference;
using Shared.SafetyGraph.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.SafetyGraph.Infrastructure.Telemetry;

/// <summary>Instruments SafetyGraphAssembler.Assemble and SafetyGraphInferenceEngine.Infer with deterministic logging, metrics, and reliability reporting.</summary>
public static class SafetyGraphTelemetry
{
    private const string Module = "SafetyGraph";

    /// <summary>Assembles a SafetyGraphModel via the real SafetyGraphAssembler.Assemble, recording a SafetyGraphsConstructed metric and a structured log event.</summary>
    public static async Task<SafetyGraphModel> ConstructWithTelemetryAsync(
        SafetyGraphConstructionRequestContract request, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        SafetyGraphModel graph = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(SafetyGraphAssembler.Assemble), clock, logEventWriter, correlationId,
            () => Task.FromResult(SafetyGraphAssembler.Assemble(request)));

        SafetyGraphMetrics.GraphsConstructed.Add(1, new KeyValuePair<string, object?>("outcome", "Success"));

        logEventWriter.Write(new LogEvent(
            EventId: SafetyGraphGovernanceEvents.GraphConstructed,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Info,
            Message: $"SafetyGraphAssembler.Assemble produced {graph.Nodes.Count} node(s) and {graph.Edges.Count} edge(s).",
            Payload: new Dictionary<string, object?>
            {
                ["NodeCount"] = graph.Nodes.Count,
                ["EdgeCount"] = graph.Edges.Count,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return graph;
    }

    /// <summary>Infers a SafetyGraphInferenceResult via the real SafetyGraphInferenceEngine.Infer, recording a SafetyGraphInferencesComputed metric and a structured log event.</summary>
    public static async Task<SafetyGraphInferenceResult> InferWithTelemetryAsync(
        SafetyGraphModel graph, PredictiveSafetyEvaluationResultContract? predictiveResult, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        SafetyGraphInferenceResult inference = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(SafetyGraphInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(SafetyGraphInferenceEngine.Infer(graph, predictiveResult)));

        SafetyGraphMetrics.InferencesComputed.Add(1, new KeyValuePair<string, object?>("requiresEscalation", inference.RequiresGraphWideEscalation));

        logEventWriter.Write(new LogEvent(
            EventId: SafetyGraphGovernanceEvents.InferenceComputed,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Info,
            Message: $"SafetyGraphInferenceEngine.Infer produced priority {inference.OverallPriority} ({inference.AppliedRuleCodes.Count} rule(s) applied).",
            Payload: new Dictionary<string, object?>
            {
                ["RequiresGraphWideEscalation"] = inference.RequiresGraphWideEscalation,
                ["OverallPriority"] = inference.OverallPriority.ToString(),
                ["AppliedRuleCount"] = inference.AppliedRuleCodes.Count,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return inference;
    }
}
