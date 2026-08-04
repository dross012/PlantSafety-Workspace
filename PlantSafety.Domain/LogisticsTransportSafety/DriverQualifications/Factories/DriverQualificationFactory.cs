// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing DriverQualification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Factories;

/// <summary>Default factory implementation for constructing <see cref="DriverQualification"/> aggregates.</summary>
public sealed class DriverQualificationFactory : IDriverQualificationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
