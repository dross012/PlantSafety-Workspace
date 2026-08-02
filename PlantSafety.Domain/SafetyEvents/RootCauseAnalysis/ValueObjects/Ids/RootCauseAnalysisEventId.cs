// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for RootCauseAnalysis domain events.
//                  Mirrors PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids.PermitsEventId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for RootCauseAnalysis domain events.</summary>
public readonly record struct RootCauseAnalysisEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RootCauseAnalysisEventId wrapping a non-empty GUID.</summary>
    public RootCauseAnalysisEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RootCauseAnalysisEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RootCauseAnalysisEventId using Uuid7.</summary>
    public static RootCauseAnalysisEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
