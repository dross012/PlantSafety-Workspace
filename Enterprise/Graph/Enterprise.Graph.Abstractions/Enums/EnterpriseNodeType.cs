// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Enums
//  File:           EnterpriseNodeType.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: the vocabulary of entity kinds the
//                  EnterpriseGraph unifies -- one node type per real
//                  bounded context from Paths B/C. Additive-only: new
//                  members may be appended, existing members must never be
//                  renumbered or removed once published.
// ============================================================================
namespace Enterprise.Graph.Abstractions.Enums;

/// <summary>The kind of real-world entity an <see cref="Nodes.EnterpriseGraphNode"/> represents.</summary>
public enum EnterpriseNodeType
{
    /// <summary>A Shared.FacilityDomain facility (Path B).</summary>
    Facility,

    /// <summary>A Shared.EmployeeDomain employee (Path C).</summary>
    Employee,

    /// <summary>A Shared.MachineDomain machine (Path C).</summary>
    Machine,

    /// <summary>A Shared.FacilityDomain hazard classification/occurrence (Path B/C).</summary>
    Hazard,

    /// <summary>A Shared.WorkflowEngine workflow decision (Path C).</summary>
    Workflow,

    /// <summary>A real domain event correlated into the graph (Path B/C).</summary>
    Event,
}
