// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Construction
//  File:           SafetyGraphBuilder.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic, pure per-node/per-edge
//                  factory methods -- the only place NodeId/EdgeId composite
//                  keys are formatted, so every caller (SafetyGraphAssembler,
//                  tests) gets the exact same identity for the same
//                  underlying entity. Invents no new domain semantics: every
//                  field placed into a node's Properties is copied verbatim
//                  from the real upstream type (EventRoutingResult,
//                  SchedulingRuleDecision, PredictiveSafetyEvaluationResultContract,
//                  Facility Hazard/Severity types).
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.Scheduling.Abstractions.Rules;

namespace Shared.SafetyGraph.Abstractions.Construction;

/// <summary>Deterministic, pure per-node/per-edge factory methods for the Safety Graph.</summary>
public static class SafetyGraphBuilder
{
    /// <summary>Builds a Facility node keyed by "Facility:{facilityId}".</summary>
    public static Graph.SafetyGraphNode BuildFacilityNode(Guid facilityId, string facilityName)
    {
        ArgumentNullException.ThrowIfNull(facilityName);

        return new Graph.SafetyGraphNode(
            NodeId: $"Facility:{facilityId}",
            NodeType: Graph.SafetyGraphNodeType.Facility,
            Label: facilityName,
            Properties: new Dictionary<string, string> { ["FacilityId"] = facilityId.ToString() });
    }

    /// <summary>Builds an Employee node keyed by "Employee:{employeeId}".</summary>
    public static Graph.SafetyGraphNode BuildEmployeeNode(Guid employeeId)
    {
        return new Graph.SafetyGraphNode(
            NodeId: $"Employee:{employeeId}",
            NodeType: Graph.SafetyGraphNodeType.Employee,
            Label: employeeId.ToString(),
            Properties: new Dictionary<string, string> { ["EmployeeId"] = employeeId.ToString() });
    }

    /// <summary>Builds a Machine node keyed by "Machine:{machineId}".</summary>
    public static Graph.SafetyGraphNode BuildMachineNode(Guid machineId)
    {
        return new Graph.SafetyGraphNode(
            NodeId: $"Machine:{machineId}",
            NodeType: Graph.SafetyGraphNodeType.Machine,
            Label: machineId.ToString(),
            Properties: new Dictionary<string, string> { ["MachineId"] = machineId.ToString() });
    }

    /// <summary>Builds a Workflow node keyed by "Workflow:{workflowId}".</summary>
    public static Graph.SafetyGraphNode BuildWorkflowNode(string workflowId)
    {
        ArgumentNullException.ThrowIfNull(workflowId);

        return new Graph.SafetyGraphNode(
            NodeId: $"Workflow:{workflowId}",
            NodeType: Graph.SafetyGraphNodeType.Workflow,
            Label: workflowId,
            Properties: new Dictionary<string, string> { ["WorkflowId"] = workflowId });
    }

    /// <summary>Builds a Hazard node from a real PredictiveHazardDecision (Phase 15), keyed by "Hazard:{correlationId}".</summary>
    public static Graph.SafetyGraphNode BuildHazardNode(PredictiveHazardDecision decision, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(correlationId);

        return new Graph.SafetyGraphNode(
            NodeId: $"Hazard:{correlationId}",
            NodeType: Graph.SafetyGraphNodeType.Hazard,
            Label: $"{decision.PredictedHazardType}:{decision.PredictedSeverity}",
            Properties: new Dictionary<string, string>
            {
                ["HazardType"] = decision.PredictedHazardType.ToString(),
                ["Severity"] = decision.PredictedSeverity.ToString(),
                ["RequiresImmediateEscalation"] = decision.RequiresImmediateEscalation.ToString(),
                ["Priority"] = decision.Priority.ToString(),
            });
    }

    /// <summary>Builds a RoutingDecision node from a real EventRoutingResult (Phase 13), keyed by "RoutingDecision:{eventCode}:{facilityId}".</summary>
    public static Graph.SafetyGraphNode BuildRoutingDecisionNode(EventRoutingResult routingResult)
    {
        ArgumentNullException.ThrowIfNull(routingResult);

        return new Graph.SafetyGraphNode(
            NodeId: $"RoutingDecision:{routingResult.EventCode}:{routingResult.FacilityId}",
            NodeType: Graph.SafetyGraphNodeType.RoutingDecision,
            Label: routingResult.EventCode,
            Properties: new Dictionary<string, string>
            {
                ["EventCode"] = routingResult.EventCode,
                ["FacilityId"] = routingResult.FacilityId.ToString(),
                ["TriggeredAuditCount"] = routingResult.TriggeredAudits.Count.ToString(),
                ["ChangedComplianceElementCount"] = routingResult.ChangedComplianceElements.Count.ToString(),
            });
    }

    /// <summary>Builds a SchedulingDecision node from a real SchedulingRuleDecision (Phase 14), keyed by "SchedulingDecision:{correlationId}".</summary>
    public static Graph.SafetyGraphNode BuildSchedulingDecisionNode(SchedulingRuleDecision schedulingDecision, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(schedulingDecision);
        ArgumentNullException.ThrowIfNull(correlationId);

        return new Graph.SafetyGraphNode(
            NodeId: $"SchedulingDecision:{correlationId}",
            NodeType: Graph.SafetyGraphNodeType.SchedulingDecision,
            Label: $"RequiredActions={schedulingDecision.TotalRequiredActionCount}",
            Properties: new Dictionary<string, string>
            {
                ["TotalRequiredActionCount"] = schedulingDecision.TotalRequiredActionCount.ToString(),
                ["WorstCaseDueWithinHours"] = schedulingDecision.WorstCaseDueWithinHours.ToString(),
                ["RequiresImmediateScheduling"] = schedulingDecision.RequiresImmediateScheduling.ToString(),
            });
    }

    /// <summary>Builds a DiagnosticsAnomaly node from a real PredictiveAuditAnomaly (Phase 15's honest Diagnostics/Audit substitute), keyed by "DiagnosticsAnomaly:{correlationId}".</summary>
    public static Graph.SafetyGraphNode BuildDiagnosticsAnomalyNode(PredictiveAuditAnomaly anomaly, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(anomaly);
        ArgumentNullException.ThrowIfNull(correlationId);

        return new Graph.SafetyGraphNode(
            NodeId: $"DiagnosticsAnomaly:{correlationId}",
            NodeType: Graph.SafetyGraphNodeType.DiagnosticsAnomaly,
            Label: anomaly.AnomalyDetected ? "AnomalyDetected" : "NoAnomaly",
            Properties: new Dictionary<string, string>
            {
                ["AnomalyDetected"] = anomaly.AnomalyDetected.ToString(),
                ["Priority"] = anomaly.Priority.ToString(),
                ["AppliedRuleCount"] = anomaly.AppliedRuleCodes.Count.ToString(),
            });
    }

    /// <summary>Builds a PredictiveInference node from a real overall PredictivePriority (Phase 15), keyed by "PredictiveInference:{correlationId}".</summary>
    public static Graph.SafetyGraphNode BuildPredictiveInferenceNode(PredictivePriority overallPriority, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(correlationId);

        return new Graph.SafetyGraphNode(
            NodeId: $"PredictiveInference:{correlationId}",
            NodeType: Graph.SafetyGraphNodeType.PredictiveInference,
            Label: overallPriority.ToString(),
            Properties: new Dictionary<string, string> { ["OverallPriority"] = overallPriority.ToString() });
    }

    /// <summary>Builds a deterministic, directed edge keyed by "{SourceNodeId}->{TargetNodeId}:{EdgeType}".</summary>
    public static Graph.SafetyGraphEdge BuildEdge(string sourceNodeId, string targetNodeId, Graph.SafetyGraphEdgeType edgeType, string label)
    {
        ArgumentNullException.ThrowIfNull(sourceNodeId);
        ArgumentNullException.ThrowIfNull(targetNodeId);
        ArgumentNullException.ThrowIfNull(label);

        return new Graph.SafetyGraphEdge(
            EdgeId: $"{sourceNodeId}->{targetNodeId}:{edgeType}",
            SourceNodeId: sourceNodeId,
            TargetNodeId: targetNodeId,
            EdgeType: edgeType,
            Label: label);
    }
}
