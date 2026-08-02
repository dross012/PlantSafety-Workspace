// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCANotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an RCA aggregate lookup finds no match. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Exceptions
//                  .RootCauseAnalysisNotFoundException. Not currently thrown by any
//                  Application-layer handler (which prefer returning an ApplicationError with
//                  ErrorSeverity.NotFound so failures flow through the standard Result<T> pipeline
//                  instead of exceptions) -- provided for symmetry with the sibling module and for
//                  any future caller that operates directly against the domain repository outside
//                  of a MediatR handler. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Exceptions;

/// <summary>Thrown when an RCA aggregate lookup finds no match.</summary>
public sealed class RCANotFoundException : RCADomainException
{
    /// <summary>Creates a not-found exception for the given RCA identity.</summary>
    public RCANotFoundException(RCAId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"RCA with id '{id}' was not found.",
            "RCA.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
