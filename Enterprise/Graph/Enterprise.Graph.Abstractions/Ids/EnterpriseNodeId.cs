// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Ids
//  File:           EnterpriseNodeId.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase D.1 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): strongly typed identifier for a node in
//                  the EnterpriseGraph. Distinct from the identifier of
//                  whatever real domain entity a node represents (see
//                  EnterpriseGraphNode.SourceId) -- a node's graph identity
//                  and its underlying domain identity are two different
//                  concerns. Mirrors Shared.FacilityDomain's FacilityId
//                  wrapper pattern.
// ============================================================================
namespace Enterprise.Graph.Abstractions.Ids;

/// <summary>Strongly typed identifier for a node in the <see cref="Graph.EnterpriseGraph"/>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct EnterpriseNodeId
{
    private EnterpriseNodeId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="EnterpriseNodeId"/>.</summary>
    public static EnterpriseNodeId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as an <see cref="EnterpriseNodeId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static EnterpriseNodeId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("EnterpriseNodeId cannot be Guid.Empty.", nameof(value));
        }

        return new EnterpriseNodeId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
