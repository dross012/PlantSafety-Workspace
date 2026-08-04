// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EvacuationRoute aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.EvacuationRoute"/> aggregates.</summary>
public sealed class EvacuationRouteFactory : Interfaces.IEvacuationRouteFactory
{
    /// <inheritdoc/>
    public Aggregates.EvacuationRoute Create(
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
        int estimatedEgressTimeSeconds)
        => Aggregates.EvacuationRoute.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, routeName, zoneId, assemblyPointId, pathGeometryRef, twinModelVersion, estimatedEgressTimeSeconds);
}
