// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           IEmergencyStopDevicesFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EmergencyStopDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces;

/// <summary>Factory interface for constructing <see cref="EmergencyStopDevices"/> aggregates.</summary>
public interface IEmergencyStopDevicesFactory
{
    /// <summary>Creates a new EmergencyStopDevices aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}