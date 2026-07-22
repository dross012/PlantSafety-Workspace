// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPETrainingFit aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Exceptions;

/// <summary>Thrown when a <see cref="PPETrainingFit"/> aggregate lookup finds no match.</summary>
public sealed class PPETrainingFitNotFoundException : PPETrainingFitDomainException
{
    /// <summary>Creates a not-found exception for the given PPETrainingFit identity.</summary>
    public PPETrainingFitNotFoundException(PPETrainingFitId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPETrainingFit with id '{id}' was not found.",
            "PPETrainingFit.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}