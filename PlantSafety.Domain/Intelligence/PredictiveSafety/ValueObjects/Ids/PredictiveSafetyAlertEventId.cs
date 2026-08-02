// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertEventId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Intelligence.PredictiveSafety domain
//                  events. Mirrors PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids
//                  .RCAEventId's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Intelligence.PredictiveSafety domain events.</summary>
public readonly record struct PredictiveSafetyAlertEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PredictiveSafetyAlertEventId wrapping a non-empty GUID.</summary>
    public PredictiveSafetyAlertEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PredictiveSafetyAlertEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PredictiveSafetyAlertEventId using Uuid7.</summary>
    public static PredictiveSafetyAlertEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
