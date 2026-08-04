// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           IEvacuationRouteFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EvacuationRoute aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;

/// <summary>Factory interface for constructing <see cref="EvacuationRoute"/> aggregates.</summary>
public interface IEvacuationRouteFactory
{
    /// <summary>Defines a new EvacuationRoute aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string routeName,
        EvacuationZoneId zoneId,
        AssemblyPointId assemblyPointId,
        string pathGeometryRef,
        string twinModelVersion,
        int estimatedEgressTimeSeconds);
}
