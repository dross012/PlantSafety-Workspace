// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SafetyZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Factories;

/// <summary>Default factory implementation for constructing <see cref="SafetyZone"/> aggregates.</summary>
public sealed class SafetyZoneFactory : ISafetyZoneFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
