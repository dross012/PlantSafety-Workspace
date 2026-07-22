// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           IPPEAssignmentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPEAssignment aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPEAssignment"/> aggregates.</summary>
public interface IPPEAssignmentFactory
{
    /// <summary>Creates a new PPEAssignment aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}