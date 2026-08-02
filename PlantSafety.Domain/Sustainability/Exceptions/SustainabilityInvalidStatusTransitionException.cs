// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           SustainabilityInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Sustainability lifecycle transition is not permitted.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Exceptions;

/// <summary>Thrown when a <see cref="Sustainability"/> lifecycle transition is not permitted.</summary>
public sealed class SustainabilityInvalidStatusTransitionException : SustainabilityDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SustainabilityInvalidStatusTransitionException(
        SustainabilityId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Sustainability' {id} from '{from}' to '{to}'.",
            "Sustainability.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
