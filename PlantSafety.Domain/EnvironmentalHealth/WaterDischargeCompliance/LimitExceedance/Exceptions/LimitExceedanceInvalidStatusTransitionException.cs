// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LimitExceedance lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Exceptions;

/// <summary>Thrown when a <see cref="LimitExceedance"/> lifecycle transition is not permitted.</summary>
public sealed class LimitExceedanceInvalidStatusTransitionException : LimitExceedanceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public LimitExceedanceInvalidStatusTransitionException(
        LimitExceedanceId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'LimitExceedance' {id} from '{from}' to '{to}'.",
            "LimitExceedance.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}