// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing LoadSecurementCheck aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Factories;

/// <summary>Default factory implementation for constructing <see cref="LoadSecurementCheck"/> aggregates.</summary>
public sealed class LoadSecurementCheckFactory : ILoadSecurementCheckFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
