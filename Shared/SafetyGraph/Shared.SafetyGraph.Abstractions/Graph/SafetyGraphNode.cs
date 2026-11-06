// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Graph
//  File:           SafetyGraphNode.cs
//  Created:        2026-08-10
//  Description:    Phase 16 (Safety Graph Model) deliverable. The unified
//                  node vocabulary spanning every cross-module entity/decision
//                  this graph represents. NodeId is a deterministic,
//                  stable composite key (never a random Guid) so the same
//                  underlying entity always produces the same node identity
//                  across separate graph constructions -- see
//                  SafetyGraphBuilder.cs for the exact per-type key formats.
//                  Properties is a plain string-keyed, string-valued
//                  dictionary (never object?) so two nodes are trivially,
//                  deterministically comparable and serializable without a
//                  custom equality/serialization surface.
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Graph;

/// <summary>The unified node-type vocabulary the Safety Graph represents.</summary>
public enum SafetyGraphNodeType
{
    /// <summary>A facility (Shared.FacilityDomain).</summary>
    Facility,

    /// <summary>A predicted or observed hazard (Shared.FacilityDomain / Shared.PredictiveSafety).</summary>
    Hazard,

    /// <summary>An employee (Shared.EmployeeDomain).</summary>
    Employee,

    /// <summary>A machine (Shared.MachineDomain).</summary>
    Machine,

    /// <summary>A workflow decision (Shared.WorkflowEngine).</summary>
    Workflow,

    /// <summary>A routing decision (Shared.SafetyData, Phase 13).</summary>
    RoutingDecision,

    /// <summary>A scheduling decision (Shared.Scheduling, Phase 14).</summary>
    SchedulingDecision,

    /// <summary>A diagnostics anomaly -- the same LogEvent-derived substitute Phase 15's PredictiveAuditAnomaly documents (no standalone Diagnostics domain exists in this program).</summary>
    DiagnosticsAnomaly,

    /// <summary>A predictive inference outcome (Shared.PredictiveSafety, Phase 15).</summary>
    PredictiveInference,
}

/// <summary>A single, immutable node in a SafetyGraph.</summary>
public sealed record SafetyGraphNode(
    string NodeId,
    SafetyGraphNodeType NodeType,
    string Label,
    IReadOnlyDictionary<string, string> Properties);
