// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Orchestration
//  File:           PredictiveSafetyEvaluationPipeline.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic composition step
//                  that evaluates all five prediction domains, in a fixed,
//                  never-varying order: Hazard -> Maintenance -> Routing ->
//                  Scheduling -> Audit. Each domain's real, upstream
//                  cross-module data (EventRoutingResult, MachineMaintenanceProfile,
//                  SchedulingRuleDecision) is extracted into that domain's
//                  Signals record, evaluated via PredictiveTelemetry (which
//                  wraps the pure Predictive*InferenceEngine.Infer calls),
//                  and each decision's escalation flag is threaded forward
//                  into every later domain's Signals -- satisfying "consume
//                  hazard/maintenance/routing predictions" without any
//                  domain re-deriving another domain's decision itself.
//                  OverallPriority is the worst-case-wins maximum across all
//                  five domain priorities -- never averaged, never weighted.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;
using Shared.PredictiveSafety.Infrastructure.Telemetry;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Orchestration;

/// <summary>Deterministically composes all five prediction domains, in fixed order, into a PredictiveSafetyEvaluationResultContract.</summary>
public static class PredictiveSafetyEvaluationPipeline
{
    /// <summary>
    /// Evaluates <paramref name="request"/> across Hazard, Maintenance, Routing, Scheduling, and
    /// Audit, in that fixed order, threading each domain's escalation flag into every later domain.
    /// </summary>
    public static async Task<PredictiveSafetyEvaluationResultContract> EvaluateAsync(
        PredictiveSafetyEvaluationRequestContract request,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(request);

        string correlationId = request.Metadata.CorrelationId;
        EventRoutingResult? routingResult = request.RoutingResult;
        SchedulingRuleDecision? schedulingDecision = request.SchedulingDecision;

        var hazardSignals = new PredictiveHazardSignals(
            EventType: request.EventType,
            Description: request.EventDescription,
            EmployeeExposureCount: request.EmployeeExposureCount,
            MachineFailureCount: request.MachineFailureCount,
            EventChainLength: request.EventChainLength,
            RoutingTriggeredAuditCount: routingResult?.TriggeredAudits.Count ?? 0,
            RoutingChangedComplianceElementCount: routingResult?.ChangedComplianceElements.Count ?? 0,
            SchedulingRequiresImmediateAction: schedulingDecision?.RequiresImmediateScheduling ?? false,
            RecentReliabilityFailureCount: request.RecentReliabilityFailureCount);
        PredictiveHazardDecision hazard = await PredictiveTelemetry.InferHazardWithTelemetryAsync(hazardSignals, clock, logEventWriter, correlationId);

        var maintenanceSignals = new PredictiveMaintenanceSignals(
            MachineId: request.MachineId,
            IsMaintenanceOverdue: request.MaintenanceProfile?.IsOverdue(request.AsOfUtc) ?? false,
            RecentFailureCount: request.RecentMachineFailureCount,
            SchedulingWorstCaseDueWithinHours: schedulingDecision?.WorstCaseDueWithinHours ?? int.MaxValue,
            RecentReliabilityFailureCount: request.RecentReliabilityFailureCount);
        PredictiveMaintenanceDecision maintenance = await PredictiveTelemetry.InferMaintenanceWithTelemetryAsync(maintenanceSignals, clock, logEventWriter, correlationId);

        var routingSignals = new PredictiveRoutingSignals(
            EventCode: routingResult?.EventCode ?? request.EventType,
            FacilityId: routingResult?.FacilityId ?? Guid.Empty,
            TriggeredAuditCount: routingResult?.TriggeredAudits.Count ?? 0,
            ChangedComplianceElementCount: routingResult?.ChangedComplianceElements.Count ?? 0,
            HazardPredictionRequiresEscalation: hazard.RequiresImmediateEscalation,
            MaintenancePredictionRequiresImmediate: maintenance.RequiresImmediateMaintenance);
        PredictiveRoutingDecision routing = await PredictiveTelemetry.InferRoutingWithTelemetryAsync(routingSignals, clock, logEventWriter, correlationId);

        var schedulingSignals = new PredictiveSchedulingSignals(
            TotalRequiredActionCount: schedulingDecision?.TotalRequiredActionCount ?? 0,
            WorstCaseDueWithinHours: schedulingDecision?.WorstCaseDueWithinHours ?? int.MaxValue,
            HazardPredictionRequiresEscalation: hazard.RequiresImmediateEscalation,
            MaintenancePredictionRequiresImmediate: maintenance.RequiresImmediateMaintenance,
            RoutingPredictionRequiresPriority: routing.RequiresPriorityRouting);
        PredictiveSchedulingDecision scheduling = await PredictiveTelemetry.InferSchedulingWithTelemetryAsync(schedulingSignals, clock, logEventWriter, correlationId);

        var auditSignals = new PredictiveAuditSignals(
            CorrelationId: correlationId,
            TotalLogEventCount: request.TotalLogEventCount,
            WarningOrAboveLogEventCount: request.WarningOrAboveLogEventCount,
            DistinctEventIdCount: request.DistinctEventIdCount,
            GovernanceViolationCount: request.GovernanceViolationCount);
        PredictiveAuditAnomaly audit = await PredictiveTelemetry.InferAuditWithTelemetryAsync(auditSignals, clock, logEventWriter, correlationId);

        PredictivePriority overallPriority = Max(hazard.Priority, maintenance.Priority, routing.Priority, scheduling.Priority, audit.Priority);

        return new PredictiveSafetyEvaluationResultContract(request.Metadata, hazard, maintenance, routing, scheduling, audit, overallPriority);
    }

    private static PredictivePriority Max(params PredictivePriority[] priorities)
    {
        PredictivePriority worst = PredictivePriority.Low;
        foreach (PredictivePriority priority in priorities)
        {
            if (priority > worst)
            {
                worst = priority;
            }
        }

        return worst;
    }
}
