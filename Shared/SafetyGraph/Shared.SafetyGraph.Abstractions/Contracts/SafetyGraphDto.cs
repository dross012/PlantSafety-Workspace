// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Contracts
//  File:           SafetyGraphDto.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Flat, serialization-friendly DTOs
//                  projected from SafetyGraphEvaluationResultContract for
//                  API/UI consumption -- deterministic mapping only.
// ============================================================================
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Contracts;

/// <summary>Flat DTO projection of a SafetyGraphNode.</summary>
public sealed record SafetyGraphNodeDto(string NodeId, string NodeType, string Label);

/// <summary>Flat DTO projection of a SafetyGraphEdge.</summary>
public sealed record SafetyGraphEdgeDto(string EdgeId, string SourceNodeId, string TargetNodeId, string EdgeType, string Label);

/// <summary>Flat DTO projection of a SafetyGraphEvaluationResultContract.</summary>
public sealed record SafetyGraphEvaluationDto(
    IReadOnlyList<SafetyGraphNodeDto> Nodes,
    IReadOnlyList<SafetyGraphEdgeDto> Edges,
    bool RequiresGraphWideEscalation,
    string OverallPriority,
    IReadOnlyList<string> AppliedRuleCodes);

/// <summary>Deterministic, pure mapping from Safety Graph domain types to their DTO projections.</summary>
public static class SafetyGraphDtoMapper
{
    /// <summary>Projects <paramref name="result"/> into a SafetyGraphEvaluationDto.</summary>
    public static SafetyGraphEvaluationDto ToDto(Graph.SafetyGraphEvaluationResultContract result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var nodes = new List<SafetyGraphNodeDto>(result.Graph.Nodes.Count);
        foreach (SafetyGraphNode node in result.Graph.Nodes)
        {
            nodes.Add(new SafetyGraphNodeDto(node.NodeId, node.NodeType.ToString(), node.Label));
        }

        var edges = new List<SafetyGraphEdgeDto>(result.Graph.Edges.Count);
        foreach (SafetyGraphEdge edge in result.Graph.Edges)
        {
            edges.Add(new SafetyGraphEdgeDto(edge.EdgeId, edge.SourceNodeId, edge.TargetNodeId, edge.EdgeType.ToString(), edge.Label));
        }

        return new SafetyGraphEvaluationDto(
            Nodes: nodes,
            Edges: edges,
            RequiresGraphWideEscalation: result.Inference.RequiresGraphWideEscalation,
            OverallPriority: result.Inference.OverallPriority.ToString(),
            AppliedRuleCodes: result.Inference.AppliedRuleCodes);
    }
}
