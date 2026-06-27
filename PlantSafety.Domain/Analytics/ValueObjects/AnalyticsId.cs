// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics
//  Layer:          Domain
//  File:           AnalyticsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for top-level Analytics records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for top-level Analytics records.</summary>
public readonly record struct AnalyticsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates an AnalyticsId wrapping a non-empty GUID.</summary>
    public AnalyticsId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("AnalyticsId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new AnalyticsId using Uuid7.</summary>
    public static AnalyticsId New() => new(Uuid7.New());
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
