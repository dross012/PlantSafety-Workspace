// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyStopDevices lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyStopDevices"/> lifecycle transition is not permitted.</summary>
public sealed class EmergencyStopDevicesInvalidStatusTransitionException : EmergencyStopDevicesDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EmergencyStopDevicesInvalidStatusTransitionException(
        EmergencyStopDevicesId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'EmergencyStopDevices' {id} from '{from}' to '{to}'.",
            "EmergencyStopDevices.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}