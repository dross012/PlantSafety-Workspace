// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyCaseValidation aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.SafetyCaseValidation"/> aggregate lookup finds no match.</summary>
public sealed class SafetyCaseValidationNotFoundException : SafetyCaseValidationDomainException
{
    /// <summary>Creates a not-found exception for the given SafetyCaseValidation identity.</summary>
    public SafetyCaseValidationNotFoundException(SafetyCaseValidationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"SafetyCaseValidation with id '{id}' was not found.",
            "SafetyCaseValidation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
