// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Graph
//  File:           SafetyGraphEdge.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The unified edge-type vocabulary.
//                  EdgeId is a deterministic composite of SourceNodeId,
//                  TargetNodeId, and EdgeType (see SafetyGraphBuilder.BuildEdge) --
//                  never a random Guid -- so the same relationship always
//                  produces the same edge identity.
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Graph;

/// <summary>The unified edge-type vocabulary the Safety Graph represents.</summary>
public enum SafetyGraphEdgeType
{
    /// <summary>Source causes/triggers target (e.g. Hazard -> RoutingDecision).</summary>
    Causal,

    /// <summary>Source depends on target's completion or state (e.g. RoutingDecision -> SchedulingDecision).</summary>
    Dependency,

    /// <summary>Source precedes or follows target in time.</summary>
    Temporal,

    /// <summary>Source is physically located at/within target (e.g. Machine -> Facility).</summary>
    Spatial,

    /// <summary>Source is organizationally assigned to/part of target (e.g. Employee -> Facility).</summary>
    Organizational,

    /// <summary>Source is a predictive-inference relationship to target (e.g. PredictiveInference -> Hazard).</summary>
    Predictive,
}

/// <summary>A single, immutable, directed edge in a SafetyGraph.</summary>
public sealed record SafetyGraphEdge(
    string EdgeId,
    string SourceNodeId,
    string TargetNodeId,
    SafetyGraphEdgeType EdgeType,
    string Label);
