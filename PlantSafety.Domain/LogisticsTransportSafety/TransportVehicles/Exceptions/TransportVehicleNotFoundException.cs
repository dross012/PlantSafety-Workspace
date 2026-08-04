// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a TransportVehicle aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Exceptions;

/// <summary>Thrown when a <see cref="TransportVehicle"/> aggregate lookup finds no match.</summary>
public sealed class TransportVehicleNotFoundException : TransportVehicleDomainException
{
    /// <summary>Creates a not-found exception for the given TransportVehicle identity.</summary>
    public TransportVehicleNotFoundException(TransportVehicleId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"TransportVehicle with id '{id}' was not found.",
            "TransportVehicle.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
