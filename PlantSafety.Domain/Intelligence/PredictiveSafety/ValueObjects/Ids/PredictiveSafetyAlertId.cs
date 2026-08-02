// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Intelligence.PredictiveSafety
//                  PredictiveSafetyAlert aggregate root. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids.RCAId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Intelligence.PredictiveSafety PredictiveSafetyAlert aggregate root.</summary>
public readonly record struct PredictiveSafetyAlertId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveSafetyAlertId wrapping a non-empty GUID.</summary>
    public PredictiveSafetyAlertId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveSafetyAlertId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveSafetyAlertId using Uuid7.</summary>
    public static PredictiveSafetyAlertId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PredictiveSafetyAlertId.</summary>
    public static PredictiveSafetyAlertId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
