// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Edges
//  File:           EnterpriseGraphEdge.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: a single directed edge in the EnterpriseGraph,
//                  from one node to another. Built exclusively via
//                  EnterpriseGraphBuilder, which validates both endpoints
//                  reference nodes that exist.
// ============================================================================
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Ids;

namespace Enterprise.Graph.Abstractions.Edges;

/// <summary>A single directed edge in the <see cref="Graph.EnterpriseGraph"/>.</summary>
public sealed record EnterpriseGraphEdge(
    EnterpriseNodeId FromNodeId,
    EnterpriseNodeId ToNodeId,
    EnterpriseEdgeType EdgeType,
    DateTime CreatedAtUtc);
