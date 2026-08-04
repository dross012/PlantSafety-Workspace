// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AccessPoint aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Factories;

/// <summary>Default factory implementation for constructing <see cref="AccessPoint"/> aggregates.</summary>
public sealed class AccessPointFactory : IAccessPointFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
