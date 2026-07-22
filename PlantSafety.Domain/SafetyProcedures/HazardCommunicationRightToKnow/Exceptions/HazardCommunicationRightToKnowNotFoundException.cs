// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardCommunicationRightToKnow aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Exceptions;

/// <summary>Thrown when a <see cref="HazardCommunicationRightToKnow"/> aggregate lookup finds no match.</summary>
public sealed class HazardCommunicationRightToKnowNotFoundException : HazardCommunicationRightToKnowDomainException
{
    /// <summary>Creates a not-found exception for the given HazardCommunicationRightToKnow identity.</summary>
    public HazardCommunicationRightToKnowNotFoundException(HazardCommunicationRightToKnowId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "HazardCommunicationRightToKnow with id '{id}' was not found.",
            "HazardCommunicationRightToKnow.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}