// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AdvancedMetricsLedger aggregate
//                  root (per tenant/site/reporting-period ESG metrics ledger).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AdvancedMetricsLedger aggregate root.</summary>
public readonly record struct AdvancedMetricsLedgerId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AdvancedMetricsLedgerId wrapping a non-empty GUID.</summary>
    public AdvancedMetricsLedgerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AdvancedMetricsLedgerId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AdvancedMetricsLedgerId using Uuid7.</summary>
    public static AdvancedMetricsLedgerId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AdvancedMetricsLedgerId.</summary>
    public static AdvancedMetricsLedgerId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AdvancedMetricsLedgerId.</summary>
    public static AdvancedMetricsLedgerId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
