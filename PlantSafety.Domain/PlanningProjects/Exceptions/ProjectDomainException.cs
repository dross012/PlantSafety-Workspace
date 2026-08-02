// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PlanningProjects module's
//                  Project aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Exceptions;

/// <summary>Abstract base for all domain exceptions raised by the Project aggregate.</summary>
public abstract class ProjectDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ProjectDomainException(
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
