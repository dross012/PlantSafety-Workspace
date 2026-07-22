// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPEAssignment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPEAssignment"/> aggregates.</summary>
public sealed class PPEAssignmentFactory : IPPEAssignmentFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}