// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           IAccessAnomalyFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AccessAnomaly aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;

/// <summary>Factory interface for constructing <see cref="AccessAnomaly"/> aggregates.</summary>
public interface IAccessAnomalyFactory
{
    /// <summary>Creates a new AccessAnomaly aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
