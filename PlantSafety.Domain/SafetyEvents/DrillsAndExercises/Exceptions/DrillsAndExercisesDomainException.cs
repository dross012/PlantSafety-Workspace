// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the SafetyEvents
//                  .DrillsAndExercises module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the SafetyEvents.DrillsAndExercises module.</summary>
public abstract class DrillsAndExercisesDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected DrillsAndExercisesDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}
