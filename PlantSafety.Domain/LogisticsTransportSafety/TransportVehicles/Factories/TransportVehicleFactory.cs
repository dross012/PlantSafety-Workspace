// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing TransportVehicle aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Factories;

/// <summary>Default factory implementation for constructing <see cref="TransportVehicle"/> aggregates.</summary>
public sealed class TransportVehicleFactory : ITransportVehicleFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
