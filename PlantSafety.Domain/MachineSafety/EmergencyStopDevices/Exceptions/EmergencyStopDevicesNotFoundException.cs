// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyStopDevices aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyStopDevices"/> aggregate lookup finds no match.</summary>
public sealed class EmergencyStopDevicesNotFoundException : EmergencyStopDevicesDomainException
{
    /// <summary>Creates a not-found exception for the given EmergencyStopDevices identity.</summary>
    public EmergencyStopDevicesNotFoundException(EmergencyStopDevicesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "EmergencyStopDevices with id '{id}' was not found.",
            "EmergencyStopDevices.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}