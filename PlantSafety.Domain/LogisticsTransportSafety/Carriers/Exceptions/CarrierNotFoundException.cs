// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Carrier aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Exceptions;

/// <summary>Thrown when a <see cref="Carrier"/> aggregate lookup finds no match.</summary>
public sealed class CarrierNotFoundException : CarrierDomainException
{
    /// <summary>Creates a not-found exception for the given Carrier identity.</summary>
    public CarrierNotFoundException(CarrierId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Carrier with id '{id}' was not found.",
            "Carrier.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
