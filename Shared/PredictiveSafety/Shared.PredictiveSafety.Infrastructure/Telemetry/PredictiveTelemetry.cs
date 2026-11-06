// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Telemetry
//  File:           PredictiveTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Wraps each of the five
//                  Predictive*InferenceEngine.Infer calls (all unchanged --
//                  still pure functions) with deterministic logging,
//                  metrics, and reliability reporting, via Shared.Telemetry's
//                  existing ReliabilityHooks/ILogEventWriter/PlantSafetyMetrics.
//                  Mirrors SchedulingTelemetry (Phase 14) exactly, one method
//                  per domain rather than one method total, since each
//                  domain's InferenceEngine has a distinct input/output
//                  shape.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;
using Shared.PredictiveSafety.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.PredictiveSafety.Infrastructure.Telemetry;

/// <summary>Instruments each Predictive*InferenceEngine.Infer call with deterministic logging, metrics, and reliability reporting.</summary>
public static class PredictiveTelemetry
{
    private const string Module = "PredictiveSafety";

    /// <summary>Infers a PredictiveHazardDecision via PredictiveHazardInferenceEngine, emitting telemetry.</summary>
    public static async Task<PredictiveHazardDecision> InferHazardWithTelemetryAsync(
        PredictiveHazardSignals signals, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        PredictiveHazardDecision decision = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(PredictiveHazardInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(PredictiveHazardInferenceEngine.Infer(signals)));

        RecordEvaluation(clock, logEventWriter, correlationId, "Hazard", decision.RequiresImmediateEscalation, decision.Priority.ToString(), decision.AppliedRuleCodes.Count);
        return decision;
    }

    /// <summary>Infers a PredictiveMaintenanceDecision via PredictiveMaintenanceInferenceEngine, emitting telemetry.</summary>
    public static async Task<PredictiveMaintenanceDecision> InferMaintenanceWithTelemetryAsync(
        PredictiveMaintenanceSignals signals, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        PredictiveMaintenanceDecision decision = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(PredictiveMaintenanceInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(PredictiveMaintenanceInferenceEngine.Infer(signals)));

        RecordEvaluation(clock, logEventWriter, correlationId, "Maintenance", decision.RequiresImmediateMaintenance, decision.Priority.ToString(), decision.AppliedRuleCodes.Count);
        return decision;
    }

    /// <summary>Infers a PredictiveRoutingDecision via PredictiveRoutingInferenceEngine, emitting telemetry.</summary>
    public static async Task<PredictiveRoutingDecision> InferRoutingWithTelemetryAsync(
        PredictiveRoutingSignals signals, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        PredictiveRoutingDecision decision = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(PredictiveRoutingInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(PredictiveRoutingInferenceEngine.Infer(signals)));

        RecordEvaluation(clock, logEventWriter, correlationId, "Routing", decision.RequiresPriorityRouting, decision.Priority.ToString(), decision.AppliedRuleCodes.Count);
        return decision;
    }

    /// <summary>Infers a PredictiveSchedulingDecision via PredictiveSchedulingInferenceEngine, emitting telemetry.</summary>
    public static async Task<PredictiveSchedulingDecision> InferSchedulingWithTelemetryAsync(
        PredictiveSchedulingSignals signals, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        PredictiveSchedulingDecision decision = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(PredictiveSchedulingInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(PredictiveSchedulingInferenceEngine.Infer(signals)));

        RecordEvaluation(clock, logEventWriter, correlationId, "Scheduling", decision.RequiresAcceleratedScheduling, decision.Priority.ToString(), decision.AppliedRuleCodes.Count);
        return decision;
    }

    /// <summary>Infers a PredictiveAuditAnomaly via PredictiveAuditInferenceEngine, emitting telemetry.</summary>
    public static async Task<PredictiveAuditAnomaly> InferAuditWithTelemetryAsync(
        PredictiveAuditSignals signals, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        PredictiveAuditAnomaly anomaly = await ReliabilityHooks.ExecuteAsync(
            Module, nameof(PredictiveAuditInferenceEngine.Infer), clock, logEventWriter, correlationId,
            () => Task.FromResult(PredictiveAuditInferenceEngine.Infer(signals)));

        RecordEvaluation(clock, logEventWriter, correlationId, "Audit", anomaly.AnomalyDetected, anomaly.Priority.ToString(), anomaly.AppliedRuleCodes.Count);
        return anomaly;
    }

    private static void RecordEvaluation(
        IClock clock, ILogEventWriter logEventWriter, string correlationId, string domain, bool requiresEscalation, string priority, int appliedRuleCount)
    {
        PredictiveMetrics.EvaluationsComputed.Add(1, new KeyValuePair<string, object?>("domain", domain));
        if (requiresEscalation)
        {
            PredictiveMetrics.EscalationsRequired.Add(1, new KeyValuePair<string, object?>("domain", domain));
        }

        logEventWriter.Write(new LogEvent(
            EventId: PredictiveGovernanceEvents.EvaluationComputed,
            Category: LogCategory.PredictiveSafety,
            Severity: LogSeverity.Info,
            Message: $"Predictive {domain} evaluation computed: requiresEscalation={requiresEscalation}, priority={priority}, {appliedRuleCount} rule(s) applied.",
            Payload: new Dictionary<string, object?>
            {
                ["Domain"] = domain,
                ["RequiresEscalation"] = requiresEscalation,
                ["Priority"] = priority,
                ["AppliedRuleCount"] = appliedRuleCount,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));
    }
}
