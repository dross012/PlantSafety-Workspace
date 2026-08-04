// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CognitiveLoadLedger aggregate root
//                  (per tenant/site/shift cognitive-workload measurement ledger).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CognitiveLoadLedger aggregate root.</summary>
public readonly record struct CognitiveLoadLedgerId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CognitiveLoadLedgerId wrapping a non-empty GUID.</summary>
    public CognitiveLoadLedgerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CognitiveLoadLedgerId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CognitiveLoadLedgerId using Uuid7.</summary>
    public static CognitiveLoadLedgerId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CognitiveLoadLedgerId.</summary>
    public static CognitiveLoadLedgerId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CognitiveLoadLedgerId.</summary>
    public static CognitiveLoadLedgerId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
