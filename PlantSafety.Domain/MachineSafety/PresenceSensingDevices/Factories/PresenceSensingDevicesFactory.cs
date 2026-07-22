// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PresenceSensingDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Factories;

/// <summary>Default factory implementation for constructing <see cref="PresenceSensingDevices"/> aggregates.</summary>
public sealed class PresenceSensingDevicesFactory : IPresenceSensingDevicesFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}