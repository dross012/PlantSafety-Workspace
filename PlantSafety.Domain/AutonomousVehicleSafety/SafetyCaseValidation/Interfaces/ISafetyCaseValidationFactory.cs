// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           ISafetyCaseValidationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SafetyCaseValidation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.SafetyCaseValidation"/> aggregates.</summary>
public interface ISafetyCaseValidationFactory
{
    /// <summary>Creates a new SafetyCaseValidation aggregate with the supplied parameters.</summary>
    Aggregates.SafetyCaseValidation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
