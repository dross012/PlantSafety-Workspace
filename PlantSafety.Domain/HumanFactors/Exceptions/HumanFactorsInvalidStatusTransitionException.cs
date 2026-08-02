// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HumanFactors lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Exceptions;

/// <summary>Thrown when a <see cref="HumanFactors"/> lifecycle transition is not permitted.</summary>
public sealed class HumanFactorsInvalidStatusTransitionException : HumanFactorsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HumanFactorsInvalidStatusTransitionException(
        HumanFactorsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'HumanFactors' {id} from '{from}' to '{to}'.",
            "HumanFactors.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
