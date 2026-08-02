// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCAEventId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for ContinuousImprovement RCA domain
//                  events. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids
//                  .RootCauseAnalysisEventId's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ContinuousImprovement RCA domain events.</summary>
public readonly record struct RCAEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RCAEventId wrapping a non-empty GUID.</summary>
    public RCAEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RCAEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RCAEventId using Uuid7.</summary>
    public static RCAEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
