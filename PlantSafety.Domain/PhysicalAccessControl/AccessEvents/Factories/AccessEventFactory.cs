// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AccessEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Factories;

/// <summary>Default factory implementation for constructing <see cref="AccessEvent"/> aggregates.</summary>
public sealed class AccessEventFactory : IAccessEventFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
