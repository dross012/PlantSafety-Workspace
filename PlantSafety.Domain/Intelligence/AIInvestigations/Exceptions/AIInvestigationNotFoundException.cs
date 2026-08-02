// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AIInvestigation aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Exceptions;

/// <summary>Thrown when an AIInvestigation aggregate lookup finds no match.</summary>
public sealed class AIInvestigationNotFoundException : AIInvestigationDomainException
{
    /// <summary>Creates a not-found exception for the given AIInvestigation identity.</summary>
    public AIInvestigationNotFoundException(AIInvestigationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AIInvestigation with id '{id}' was not found.",
            "AIInvestigation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
