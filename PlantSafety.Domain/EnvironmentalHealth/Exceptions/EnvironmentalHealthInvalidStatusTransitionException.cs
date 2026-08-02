// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EnvironmentalHealth lifecycle transition is not permitted.
//                  Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Exceptions;

/// <summary>Thrown when a <see cref="EnvironmentalHealth"/> lifecycle transition is not permitted.</summary>
public sealed class EnvironmentalHealthInvalidStatusTransitionException : EnvironmentalHealthDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EnvironmentalHealthInvalidStatusTransitionException(
        EnvironmentalHealthId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EnvironmentalHealth' {id} from '{from}' to '{to}'.",
            "EnvironmentalHealth.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
