// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           DOTSpec.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object wrapping a DOT packaging specification code as defined by
//                  49 CFR Part 178 (e.g. "1A1" steel drum, "3A" steel jerrican, "4G" fiberboard
//                  box, "11G" fiberboard IBC, "31HA1" composite IBC). Validated as a non-empty,
//                  2-8 character alphanumeric code.
//                  Additive-only evolution.
// ============================================================================
using System.Text.RegularExpressions;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;

/// <summary>DOT packaging specification code (49 CFR Part 178), e.g. "1A1", "3A", "4G", "31HA1".</summary>
public readonly partial record struct DOTSpec
{
    /// <summary>Canonical, upper-cased spec code, e.g. "1A1".</summary>
    public string Value { get; }

    /// <summary>Creates a DOTSpec, validating a 2-8 character alphanumeric format.</summary>
    public DOTSpec(string value)
    {
        var candidate = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (!DOTSpecPattern().IsMatch(candidate))
            throw new InvalidDOTSpecException(value, CorrelationId.New(), CausationId.New());

        Value = candidate;
    }

    [GeneratedRegex(@"^[A-Z0-9]{2,8}$")]
    private static partial Regex DOTSpecPattern();

    /// <inheritdoc/>
    public override string ToString() => Value;
}
