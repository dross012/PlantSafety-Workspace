// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ChecklistCompletion module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ChecklistCompletion module.</summary>
public abstract class ChecklistCompletionDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ChecklistCompletionDomainException(
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