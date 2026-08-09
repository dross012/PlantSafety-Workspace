// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Ids
//  File:           EnterpriseRuleId.cs
//  Created:        2026-08-08
//  Description:    Phase D.5 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): strongly typed identifier for an
//                  EnterpriseRule. Mirrors the wrapper pattern established
//                  throughout this program (Shared.FacilityDomain's
//                  FacilityId, Enterprise.Graph's EnterpriseNodeId, etc.).
// ============================================================================
namespace Enterprise.RulesEngine.Abstractions.Ids;

/// <summary>Strongly typed identifier for an <c>EnterpriseRule</c>. Wraps a non-empty <see cref="Guid"/>.</summary>
public readonly record struct EnterpriseRuleId
{
    private EnterpriseRuleId(Guid value) => Value = value;

    /// <summary>The underlying identifier value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new, random <see cref="EnterpriseRuleId"/>.</summary>
    public static EnterpriseRuleId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/> as an <see cref="EnterpriseRuleId"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public static EnterpriseRuleId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("EnterpriseRuleId cannot be Guid.Empty.", nameof(value));
        }

        return new EnterpriseRuleId(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
