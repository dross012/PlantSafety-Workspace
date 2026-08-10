// ============================================================================
//  Project:        Shared.WorkflowEngine.Infrastructure
//  Module:         Telemetry
//  File:           WorkflowEngineTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 8 (Observability, Telemetry, Diagnostics, and
//                  Governance) deliverable. Wraps Shared.WorkflowEngine.
//                  Abstractions.Engine.WorkflowEngine.Evaluate (Phase C.7,
//                  unchanged by this phase -- still a pure function) with
//                  deterministic logging, metrics, and reliability
//                  reporting, via Shared.Telemetry's existing
//                  ReliabilityHooks/ILogEventWriter/PlantSafetyMetrics. Does
//                  not alter WorkflowEngine.Evaluate's decision logic in any
//                  way -- this is a decorator, not a modification. This is
//                  the one real, live execution path instrumented in Phase
//                  8 (WorkflowOrchestrationController's Evaluate action was
//                  already wired to this engine before this phase; FacilityTelemetry
//                  and AuditTelemetry below wrap dormant capabilities with
//                  no live caller in this program today -- see their own
//                  header comments).
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;
using Shared.Telemetry.Abstractions.Reliability;
using Shared.WorkflowEngine.Abstractions.Engine;

namespace Shared.WorkflowEngine.Infrastructure.Telemetry;

/// <summary>Instruments WorkflowEngine.Evaluate with deterministic logging, metrics, and reliability reporting.</summary>
public static class WorkflowEngineTelemetry
{
    private const string Module = "WorkflowEngine";

    /// <summary>
    /// Evaluates <paramref name="context"/> via the real WorkflowEngine.Evaluate, recording a
    /// WorkflowDecisionsEvaluated metric (tagged by the resulting SeverityLevel) and a structured
    /// WORKFLOW_DECISION_EVALUATED log event. Never alters the evaluation's result.
    /// </summary>
    public static async Task<WorkflowDecision> EvaluateWithTelemetryAsync(
        WorkflowContext context,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(context);

        WorkflowDecision decision = await ReliabilityHooks.ExecuteAsync(
            Module,
            nameof(WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate),
            clock,
            logEventWriter,
            correlationId,
            () => Task.FromResult(WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context)));

        PlantSafetyMetrics.WorkflowDecisionsEvaluated.Add(1, new KeyValuePair<string, object?>("severityLevel", context.SeverityLevel.ToString()));

        logEventWriter.Write(new LogEvent(
            EventId: "WORKFLOW_DECISION_EVALUATED",
            Category: LogCategory.WorkflowEngine,
            Severity: LogSeverity.Info,
            Message: $"WorkflowEngine.Evaluate produced {decision.RequiredActions.Count} action(s), {decision.SopTriggers.Count} SOP trigger(s), {decision.PermitTriggers.Count} permit trigger(s), {decision.EvacuationTriggers.Count} evacuation trigger(s) for {context.SeverityLevel} {context.HazardType}.",
            Payload: new Dictionary<string, object?>
            {
                ["HazardType"] = context.HazardType.ToString(),
                ["SeverityLevel"] = context.SeverityLevel.ToString(),
                ["RequiredActionCount"] = decision.RequiredActions.Count,
                ["SopTriggerCount"] = decision.SopTriggers.Count,
                ["PermitTriggerCount"] = decision.PermitTriggers.Count,
                ["EvacuationTriggerCount"] = decision.EvacuationTriggers.Count,
                ["NotificationRoutingCount"] = decision.NotificationRoutings.Count,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return decision;
    }
}
