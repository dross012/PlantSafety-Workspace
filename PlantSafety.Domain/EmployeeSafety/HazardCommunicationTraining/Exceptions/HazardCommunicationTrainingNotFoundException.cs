// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardCommunicationTraining aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Exceptions;

/// <summary>Thrown when a <see cref="HazardCommunicationTraining"/> aggregate lookup finds no match.</summary>
public sealed class HazardCommunicationTrainingNotFoundException : HazardCommunicationTrainingDomainException
{
    /// <summary>Creates a not-found exception for the given HazardCommunicationTraining identity.</summary>
    public HazardCommunicationTrainingNotFoundException(HazardCommunicationTrainingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "HazardCommunicationTraining with id '{id}' was not found.",
            "HazardCommunicationTraining.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}