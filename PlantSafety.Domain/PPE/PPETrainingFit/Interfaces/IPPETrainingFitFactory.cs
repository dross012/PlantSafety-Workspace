// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           IPPETrainingFitFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPETrainingFit aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPETrainingFit"/> aggregates.</summary>
public interface IPPETrainingFitFactory
{
    /// <summary>Creates a new PPETrainingFit aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}