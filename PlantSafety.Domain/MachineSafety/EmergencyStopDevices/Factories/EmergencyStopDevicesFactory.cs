// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EmergencyStopDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Factories;

/// <summary>Default factory implementation for constructing <see cref="EmergencyStopDevices"/> aggregates.</summary>
public sealed class EmergencyStopDevicesFactory : IEmergencyStopDevicesFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}