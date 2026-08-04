// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Carrier aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Factories;

/// <summary>Default factory implementation for constructing <see cref="Carrier"/> aggregates.</summary>
public sealed class CarrierFactory : ICarrierFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
