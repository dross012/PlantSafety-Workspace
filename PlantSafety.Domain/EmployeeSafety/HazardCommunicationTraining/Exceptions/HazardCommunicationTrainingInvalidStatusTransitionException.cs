// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardCommunicationTraining lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Exceptions;

/// <summary>Thrown when a <see cref="HazardCommunicationTraining"/> lifecycle transition is not permitted.</summary>
public sealed class HazardCommunicationTrainingInvalidStatusTransitionException : HazardCommunicationTrainingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazardCommunicationTrainingInvalidStatusTransitionException(
        HazardCommunicationTrainingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'HazardCommunicationTraining' {id} from '{from}' to '{to}'.",
            "HazardCommunicationTraining.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}