// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           ProperShippingName.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object wrapping the DOT "proper shipping name" (49 CFR 172.101 Table,
//                  Column 2) for a hazardous material, e.g. "Acetone" or "Corrosive liquid, n.o.s.".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

/// <summary>DOT proper shipping name for a hazardous material.</summary>
public readonly record struct ProperShippingName
{
    /// <summary>Maximum permitted length for a proper shipping name.</summary>
    public const int MaxLength = 200;

    /// <summary>Trimmed, non-empty proper shipping name text.</summary>
    public string Value { get; }

    /// <summary>Creates a ProperShippingName, validating non-empty and max-length constraints.</summary>
    public ProperShippingName(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("ProperShippingName must not be empty.", nameof(value));
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ProperShippingName must not exceed {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
