// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AIInvestigation lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Exceptions;

/// <summary>Thrown when an AIInvestigation lifecycle transition is not permitted.</summary>
public sealed class AIInvestigationInvalidStatusTransitionException : AIInvestigationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AIInvestigationInvalidStatusTransitionException(
        AIInvestigationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition AIInvestigation '{id}' from '{from}' to '{to}'.",
            "AIInvestigation.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
