// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           IAccessEventFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AccessEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;

/// <summary>Factory interface for constructing <see cref="AccessEvent"/> aggregates.</summary>
public interface IAccessEventFactory
{
    /// <summary>Creates a new AccessEvent aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
