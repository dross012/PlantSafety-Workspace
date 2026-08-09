// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Enums
//  File:           EnterpriseEdgeType.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: the vocabulary of relationships between
//                  EnterpriseGraph nodes. Additive-only: new members may be
//                  appended, existing members must never be renumbered or
//                  removed once published.
// ============================================================================
namespace Enterprise.Graph.Abstractions.Enums;

/// <summary>The kind of relationship an <see cref="Edges.EnterpriseGraphEdge"/> represents.</summary>
public enum EnterpriseEdgeType
{
    /// <summary>An Employee supervises another Employee.</summary>
    Supervises,

    /// <summary>An Employee operates a Machine.</summary>
    Operates,

    /// <summary>A node is physically located within a Facility (or Facility hierarchy level).</summary>
    LocatedIn,

    /// <summary>A Hazard causes another Hazard or Event (mirrors Shared.FacilityDomain's PropagationStage chains).</summary>
    Causes,

    /// <summary>A Workflow mitigates a Hazard.</summary>
    Mitigates,

    /// <summary>A Hazard propagates to another Hazard (a specific case of Causes, kept distinct for graph queries that care about propagation chains specifically).</summary>
    Propagates,
}
