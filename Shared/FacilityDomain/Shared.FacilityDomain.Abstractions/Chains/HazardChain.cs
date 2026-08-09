// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Chains
//  File:           HazardChain.cs
//  Created:        2026-08-08
//  Description:    Phase C.9: an ordered, immutable multi-hazard event
//                  chain (e.g. Spill -> Fire -> Injury). Built exclusively
//                  via HazardChainBuilder -- callers read a HazardChain,
//                  they don't construct one directly.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

namespace Shared.FacilityDomain.Abstractions.Chains;

/// <summary>An ordered, immutable multi-hazard event chain.</summary>
public sealed record HazardChain(IReadOnlyList<HazardChainNode> Nodes)
{
    /// <summary>The number of stages in the chain.</summary>
    public int Length => Nodes.Count;

    /// <summary>The chain's originating stage.</summary>
    public PropagationStage OriginStage => Nodes[0].Stage;

    /// <summary>The chain's terminal (most recent) stage.</summary>
    public PropagationStage TerminalStage => Nodes[^1].Stage;

    /// <summary>The worst-case severity across every node in the chain.</summary>
    public SeverityLevel OverallSeverity => Nodes
        .Select(n => n.Severity)
        .OrderByDescending(SeverityMetadata.WeightOf)
        .First();
}
