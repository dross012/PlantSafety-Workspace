// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           ISafetyZoneFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SafetyZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;

/// <summary>Factory interface for constructing <see cref="SafetyZone"/> aggregates.</summary>
public interface ISafetyZoneFactory
{
    /// <summary>Creates a new SafetyZone aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
