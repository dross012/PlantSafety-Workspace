// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPETrainingFit lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Exceptions;

/// <summary>Thrown when a <see cref="PPETrainingFit"/> lifecycle transition is not permitted.</summary>
public sealed class PPETrainingFitInvalidStatusTransitionException : PPETrainingFitDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PPETrainingFitInvalidStatusTransitionException(
        PPETrainingFitId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'PPETrainingFit' {id} from '{from}' to '{to}'.",
            "PPETrainingFit.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}