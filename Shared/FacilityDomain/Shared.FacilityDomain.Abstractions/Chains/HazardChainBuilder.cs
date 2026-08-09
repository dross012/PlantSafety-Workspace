// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Chains
//  File:           HazardChainBuilder.cs
//  Created:        2026-08-08
//  Description:    Phase C.9: a fluent, mutable builder for HazardChain --
//                  the only supported way to construct one. Deterministic:
//                  nodes are appended in exactly the order AddStage is
//                  called, no reordering or inference.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

namespace Shared.FacilityDomain.Abstractions.Chains;

/// <summary>A fluent, mutable builder for an immutable <see cref="HazardChain"/>.</summary>
public sealed class HazardChainBuilder
{
    private readonly List<HazardChainNode> _nodes = [];

    /// <summary>Appends a stage to the chain being built. Returns this builder for fluent chaining.</summary>
    public HazardChainBuilder AddStage(PropagationStage stage, SeverityLevel severity, DateTime occurredAtUtc, string? description = null)
    {
        _nodes.Add(new HazardChainNode(stage, severity, occurredAtUtc, description));
        return this;
    }

    /// <summary>Builds the immutable <see cref="HazardChain"/> from every stage added so far.</summary>
    /// <exception cref="InvalidOperationException">No stages were added.</exception>
    public HazardChain Build()
    {
        if (_nodes.Count == 0)
        {
            throw new InvalidOperationException("A hazard chain must contain at least one stage.");
        }

        return new HazardChain([.. _nodes]);
    }
}
