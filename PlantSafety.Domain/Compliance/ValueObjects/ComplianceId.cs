// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance
//  Layer:          Domain
//  File:           ComplianceId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for top-level Compliance records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for top-level Compliance records.</summary>
public readonly record struct ComplianceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a ComplianceId wrapping a non-empty GUID.</summary>
    public ComplianceId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("ComplianceId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new ComplianceId using Uuid7.</summary>
    public static ComplianceId New() => new(Uuid7.New());
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
