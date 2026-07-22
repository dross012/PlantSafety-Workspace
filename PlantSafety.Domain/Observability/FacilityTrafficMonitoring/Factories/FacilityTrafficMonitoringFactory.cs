// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing FacilityTrafficMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Factories;

/// <summary>Default factory implementation for constructing <see cref="FacilityTrafficMonitoring"/> aggregates.</summary>
public sealed class FacilityTrafficMonitoringFactory : IFacilityTrafficMonitoringFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}