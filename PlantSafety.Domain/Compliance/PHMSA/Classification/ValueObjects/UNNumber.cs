// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           UNNumber.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object representing a DOT/UN identification number as assigned by
//                  49 CFR 172.101 Table (e.g. "UN1090" for acetone). Format is fixed: the
//                  literal prefix "UN" followed by exactly 4 digits.
//                  Additive-only evolution.
// ============================================================================
using System.Text.RegularExpressions;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

/// <summary>DOT/UN identification number, format "UN" + exactly 4 digits (e.g. "UN1090").</summary>
public readonly partial record struct UNNumber
{
    /// <summary>Canonical string form, e.g. "UN1090".</summary>
    public string Value { get; }

    /// <summary>Creates a UNNumber, validating the canonical "UN" + 4-digit format.</summary>
    public UNNumber(string value)
    {
        var candidate = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (!UNNumberPattern().IsMatch(candidate))
            throw new InvalidUNNumberException(value, CorrelationId.New(), CausationId.New());

        Value = candidate;
    }

    [GeneratedRegex(@"^UN\d{4}$")]
    private static partial Regex UNNumberPattern();

    /// <inheritdoc/>
    public override string ToString() => Value;
}
