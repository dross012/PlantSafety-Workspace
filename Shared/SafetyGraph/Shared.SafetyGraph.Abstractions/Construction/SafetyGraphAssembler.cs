// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Construction
//  File:           SafetyGraphAssembler.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministically assembles a full
//                  SafetyGraph from a SafetyGraphConstructionRequestContract:
//                  builds every present node via SafetyGraphBuilder, wires
//                  the fixed set of relationship edges below, then sorts
//                  Nodes by NodeId (ordinal) and Edges by (SourceNodeId,
//                  TargetNodeId, EdgeType, EdgeId) (ordinal) -- deterministic
//                  node/edge ordering, satisfying this phase's own
//                  requirement. Which nodes/edges appear depends only on
//                  which optional inputs are present in the request (never
//                  on caller identity, tenant, or any other non-deterministic
//                  factor).
//
//                  Fixed edge set (only wired when both endpoints exist):
//                    Employee   -Organizational-> Facility
//                    Machine    -Spatial->        Facility
//                    Workflow   -Dependency->      RoutingDecision
//                    Hazard     -Causal->          RoutingDecision
//                    Hazard     -Causal->          Machine
//                    RoutingDecision -Dependency-> SchedulingDecision
//                    SchedulingDecision -Temporal-> PredictiveInference
//                    PredictiveInference -Predictive-> Hazard
//                    PredictiveInference -Predictive-> DiagnosticsAnomaly
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Construction;

/// <summary>Deterministically assembles a full SafetyGraph from raw cross-module inputs.</summary>
public static class SafetyGraphAssembler
{
    /// <summary>Assembles a SafetyGraph from <paramref name="request"/>. Same input always produces the same output.</summary>
    public static SafetyGraphModel Assemble(SafetyGraphConstructionRequestContract request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string correlationId = request.Metadata.CorrelationId;
        var nodes = new List<SafetyGraphNode>();
        var edges = new List<SafetyGraphEdge>();

        SafetyGraphNode facilityNode = SafetyGraphBuilder.BuildFacilityNode(request.FacilityId, request.FacilityName);
        nodes.Add(facilityNode);

        SafetyGraphNode? employeeNode = null;
        if (request.EmployeeId is Guid employeeId)
        {
            employeeNode = SafetyGraphBuilder.BuildEmployeeNode(employeeId);
            nodes.Add(employeeNode);
            edges.Add(SafetyGraphBuilder.BuildEdge(employeeNode.NodeId, facilityNode.NodeId, SafetyGraphEdgeType.Organizational, "EmployeeAssignedToFacility"));
        }

        SafetyGraphNode machineNode = SafetyGraphBuilder.BuildMachineNode(request.MachineId);
        nodes.Add(machineNode);
        edges.Add(SafetyGraphBuilder.BuildEdge(machineNode.NodeId, facilityNode.NodeId, SafetyGraphEdgeType.Spatial, "MachineLocatedAtFacility"));

        SafetyGraphNode? workflowNode = null;
        if (request.WorkflowId is not null)
        {
            workflowNode = SafetyGraphBuilder.BuildWorkflowNode(request.WorkflowId);
            nodes.Add(workflowNode);
        }

        SafetyGraphNode? routingNode = null;
        if (request.RoutingResult is not null)
        {
            routingNode = SafetyGraphBuilder.BuildRoutingDecisionNode(request.RoutingResult);
            nodes.Add(routingNode);

            if (workflowNode is not null)
            {
                edges.Add(SafetyGraphBuilder.BuildEdge(workflowNode.NodeId, routingNode.NodeId, SafetyGraphEdgeType.Dependency, "WorkflowDrivesRouting"));
            }
        }

        SafetyGraphNode? schedulingNode = null;
        if (request.SchedulingDecision is not null)
        {
            schedulingNode = SafetyGraphBuilder.BuildSchedulingDecisionNode(request.SchedulingDecision, correlationId);
            nodes.Add(schedulingNode);

            if (routingNode is not null)
            {
                edges.Add(SafetyGraphBuilder.BuildEdge(routingNode.NodeId, schedulingNode.NodeId, SafetyGraphEdgeType.Dependency, "RoutingDrivesScheduling"));
            }
        }

        SafetyGraphNode? hazardNode = null;
        SafetyGraphNode? predictiveNode = null;
        SafetyGraphNode? diagnosticsAnomalyNode = null;
        if (request.PredictiveResult is not null)
        {
            PredictiveSafetyEvaluationResultContract predictiveResult = request.PredictiveResult;

            hazardNode = SafetyGraphBuilder.BuildHazardNode(predictiveResult.Hazard, correlationId);
            nodes.Add(hazardNode);
            if (routingNode is not null)
            {
                edges.Add(SafetyGraphBuilder.BuildEdge(hazardNode.NodeId, routingNode.NodeId, SafetyGraphEdgeType.Causal, "HazardTriggersRouting"));
            }

            edges.Add(SafetyGraphBuilder.BuildEdge(hazardNode.NodeId, machineNode.NodeId, SafetyGraphEdgeType.Causal, "HazardAffectsMachine"));

            predictiveNode = SafetyGraphBuilder.BuildPredictiveInferenceNode(predictiveResult.OverallPriority, correlationId);
            nodes.Add(predictiveNode);
            if (schedulingNode is not null)
            {
                edges.Add(SafetyGraphBuilder.BuildEdge(schedulingNode.NodeId, predictiveNode.NodeId, SafetyGraphEdgeType.Temporal, "SchedulingPrecedesPrediction"));
            }

            edges.Add(SafetyGraphBuilder.BuildEdge(predictiveNode.NodeId, hazardNode.NodeId, SafetyGraphEdgeType.Predictive, "PredictionInformsHazard"));

            diagnosticsAnomalyNode = SafetyGraphBuilder.BuildDiagnosticsAnomalyNode(predictiveResult.Audit, correlationId);
            nodes.Add(diagnosticsAnomalyNode);
            edges.Add(SafetyGraphBuilder.BuildEdge(predictiveNode.NodeId, diagnosticsAnomalyNode.NodeId, SafetyGraphEdgeType.Predictive, "PredictionInformsDiagnostics"));
        }

        IReadOnlyList<SafetyGraphNode> sortedNodes = nodes
            .OrderBy(node => node.NodeId, StringComparer.Ordinal)
            .ToArray();

        IReadOnlyList<SafetyGraphEdge> sortedEdges = edges
            .OrderBy(edge => edge.SourceNodeId, StringComparer.Ordinal)
            .ThenBy(edge => edge.TargetNodeId, StringComparer.Ordinal)
            .ThenBy(edge => edge.EdgeType)
            .ThenBy(edge => edge.EdgeId, StringComparer.Ordinal)
            .ToArray();

        return new SafetyGraphModel(request.Metadata, sortedNodes, sortedEdges);
    }
}
