// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PredictiveSafetyAlert lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Exceptions;

/// <summary>Thrown when a PredictiveSafetyAlert lifecycle transition is not permitted.</summary>
public sealed class PredictiveSafetyAlertInvalidStatusTransitionException : PredictiveSafetyAlertDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PredictiveSafetyAlertInvalidStatusTransitionException(
        PredictiveSafetyAlertId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition PredictiveSafetyAlert '{id}' from '{from}' to '{to}'.",
            "PredictiveSafety.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
