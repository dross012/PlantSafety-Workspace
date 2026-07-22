// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PresenceSensingDevices aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Exceptions;

/// <summary>Thrown when a <see cref="PresenceSensingDevices"/> aggregate lookup finds no match.</summary>
public sealed class PresenceSensingDevicesNotFoundException : PresenceSensingDevicesDomainException
{
    /// <summary>Creates a not-found exception for the given PresenceSensingDevices identity.</summary>
    public PresenceSensingDevicesNotFoundException(PresenceSensingDevicesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PresenceSensingDevices with id '{id}' was not found.",
            "PresenceSensingDevices.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}