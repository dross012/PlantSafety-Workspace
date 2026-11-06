// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Orchestration
//  File:           SafetyGraphEvaluationPipeline.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The deterministic composition step
//                  that constructs a graph, then infers its escalation
//                  outcome, in that fixed order: Construct -> Infer. Mirrors
//                  SchedulingEvaluationPipeline/PredictiveSafetyEvaluationPipeline
//                  (Phases 14/15) exactly.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Inference;
using Shared.SafetyGraph.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Orchestration;

/// <summary>Deterministically composes graph construction and graph inference, in fixed order.</summary>
public static class SafetyGraphEvaluationPipeline
{
    /// <summary>Evaluates <paramref name="request"/>: constructs the graph, then infers its escalation outcome, in that fixed order.</summary>
    public static async Task<SafetyGraphEvaluationResultContract> EvaluateAsync(
        SafetyGraphEvaluationRequestContract request,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(request);

        string correlationId = request.Metadata.CorrelationId;

        SafetyGraphModel graph = await SafetyGraphTelemetry.ConstructWithTelemetryAsync(request.ConstructionRequest, clock, logEventWriter, correlationId);
        SafetyGraphInferenceResult inference = await SafetyGraphTelemetry.InferWithTelemetryAsync(graph, request.ConstructionRequest.PredictiveResult, clock, logEventWriter, correlationId);

        return new SafetyGraphEvaluationResultContract(request.Metadata, graph, inference);
    }
}
