// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AutomatedIncidentDetection aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Exceptions;

/// <summary>Thrown when a <see cref="AutomatedIncidentDetection"/> aggregate lookup finds no match.</summary>
public sealed class AutomatedIncidentDetectionNotFoundException : AutomatedIncidentDetectionDomainException
{
    /// <summary>Creates a not-found exception for the given AutomatedIncidentDetection identity.</summary>
    public AutomatedIncidentDetectionNotFoundException(AutomatedIncidentDetectionId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AutomatedIncidentDetection with id '{id}' was not found.",
            "AutomatedIncidentDetection.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}