// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RootCauseAnalysis aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Exceptions;

/// <summary>Thrown when a RootCauseAnalysis aggregate lookup finds no match.</summary>
public sealed class RootCauseAnalysisNotFoundException : RootCauseAnalysisDomainException
{
    /// <summary>Creates a not-found exception for the given RootCauseAnalysis identity.</summary>
    public RootCauseAnalysisNotFoundException(RootCauseAnalysisId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"RootCauseAnalysis with id '{id}' was not found.",
            "RootCauseAnalysis.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
