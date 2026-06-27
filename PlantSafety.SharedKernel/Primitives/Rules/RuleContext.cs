/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               RuleContext.cs
    DESCRIPTION:        Canonical rule context primitive. Provides a strongly-typed, immutable context bag for rule
                        evaluation. MUST remain additive-only and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Collections.Generic;

namespace PlantSafety.SharedKernel.Primitives.Rules;

/// <summary>
/// Canonical rule context bag.
/// </summary>
public sealed record RuleContext
{
    /// <summary>
    /// Arbitrary key/value metadata for rule evaluation.
    /// </summary>
    public IReadOnlyDictionary<string, object> Items { get; }

    public RuleContext(IDictionary<string, object>? items = null)
    {
        Items = items is null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(items);
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (Items.TryGetValue(key, out var raw) && raw is T cast)
        {
            value = cast;
            return true;
        }

        value = default;
        return false;
    }
}
