// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Orchestration
//  File:           PredictiveSafetyEventEmitter.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Emits a composed
//                  PredictiveSafetyEvaluationResultContract as a structured
//                  LogEvent -- the unified event schema (Phase 12 precedent:
//                  no new event bus/envelope type, the existing LogEvent.
//                  Payload IS the unified event schema), mirroring
//                  SchedulingEventEmitter (Phase 14) exactly. Pure emission:
//                  never recomputes any decision.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Orchestration;

/// <summary>Emits a composed PredictiveSafetyEvaluationResultContract as a structured LogEvent.</summary>
public static class PredictiveSafetyEventEmitter
{
    private const string Module = "PredictiveSafety";

    /// <summary>Writes one PREDICTIVE_EVALUATION_EVENT_PUBLISHED log event carrying <paramref name="result"/>.</summary>
    public static void Emit(PredictiveSafetyEvaluationResultContract result, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: PredictiveGovernanceEvents.EvaluationEventPublished,
            Category: LogCategory.PredictiveSafety,
            Severity: LogSeverity.Info,
            Message: $"Published Predictive Safety evaluation for tenant \"{result.Metadata.TenantId}\": overall priority {result.OverallPriority}.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["OverallPriority"] = result.OverallPriority.ToString(),
                ["HazardRequiresEscalation"] = result.Hazard.RequiresImmediateEscalation,
                ["MaintenanceRequiresImmediate"] = result.Maintenance.RequiresImmediateMaintenance,
                ["RoutingRequiresPriority"] = result.Routing.RequiresPriorityRouting,
                ["SchedulingRequiresAcceleration"] = result.Scheduling.RequiresAcceleratedScheduling,
                ["AuditAnomalyDetected"] = result.Audit.AnomalyDetected,
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: Module,
            OccurredAtUtc: result.Metadata.IssuedAtUtc));
    }
}
