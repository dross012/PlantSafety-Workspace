// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           IPresenceSensingDevicesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PresenceSensingDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces;

/// <summary>Factory interface for constructing <see cref="PresenceSensingDevices"/> aggregates.</summary>
public interface IPresenceSensingDevicesFactory
{
    /// <summary>Creates a new PresenceSensingDevices aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}