// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyCaseValidation lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.SafetyCaseValidation"/> lifecycle transition is not permitted.</summary>
public sealed class SafetyCaseValidationInvalidStatusTransitionException : SafetyCaseValidationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SafetyCaseValidationInvalidStatusTransitionException(
        SafetyCaseValidationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'SafetyCaseValidation' {id} from '{from}' to '{to}'.",
            "SafetyCaseValidation.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
