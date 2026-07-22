// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AutomatedIncidentDetection lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Exceptions;

/// <summary>Thrown when a <see cref="AutomatedIncidentDetection"/> lifecycle transition is not permitted.</summary>
public sealed class AutomatedIncidentDetectionInvalidStatusTransitionException : AutomatedIncidentDetectionDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AutomatedIncidentDetectionInvalidStatusTransitionException(
        AutomatedIncidentDetectionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AutomatedIncidentDetection' {id} from '{from}' to '{to}'.",
            "AutomatedIncidentDetection.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}