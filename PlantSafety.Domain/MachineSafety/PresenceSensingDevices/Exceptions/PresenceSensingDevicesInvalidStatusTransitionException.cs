// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PresenceSensingDevices lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Exceptions;

/// <summary>Thrown when a <see cref="PresenceSensingDevices"/> lifecycle transition is not permitted.</summary>
public sealed class PresenceSensingDevicesInvalidStatusTransitionException : PresenceSensingDevicesDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PresenceSensingDevicesInvalidStatusTransitionException(
        PresenceSensingDevicesId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PresenceSensingDevices' {id} from '{from}' to '{to}'.",
            "PresenceSensingDevices.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}