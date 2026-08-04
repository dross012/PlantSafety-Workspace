// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           IAccessPointFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AccessPoint aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;

/// <summary>Factory interface for constructing <see cref="AccessPoint"/> aggregates.</summary>
public interface IAccessPointFactory
{
    /// <summary>Creates a new AccessPoint aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
