// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           ITransportVehicleFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing TransportVehicle aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces;

/// <summary>Factory interface for constructing <see cref="TransportVehicle"/> aggregates.</summary>
public interface ITransportVehicleFactory
{
    /// <summary>Creates a new TransportVehicle aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
