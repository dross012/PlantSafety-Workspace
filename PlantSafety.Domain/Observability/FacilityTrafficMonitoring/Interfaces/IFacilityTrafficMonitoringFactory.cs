// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           IFacilityTrafficMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing FacilityTrafficMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Interfaces;

/// <summary>Factory interface for constructing <see cref="FacilityTrafficMonitoring"/> aggregates.</summary>
public interface IFacilityTrafficMonitoringFactory
{
    /// <summary>Creates a new FacilityTrafficMonitoring aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}