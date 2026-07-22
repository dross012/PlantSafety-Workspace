// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EmergencyPreparedness aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Factories;

/// <summary>Default factory implementation for constructing <see cref="EmergencyPreparedness"/> aggregates.</summary>
public sealed class EmergencyPreparednessFactory : IEmergencyPreparednessFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}