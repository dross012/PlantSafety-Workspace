// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Nodes
//  File:           EnterpriseGraphNode.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: a single node in the EnterpriseGraph.
//                  SourceId is the underlying real domain entity's id
//                  (e.g. a FacilityId.Value/EmployeeId.Value/MachineId.Value)
//                  kept as a plain Guid rather than importing every
//                  upstream domain's strong id type -- NodeType is what
//                  disambiguates which domain SourceId belongs to. Built
//                  exclusively via EnterpriseGraphBuilder.
// ============================================================================
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Ids;

namespace Enterprise.Graph.Abstractions.Nodes;

/// <summary>A single node in the <see cref="Graph.EnterpriseGraph"/>.</summary>
public sealed record EnterpriseGraphNode(
    EnterpriseNodeId NodeId,
    EnterpriseNodeType NodeType,
    Guid SourceId,
    string Label,
    DateTime CreatedAtUtc);
