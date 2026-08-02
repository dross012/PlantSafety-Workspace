// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RootCauseAnalysis lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Exceptions;

/// <summary>Thrown when a RootCauseAnalysis lifecycle transition is not permitted.</summary>
public sealed class RootCauseAnalysisInvalidStatusTransitionException : RootCauseAnalysisDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RootCauseAnalysisInvalidStatusTransitionException(
        RootCauseAnalysisId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition RootCauseAnalysis '{id}' from '{from}' to '{to}'.",
            "RootCauseAnalysis.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
