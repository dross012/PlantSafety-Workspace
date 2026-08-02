// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the RootCauseAnalysis aggregate root.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (RootCauseAnalysisModuleMarker) and a
//                  read-only query slice over a "RootCauseAnalysis" table existed. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids.PermitsId.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;

/// <summary>Strong-typed identity for the RootCauseAnalysis aggregate root.</summary>
public readonly record struct RootCauseAnalysisId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RootCauseAnalysisId wrapping a non-empty GUID.</summary>
    public RootCauseAnalysisId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RootCauseAnalysisId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RootCauseAnalysisId using Uuid7.</summary>
    public static RootCauseAnalysisId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RootCauseAnalysisId.</summary>
    public static RootCauseAnalysisId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
