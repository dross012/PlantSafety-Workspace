// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainRule.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Abstract base for named domain rules. A rule encapsulates a single
//      invariant check and its violation message. Rules compose via And/Or
//      when multiple invariants must be combined.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Rules;

/// <summary>Abstract base for a single domain invariant rule over <typeparamref name="T"/>.</summary>
public abstract class DomainRule<T>
{
    /// <summary>Returns <see langword="true"/> when the rule is satisfied by <paramref name="value"/>.</summary>
    public abstract bool IsSatisfiedBy(T value);

    /// <summary>Throws <see cref="InvalidOperationException"/> when the rule is violated.</summary>
    public void EnforceOn(T value)
    {
        if (!IsSatisfiedBy(value))
            throw new InvalidOperationException(GetViolationMessage(value));
    }

    /// <summary>Describes the rule violation for diagnostics and exception messages.</summary>
    protected abstract string GetViolationMessage(T value);
}
