// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCAId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ContinuousImprovement RCA
//                  aggregate root. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids
//                  .RootCauseAnalysisId's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

/// <summary>Strong-typed identity for the ContinuousImprovement RCA aggregate root.</summary>
public readonly record struct RCAId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RCAId wrapping a non-empty GUID.</summary>
    public RCAId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RCAId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RCAId using Uuid7.</summary>
    public static RCAId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RCAId.</summary>
    public static RCAId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
