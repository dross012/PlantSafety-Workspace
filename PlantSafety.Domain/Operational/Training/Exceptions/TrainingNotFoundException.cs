// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Exceptions;

/// <summary>Thrown when a <see cref="Training"/> aggregate lookup finds no match.</summary>
public sealed class TrainingNotFoundException : TrainingDomainException
{
    /// <summary>Creates a not-found exception for the given Training identity.</summary>
    public TrainingNotFoundException(TrainingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Training with id '{id}' was not found.",
            "Training.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
