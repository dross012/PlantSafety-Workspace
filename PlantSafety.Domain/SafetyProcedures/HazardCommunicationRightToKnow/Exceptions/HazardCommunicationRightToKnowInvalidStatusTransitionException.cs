// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardCommunicationRightToKnow lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Exceptions;

/// <summary>Thrown when a <see cref="HazardCommunicationRightToKnow"/> lifecycle transition is not permitted.</summary>
public sealed class HazardCommunicationRightToKnowInvalidStatusTransitionException : HazardCommunicationRightToKnowDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazardCommunicationRightToKnowInvalidStatusTransitionException(
        HazardCommunicationRightToKnowId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'HazardCommunicationRightToKnow' {id} from '{from}' to '{to}'.",
            "HazardCommunicationRightToKnow.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}