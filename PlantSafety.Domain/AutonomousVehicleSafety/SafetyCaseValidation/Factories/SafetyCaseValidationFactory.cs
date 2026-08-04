// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SafetyCaseValidation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.SafetyCaseValidation"/> aggregates.</summary>
public sealed class SafetyCaseValidationFactory : ISafetyCaseValidationFactory
{
    /// <inheritdoc/>
    public Aggregates.SafetyCaseValidation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.SafetyCaseValidation.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
