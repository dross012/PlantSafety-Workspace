// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents
//  Layer:          Domain
//  File:           SafetyEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for top-level SafetyEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for top-level SafetyEvent records.</summary>
public readonly record struct SafetyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }
    /// <summary>Creates a SafetyEventId wrapping a non-empty GUID.</summary>
    public SafetyEventId(Guid value) { if (value == Guid.Empty) throw new ArgumentException("SafetyEventId must not be empty.", nameof(value)); Value = value; }
    /// <summary>Creates a new SafetyEventId using Uuid7.</summary>
    public static SafetyEventId New() => new(Uuid7.New());
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
