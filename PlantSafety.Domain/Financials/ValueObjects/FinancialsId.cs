// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for financial records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for financial records.</summary>
public readonly record struct FinancialsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a FinancialsId wrapping a non-empty GUID.</summary>
    public FinancialsId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("FinancialsId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new FinancialsId using Uuid7.</summary>
    public static FinancialsId New() => new(Uuid7.New());
    /// <summary>Wraps an existing GUID.</summary>
    public static FinancialsId FromGuid(Guid value) => new(value);
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
