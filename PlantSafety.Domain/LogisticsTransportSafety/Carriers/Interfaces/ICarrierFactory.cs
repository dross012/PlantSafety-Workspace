// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           ICarrierFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Carrier aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces;

/// <summary>Factory interface for constructing <see cref="Carrier"/> aggregates.</summary>
public interface ICarrierFactory
{
    /// <summary>Creates a new Carrier aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
